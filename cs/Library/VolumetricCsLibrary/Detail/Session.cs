// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable

namespace Microsoft.MixedReality.Volumetric.Detail
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;
    using static Detail.Api;

    internal sealed partial class Session
    {
        // 200ms timeout in nanoseconds for vaWaitEvent.
        // This allows periodic checking for pending callbacks (e.g., late OnStart/OnReady subscriptions)
        // even when no platform events are arriving. The timeout ensures the app remains responsive
        // to late subscriptions without spinning the CPU polling for events.
        private const VaDuration WaitEventTimeoutNs = (VaDuration)200_000_000;

        private readonly VolumetricApp _vapp;
        private readonly IntPtr _handle;
        private readonly SessionExtensions _extensions;
        private VaSystemId _systemId;
        private object _volumesLock = new object();
        private List<Volume> _volumes = new List<Volume>();
        private VaEventDataBuffer _eventData = new VaEventDataBuffer() { type = VaStructureType.VA_TYPE_EVENT_DATA_BUFFER };

        private int _isDestroyed;    // Atomic flag that will only be set 1 once in Destroy() and otherwise will remain 0.

        internal Session(VolumetricApp vapp, IntPtr handle, SessionExtensions extensionsContext)
        {
            _vapp = vapp;
            _handle = handle;
            _extensions = extensionsContext;
        }

        public IntPtr Handle => _handle;
        public VaSystemId SystemId => _systemId;
        public SessionExtensions Extensions => _extensions;

        public event Action<Session>? OnSystemChanged;
        public event Action<Session>? OnSessionStopped;

        // Return true to indicate the event has been preprocessed and should not be processed further.
        public event Func<VaEventDataBuffer, bool>? PreprocessEvent;

        internal static IntPtr CreateHandle(
            PFN_vaCreateSession vaCreateSession,
            string appName,
            VaVersion apiVersion,
            IReadOnlyList<string> extensions,
            VaVolumeRestoreBehaviorExt volumeRestoreBehavior,
            VaSessionWaitForSystemBehavior waitForSystemBehavior)
        {
            IntPtr session = IntPtr.Zero;
            IntPtr restoreBehaviorPtr = IntPtr.Zero;

            var createInfo = new VaSessionCreateInfo();
            createInfo.type = Api.VaStructureType.VA_TYPE_SESSION_CREATE_INFO;
            createInfo.applicationInfo.apiVersion = apiVersion;
            createInfo.applicationInfo.applicationName = appName;
            createInfo.applicationInfo.libraryName = "Volumetric C# Library";
            createInfo.waitForSystemBehavior = waitForSystemBehavior;
            createInfo.enabledExtensionCount = (uint)extensions.Count;
            createInfo.enabledExtensionNames = Marshal.AllocHGlobal(Marshal.SizeOf<IntPtr>() * extensions.Count);

            try
            {
                var p = createInfo.enabledExtensionNames;
                for (int i = 0; i < extensions.Count; i++)
                {
                    var pName = Marshal.StringToHGlobalAnsi(extensions[i]);
                    Marshal.WriteIntPtr(p, pName);
                    p = checked((IntPtr)(p.ToInt64() + Marshal.SizeOf<IntPtr>()));
                }

                var restoreBehaviorExt = new VaSessionCreateWithVolumeRestoreBehaviorExt
                {
                    type = Api.VaStructureType.VA_TYPE_SESSION_CREATE_WITH_VOLUME_RESTORE_BEHAVIOR_EXT,
                    restoreBehavior = volumeRestoreBehavior
                };

                restoreBehaviorPtr = Marshal.AllocHGlobal(Marshal.SizeOf(restoreBehaviorExt));
                Marshal.StructureToPtr(restoreBehaviorExt, restoreBehaviorPtr, false);
                createInfo.next = restoreBehaviorPtr;

                Api.CheckResult(vaCreateSession(createInfo, out session));
                Trace.LogInfo(() => $"vaCreateSession: instance = {session}");

                // free marshalling data
                p = createInfo.enabledExtensionNames;
                for (int i = 0; i < extensions.Count; i++)
                {
                    var pName = Marshal.ReadIntPtr(p);
                    Marshal.FreeHGlobal(pName);
                    p = checked((IntPtr)(p.ToInt64() + Marshal.SizeOf<IntPtr>()));
                }
            }
            finally
            {
                Marshal.FreeHGlobal(createInfo.enabledExtensionNames);
                Marshal.FreeHGlobal(restoreBehaviorPtr);
            }

            return session;
        }

        public Volume AddVolume(Volume volume)
        {
            lock (_volumesLock)
            {
                _volumes.Add(volume);
            }
            return volume;
        }

        public void RemoveVolume(Volume volume)
        {
            lock (_volumesLock)
            {
                _volumes.Remove(volume);
            }
        }

        public Volume[] ActiveVolumes()
        {
            lock (_volumesLock)
            {
                return _volumes.Where(v => !v.IsClosed).ToArray();
            }
        }

        public void ProcessEventsThenWait()
        {
            var result = WaitEvent(WaitEventTimeoutNs, ref _eventData);
            if (result == VaResult.VA_SUCCESS)
            {
                ProcessEvent(_eventData);
            }
            // VA_TIMEOUT_EXPIRED: eventData is not valid, nothing to process.

            // Always check for pending callbacks (late onStart/onReady subscriptions)
            // after processing events, regardless of whether any events were available.
            ProcessPendingCallbacks();
        }

        public void ProcessEventsThenYield()
        {
            while (true)
            {
                if (PollEvent(ref _eventData))
                {
                    ProcessEvent(_eventData);

                    if (_eventData.type == VaStructureType.VA_TYPE_EVENT_UPDATE_VOLUME)
                    {
                        break;  // yield on every volume update
                    }
                }
                else
                {
                    break;  // no more events, yield.
                }
            }

            // Always check for pending callbacks (late onStart/onReady subscriptions)
            // after processing events, regardless of whether any events were available.
            ProcessPendingCallbacks();
        }

        private void ProcessEvent(in VaEventDataBuffer eventData)
        {
            if (PreprocessEvent != null)
            {
                bool isPreprocessed = PreprocessEvent.Invoke(eventData);
                if (isPreprocessed)
                {
                    return;
                }
            }

            switch (eventData.type)
            {
                case Api.VaStructureType.VA_TYPE_EVENT_CONNECTED_SYSTEM_CHANGED:
                    var systemChanged = VaEventConvert<VaEventConnectedSystemChanged>(eventData);
                    ProcessConnectedSystemChanged(systemChanged);
                    break;
                case Api.VaStructureType.VA_TYPE_EVENT_VOLUME_STATE_CHANGED:
                    var volumeStateChanged = VaEventConvert<VaEventVolumeStateChanged>(eventData);
                    GetVolumeOrThrow(volumeStateChanged.volume).HandleVolumeStateChanged(volumeStateChanged);
                    break;
                case Api.VaStructureType.VA_TYPE_EVENT_UPDATE_VOLUME:
                    var updateVolume = VaEventConvert<VaEventUpdateVolume>(eventData);
                    GetVolumeOrThrow(updateVolume.volume).HandleUpdate(updateVolume.frameId);
                    break;
                case Api.VaStructureType.VA_TYPE_EVENT_SESSION_STOPPED:
                    OnSessionStopped?.Invoke(this);
                    break;
                case Api.VaStructureType.VA_TYPE_EVENT_ELEMENT_ASYNC_STATE_CHANGED:
                    var elementAsyncStateChanged = VaEventConvert<VaEventElementAsyncStateChanged>(eventData);
                    GetVolumeOrThrow(elementAsyncStateChanged.volume).HandleElementAsyncStateChanged(elementAsyncStateChanged);
                    break;
                case Api.VaStructureType.VA_TYPE_EVENT_ADAPTIVE_CARD_ACTION_INVOKED_EXT:
                    var adaptiveCardActionInvoked = VaEventConvert<VaEventAdaptiveCardActionInvokedExt>(eventData);
                    GetVolumeOrThrow(adaptiveCardActionInvoked.volume).HandleAdaptiveCardActionInvoked(adaptiveCardActionInvoked);
                    break;
                case Api.VaStructureType.VA_TYPE_EVENT_VOLUME_CONTAINER_MODE_CHANGED_EXT:
                    var containerModeChanged = VaEventConvert<VaEventVolumeContainerModeChangedExt>(eventData);
                    GetVolumeOrThrow(containerModeChanged.volume).Container.HandleContainerModeChanged(in containerModeChanged);
                    break;
                case Api.VaStructureType.VA_TYPE_EVENT_VOLUME_RESTORE_RESULT_EXT:
                    var restoreResult = VaEventConvert<VaEventVolumeRestoreResultExt>(eventData);
                    GetVolumeOrThrow(restoreResult.volume).HandleRestoreResult(restoreResult);
                    break;
                case Api.VaStructureType.VA_TYPE_EVENT_VOLUME_RESTORE_REQUEST_EXT:
                    var restoreRequest = VaEventConvert<VaEventVolumeRestoreRequestExt>(eventData);
                    _vapp.HandleVolumeRestoreRequest(restoreRequest);
                    break;
                case Api.VaStructureType.VA_TYPE_EVENT_VOLUME_RESTORE_ID_INVALIDATED_EXT:
                    var restoreIdInvalidated = VaEventConvert<VaEventVolumeRestoreIdInvalidatedExt>(eventData);
                    _vapp.HandleVolumeRestoreIdInvalidated(restoreIdInvalidated);
                    break;
                default:
                    string msg = $"VaEventDataBuffer: type = {eventData.type}";
                    Trace.LogInfo(() => msg);
                    break;
            }
        }

        private bool PollEvent(ref VaEventDataBuffer buffer)
        {
            buffer.type = Api.VaStructureType.VA_TYPE_EVENT_DATA_BUFFER;
            var result = Api.CheckResult(Api.vaPollEvent(Handle, ref buffer));
            return result != VaResult.VA_EVENT_UNAVAILABLE;
        }

        private void WaitForNextEvent(ref VaEventDataBuffer buffer)
        {
            buffer.type = Api.VaStructureType.VA_TYPE_EVENT_DATA_BUFFER;
            Api.CheckResult(Api.vaWaitForNextEvent(Handle, ref buffer));
        }

        private VaResult WaitEvent(VaDuration timeout, ref VaEventDataBuffer buffer)
        {
            buffer.type = Api.VaStructureType.VA_TYPE_EVENT_DATA_BUFFER;
            var waitInfo = new VaEventWaitInfo
            {
                type = Api.VaStructureType.VA_TYPE_EVENT_WAIT_INFO,
                timeout = timeout
            };
            return Api.CheckResult(Api.vaWaitEvent(Handle, waitInfo, ref buffer));
        }

        private void ProcessPendingCallbacks()
        {
            // Check for late app OnStart subscription
            _vapp.TryDispatchPendingOnStart();

            // Check for late volume OnReady and OnRestoreResult subscriptions
            foreach (var volume in ActiveVolumes())
            {
                volume.TryDispatchPendingOnRestoreResult();
                volume.TryDispatchPendingOnReady();
            }
        }

        private void ProcessConnectedSystemChanged(VaEventConnectedSystemChanged systemChanged)
        {
            if (_systemId != systemChanged.systemId)
            {
                _systemId = systemChanged.systemId;
                this.OnSystemChanged?.Invoke(this);
            }
        }

        private Volume GetVolumeOrThrow(IntPtr handle)
        {
            lock (this)
            {
                Volume? volume;
                lock (_volumesLock)
                {
                    volume = _volumes.Find(v => v.Handle == handle);
                }

                if (volume is null)
                {
                    throw new ArgumentException("Invalid volume");
                }
                return volume;
            }
        }

        public void Destroy()
        {
            if (Interlocked.CompareExchange(ref _isDestroyed, 1, 0) == 0)
            {
                lock (_volumesLock)
                {
                    _volumes.ForEach(volume => volume.Destroy());
                    _volumes.Clear();
                }

                if (_handle != IntPtr.Zero)
                {
                    Api.vaDestroySession(_handle);
                }
            }
        }

        internal void RequestStop()
        {
            Detail.Api.CheckResult(Api.vaRequestStopSession(Handle));
        }
    }
}
