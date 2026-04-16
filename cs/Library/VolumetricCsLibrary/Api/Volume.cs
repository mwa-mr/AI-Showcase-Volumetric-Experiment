// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable

namespace Microsoft.MixedReality.Volumetric
{
    using Detail;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Api = Detail.Api;

    /// <summary>
    /// Describes the state of the volume frame, including the current frame time, and duration.
    /// </summary>
    public class FrameState
    {
        /// <summary>
        /// Gets whether the frame is current.
        /// It returns true if the frameId is from the current vaBeginUpdateVolume call
        /// and the vaEndUpdateVolume call has not yet been made.
        /// This is useful for determining if the frame is still being updated.
        /// </summary>
        public bool IsCurrent { get; internal set; }

        /// <summary>
        /// Gets the frame ID of the current frame if IsCurrent is true.
        /// This is the frame ID from the vaBeginUpdateVolume call.
        /// If IsCurrent is false, this value is from previous frame.
        /// </summary>
        public VaFrameId frameId { get; internal set; }

        /// <summary>
        /// Gets the time of the current frame.
        /// This is the time returned from the vaBeginUpdateVolume call.
        /// If IsCurrent is false, this value is from previous frame.
        /// This value is measured in nanoseconds.
        /// </summary>
        public VaTime frameTime { get; internal set; }

        /// <summary>
        /// Gets the duration of the current frame.
        /// This is the duration returned from the vaBeginUpdateVolume call.
        /// If IsCurrent is false, this value is from previous frame.
        /// This value is measured in nanoseconds.
        /// </summary>
        public VaDuration frameDuration { get; internal set; }
    };

    /// <summary>
    /// The Volume class represents 3D space that encapsulates visuals, dynamics, and data for spatial presentation and interaction.
    /// It is anlogous to a 2D Window on a screen, but in 3D mixed reality experiences.
    /// A Volume presents a container for user to interact with and manage placement in 3D space.
    /// It serves as a container for elements that can be added, updated, and removed.
    /// </summary>
    public class Volume
    {
        private readonly IntPtr _handle;
        internal IntPtr Handle => _handle;

        private readonly VolumetricApp _app;
        private readonly Session _session;

        private readonly object _elementsLock = new();
        private readonly List<Element> _elements = new();

        private VolumeContent? _contentElement;
        private VolumeContainer? _containerElement;

        private Api.VaVolumeState? _state;
        private readonly FrameState _frameState = new FrameState();
        private VaVolumeUpdateMode _requestedUpdateMode = VaVolumeUpdateMode.OnDemand;

        private bool _isReady;
        private bool _onReadyDispatched; // Track if OnReady was dispatched
        private Action<Volume>? _onReady;

        private bool _restoreResultReceived;
        private bool _onRestoreResultDispatched;
        private VaVolumeRestoredResultExt _restoreResultValue;
        private Action<Volume, VaVolumeRestoredResultExt>? _onRestoreResult;

        private int _isDestroyed;    // Atomic flag that will only be set 1 once in Destroy() and otherwise will remain 0.

        /// <summary>
        /// Creates a new Volume object in the specified VolumetricApp.
        /// This method starts the volume connection to the active system, therefore it will fail if there's no system connected.
        /// The volume creation can be successful when it starts the volume creation in platform,
        /// but the volume is not ready to be used until the OnReady event is raised.
        /// </summary>
        public Volume(VolumetricApp app, bool isRestorable = false, VaUuid? restoreId = null)
        {
            _app = app;
            _session = app.Session;
            _handle = Volume.CreateVolume(_app, _session.Handle, _session.SystemId, isRestorable, restoreId);
            _session.AddVolume(this);
        }

        /// <summary>
        /// Gets the VolumetricApp that this volume belongs to.
        /// This won't change after the volume is created.
        /// </summary>
        public VolumetricApp App => _app;

        /// <summary>
        /// The event that is raised once when the volume is ready to be used.
        /// This is raised for the very first frame after the volume is created.
        /// The application typically can start creating elements and set properties in this event.
        /// This event is guaranteed to be raised exactly once before any OnUpdate events.
        /// If subscribed after the volume is already running, the callback will be invoked on the next event processing cycle.
        /// </summary>
        public Action<Volume>? OnReady
        {
            get => _onReady;
            set
            {
                _onReady = value;
                if (_isReady && value != null && !_onReadyDispatched)
                {
                    _onReadyDispatched = true;
                    value.Invoke(this);
                }
            }
        }

        /// <summary>
        /// The event that is raised when the volume is updated.
        /// This is the update request for the application to set properties or manage elements.
        /// The very first frame update is raised in OnReady event.
        /// The application can setup the cadence of the volume update using RequestUpdate method.
        /// If the application does not call RequestUpdate, the volume will not be updated,
        /// except for the first frame that is always raised as OnReady event.
        /// </summary>
        public Action<Volume>? OnUpdate { get; set; }

        /// <summary>
        /// The event that is raised when the volume is paused.
        /// This can be raised when the volume is paused by the user or by the system.
        /// For example, when the user opens the volume management UI and therefore all volumes are hidden.
        /// There will be no update event raised for the volume while it is paused.
        /// </summary>
        public Action<Volume>? OnPause { get; set; }

        /// <summary>
        /// The event that is raised when the volume is resumed.
        /// This can be raised when the volume is resumed by the user or by the system.
        /// For example, when the user closes the volume management UI and therefore all volumes are visible again.
        /// Any outstanding update requests will be processed after the volume is resumed.
        /// </summary>
        public Action<Volume>? OnResume { get; set; }

        /// <summary>
        /// The event that is raised when the volume is closed.
        /// This can be raised when the volume is closed by the user or by the system.
        /// The application can also close the volume by calling RequestClose method.
        /// This event is raised right before the volume and all its elements are destroyed.
        /// The application should clean up any resources that are associated with the volume in this event.
        /// </summary>
        /// <remarks>
        /// For an volumetric only app that has no 2D window on desktop, it typically handle this event
        /// to detect if all volumes are closed and then call VolumetricApp.Quit() to exit the app.
        /// </remarks>
        public Action<Volume>? OnClose { get; set; }

        /// <summary>
        /// The event that is raised when the volume restore result is available.
        /// If subscribed after the restore result has already been received, the callback will be invoked immediately.
        /// </summary>
        public Action<Volume, VaVolumeRestoredResultExt>? OnRestoreResult
        {
            get => _onRestoreResult;
            set
            {
                _onRestoreResult = value;
                if (_restoreResultReceived && value != null && !_onRestoreResultDispatched)
                {
                    _onRestoreResultDispatched = true;
                    value.Invoke(this, _restoreResultValue);
                }
            }
        }

        /// <summary>
        /// Gets whether the volume is in running state.
        /// This is the state that the volume can be notified to be updated.
        /// The volume is in running state when the volume is ready and the system is connected.
        /// When the volume is not in running state, there will be no update request for the volume.
        /// The application should not set properties or call methods on the volume when it is not in running state,
        /// such as acquire mesh buffer functions that will throw exception if the volume is not in running state.
        /// </summary>
        public bool IsRunning => _state == Api.VaVolumeState.VA_VOLUME_STATE_RUNNING;

        /// <summary>
        /// Gets whether the volume is in closed state.
        /// This is the state that the volume is closed and all its elements are destroyed.
        /// The volume will not be functional or visible it is closed.
        /// The application can only call Destroy() method on the volume after it is closed.
        /// </summary>
        public bool IsClosed => _state == Api.VaVolumeState.VA_VOLUME_STATE_CLOSED;

        /// <summary>
        /// Gets the current state of the volume.
        /// </summary>
        public FrameState FrameState => _frameState;

        /// <summary>
        /// Destroys the volume and all its elements.
        /// This is typically managed by the library automatically when the volume is closed.
        /// The application should not call this method directly unless it is sure to manage resource across threads properly.
        /// Otherwise, the application should use RequestClose() method instead to close the volume.
        /// </summary>
        public void Destroy()
        {
            if (Interlocked.CompareExchange(ref _isDestroyed, 1, 0) == 0)
            {
                RemoveAllElements();
                _session.RemoveVolume(this);

                if (_handle != IntPtr.Zero)
                {
                    Api.vaDestroyVolume(_handle);
                }
            }
        }

        /// <summary>
        /// Requests the volume to be updated.
        /// If the application does not call this method, the volume will not be updated except for the first frame that is always raised as OnReady event.
        /// The application can setup the cadence of the volume update using this method.
        /// The application can also use it to request an OnDemand update that only happens once.
        /// The OnUpdate event will be raised by the platform when the volume should be updated.
        /// The update cadence and timing might be inprecise and allocated by the platform according to the system workload and policy.
        /// </summary>
        public void RequestUpdate(VaVolumeUpdateMode updateMode = VaVolumeUpdateMode.OnDemand)
        {
            var requestInfo = new Api.VaUpdateVolumeRequestInfo
            {
                type = Api.VaStructureType.VA_TYPE_UPDATE_VOLUME_REQUEST_INFO,
                updateMode = (Api.VaVolumeUpdateMode)updateMode,
            };
            var requestResult = new Api.VaUpdateVolumeRequestResult
            {
                type = Api.VaStructureType.VA_TYPE_UPDATE_VOLUME_REQUEST_RESULT,
            };
            Api.CheckResult(Api.vaRequestUpdateVolume(_handle, requestInfo, out requestResult));

            _requestedUpdateMode = updateMode;
        }

        /// <summary>
        /// Requests the volume to be updated OnDemand after a delay.
        /// The OnUpdate event will be raised by the platform when the volume should be updated after the delay.
        /// The update timing might be inprecise and allocated by the platform according to the system workload and policy.
        /// The application should not rely on this delay as precise timer.
        /// </summary>
        public void RequestUpdateAfter(TimeSpan delay)
        {
            VaDuration delayNs = (VaDuration)(delay.TotalMilliseconds * 1e6f); // Convert to nanoseconds
            if (delayNs <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(delay), delay, "Delay must be non-negative.");
            }

            var requestInfo = new Api.VaUpdateVolumeRequestInfo
            {
                type = Api.VaStructureType.VA_TYPE_UPDATE_VOLUME_REQUEST_INFO,
                updateMode = Api.VaVolumeUpdateMode.VA_VOLUME_UPDATE_MODE_ON_DEMAND,
                onDemandUpdateDelay = delayNs,
            };

            var requestResult = new Api.VaUpdateVolumeRequestResult
            {
                type = Api.VaStructureType.VA_TYPE_UPDATE_VOLUME_REQUEST_RESULT,
            };

            Api.CheckResult(Api.vaRequestUpdateVolume(_handle, requestInfo, out requestResult));

            _requestedUpdateMode = VaVolumeUpdateMode.OnDemand;
        }

        /// <summary>
        /// Requests the volume to be closed.
        /// The platform and the volume will clean up and prepare the closing
        /// and the OnClose event will be raised for the application to clean up any resources as needed.
        /// The application does not need to call Destroy() method after this.
        /// The volume will be destroyed automatically after the OnClose event is handled.
        /// </summary>
        public void RequestClose()
        {
            Api.CheckResult(Api.vaRequestCloseVolume(_handle));
        }

        /// <summary>
        /// Get the content element of the volume to modify properties for volume content behaviors.
        /// </summary>
        public VolumeContent Content { get { return _contentElement ??= new VolumeContent(this); } }

        /// <summary>
        /// Get the container element of the volume to modify properties for volume container behaviors.
        /// </summary>
        public VolumeContainer Container { get { return _containerElement ??= new VolumeContainer(this); } }

        /// <summary>
        /// Get the restore id of the volume.  This id is unique for each volume when the volume is configured to support restore.
        /// </summary>
        /// <returns>A valid Uuid for this volume, or zero uuid if the volume is not restorable.</returns>
        public VaUuid RestoreId
        {
            get
            {
                var restoreId = new VaUuid();
                Api.CheckResult(Api.vaGetVolumeRestoreIdExt(_handle, out restoreId));
                return restoreId;
            }
        }

        private static IntPtr CreateVolume(VolumetricApp app, IntPtr session, VaSystemId systemId, bool isRestorable, VaUuid? restoreId)
        {
            IntPtr handle = IntPtr.Zero;
            IntPtr restoreIdPtr = IntPtr.Zero;
            IntPtr restoreConfigPtr = IntPtr.Zero;

            try
            {

                if (restoreId.HasValue)
                {
                    var createWithRestoreId = new Api.VaVolumeCreateWithRestoreIdExt
                    {
                        type = Api.VaStructureType.VA_TYPE_VOLUME_CREATE_WITH_RESTORE_ID_EXT,
                        volumeRestoreId = restoreId.Value,
                    };

                    restoreIdPtr = Marshal.AllocHGlobal(Marshal.SizeOf(createWithRestoreId));
                    Marshal.StructureToPtr(createWithRestoreId, restoreIdPtr, false);
                }

                var restoreConfig = new Api.VaVolumeCreateWithRestoreConfigExt
                {
                    type = Api.VaStructureType.VA_TYPE_VOLUME_CREATE_WITH_RESTORE_CONFIG_EXT,
                    next = restoreIdPtr,
                    restorable = (VaBool32)(isRestorable ? 1 : 0),
                };

                restoreConfigPtr = Marshal.AllocHGlobal(Marshal.SizeOf(restoreConfig));
                Marshal.StructureToPtr(restoreConfig, restoreConfigPtr, false);

                var vaCreateInfo = new Api.VaVolumeCreateInfo
                {
                    type = Api.VaStructureType.VA_TYPE_VOLUME_CREATE_INFO,
                    next = restoreConfigPtr,
                    systemId = systemId,
                };

                Api.CheckResult(Api.vaCreateVolume(session, vaCreateInfo, out handle));
                Trace.LogInfo(() => $"create_volume: volume = {handle}");
            }
            finally
            {
                Marshal.FreeHGlobal(restoreIdPtr);
                Marshal.FreeHGlobal(restoreConfigPtr);
            }

            return handle;
        }

        internal void HandleVolumeStateChanged(Api.VaEventVolumeStateChanged stateChanged)
        {
            Trace.LogInfo(() => $"Volume[{Handle.ToInt64()}] state changed: from {_state} to {stateChanged.state}");

            _state = stateChanged.state;
            switch (stateChanged.action)
            {
                case Api.VaVolumeStateAction.VA_VOLUME_STATE_ACTION_ON_READY:
                    HandleUpdate((VaFrameId)1); // First update
                    break;
                case Api.VaVolumeStateAction.VA_VOLUME_STATE_ACTION_ON_CLOSE:
                    RemoveAllElements();
                    OnClose?.Invoke(this);
                    this.Destroy();
                    break;
                case Api.VaVolumeStateAction.VA_VOLUME_STATE_ACTION_ON_PAUSE:
                    OnPause?.Invoke(this);
                    break;
                case Api.VaVolumeStateAction.VA_VOLUME_STATE_ACTION_ON_RESUME:
                    OnResume?.Invoke(this);
                    break;
                default:
                    Trace.LogWarning(() => $"Unhandled volume state action: {stateChanged.action}");
                    break;
            }
        }

        internal void HandleElementAsyncStateChanged(Api.VaEventElementAsyncStateChanged stateChanged)
        {
            // Query for data that has a list of elements that have changed their async state.
            var changes = new Api.VaChangedElements
            {
                type = Api.VaStructureType.VA_TYPE_CHANGED_ELEMENTS
            };

            var info = new Api.VaChangedElementsGetInfo
            {
                type = Api.VaStructureType.VA_TYPE_CHANGED_ELEMENTS_GET_INFO,
                filterFlags = Api.VaElementChangeFilterFlags.VA_ELEMENT_CHANGE_FILTER_ASYNC_STATE,
            };

            // Two call idiom. This call will tell us how many elements are available but we won't get the list of elements.
            Api.CheckResult(Api.vaGetChangedElements(_handle, info, out changes));

            while (changes.elementCountOutput > 0)
            {
                IntPtr changedElementsPtr = IntPtr.Zero;

                try
                {
                    int elementCount = (int)changes.elementCountOutput;
                    // Now we know how many elements are available, so we can allocate a buffer to hold them.
                    changedElementsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(_handle) * elementCount);

                    // This buffer will be filled with the actual elements in the second call.
                    changes.elementCapacityInput = changes.elementCountOutput;
                    changes.elements = changedElementsPtr;

                    Api.CheckResult(Api.vaGetChangedElements(_handle, info, out changes));

                    if (changes.elementCountOutput > 0)
                    {
                        IntPtr[] changedElements = new IntPtr[elementCount];
                        Marshal.Copy(changes.elements, changedElements, 0, elementCount);

                        foreach (var elementHandle in changedElements)
                        {
                            if (TryGetElement(elementHandle, out Element element))
                            {
                                // If there is a change, this will raise an event on the Element.
                                element.UpdateAsyncState();
                            }
                        }
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(changedElementsPtr);
                }

                // Reset struct to get new output count, to see if there are any more changes to process.
                changes = new Api.VaChangedElements
                {
                    type = Api.VaStructureType.VA_TYPE_CHANGED_ELEMENTS
                };

                // Two call idiom. This call will tell us how many elements are available but we won't get the list of elements.
                Api.CheckResult(Api.vaGetChangedElements(_handle, info, out changes));
            }
        }

        internal void HandleAdaptiveCardActionInvoked(Api.VaEventAdaptiveCardActionInvokedExt actionInvoked)
        {
            if (TryGetElement(actionInvoked.element, out Element element) && element is AdaptiveCard adaptiveCard)
            {
                adaptiveCard.PollAdaptiveCardActionInvokedData();
            }
        }

        internal void HandleUpdate(VaFrameId frameId)
        {
            var frameState = new Api.VaUpdateVolumeFrameState
            {
                type = Api.VaStructureType.VA_TYPE_UPDATE_VOLUME_FRAME_STATE,
            };

            {
                var beginInfo = new Api.VaUpdateVolumeBeginInfo
                {
                    type = Api.VaStructureType.VA_TYPE_UPDATE_VOLUME_BEGIN_INFO,
                    frameId = frameId,
                };
                Api.CheckResult(Api.vaBeginUpdateVolume(_handle, beginInfo, out frameState));
            }

            // Mark frame state to be current after successful begin update volume
            _frameState.frameId = frameId;
            _frameState.frameTime = frameState.time;
            _frameState.frameDuration = frameState.duration;
            _frameState.IsCurrent = true;

            try
            {
                List<DeferredAction> deferredActions;
                lock (_dispatchToNextUpdate)
                {
                    deferredActions = new List<DeferredAction>(_dispatchToNextUpdate);
                    _dispatchToNextUpdate.Clear();
                }

                foreach (var deferredAction in deferredActions)
                {
                    deferredAction.Action?.Invoke();
                    deferredAction.TaskCompletionSource?.SetResult(true);
                }

                if (frameId == (VaFrameId)1)
                {
                    _isReady = true;
                    if (_onReady != null)
                    {
                        _onReadyDispatched = true;
                        _onReady.Invoke(this);
                    }
                }
                else
                {
                    OnUpdate?.Invoke(this);
                }
            }
            finally
            {
                // Mark frame state as not current before end updating volume
                // yet retain cached time and duration value as previous frame.
                _frameState.IsCurrent = false;
            }

            {
                var endInfo = new Api.VaUpdateVolumeEndInfo
                {
                    type = Api.VaStructureType.VA_TYPE_UPDATE_VOLUME_END_INFO,
                    frameId = frameId,
                };
                Api.CheckResult(Api.vaEndUpdateVolume(_handle, endInfo));
            }
        }

        internal void HandleRestoreResult(Api.VaEventVolumeRestoreResultExt restoreResult)
        {
            _restoreResultValue = (VaVolumeRestoredResultExt)restoreResult.volumeRestoreResult;
            _restoreResultReceived = true;
            if (_onRestoreResult != null)
            {
                _onRestoreResultDispatched = true;
                _onRestoreResult.Invoke(this, _restoreResultValue);
            }
        }

        /// <summary>
        /// Dispatches pending OnReady callback if the volume is ready but OnReady hasn't been invoked yet.
        /// This handles the case where OnReady is subscribed after the volume has already transitioned to RUNNING.
        /// </summary>
        internal void TryDispatchPendingOnReady()
        {
            if (_isReady && !_onReadyDispatched && _onReady != null)
            {
                _onReadyDispatched = true;
                _onReady.Invoke(this);
            }
        }

        /// <summary>
        /// Dispatches pending OnRestoreResult callback if the restore result was received but hasn't been invoked yet.
        /// This handles the case where OnRestoreResult is subscribed after the restore result event has already arrived.
        /// </summary>
        internal void TryDispatchPendingOnRestoreResult()
        {
            if (_restoreResultReceived && !_onRestoreResultDispatched && _onRestoreResult != null)
            {
                _onRestoreResultDispatched = true;
                _onRestoreResult.Invoke(this, _restoreResultValue);
            }
        }

        internal void AddElement(Element element)
        {
            lock (_elementsLock)
            {
                _elements.Add(element);
            }
        }

        /// <summary>
        /// Removes the specified element from the volume.
        /// This will also destroy the element once it is disconnected from the volume.
        /// </summary>
        public void RemoveElement(Element element)
        {
            lock (_elementsLock)
            {
                _elements.Remove(element);

                if (ReferenceEquals(element, _containerElement))
                {
                    _containerElement = null;
                }

                if (ReferenceEquals(element, _contentElement))
                {
                    _contentElement = null;
                }
            }

            element.DestroyHandle();
        }

        /// <summary>
        /// Removes all elements from the volume.
        /// This will also destroy all elements once they are disconnected from the volume.
        /// </summary>
        public void RemoveAllElements()
        {
            List<Element> toDestroy = new();
            lock (_elementsLock)
            {
                toDestroy.AddRange(_elements);
                _elements.Clear();

                if (_containerElement != null)
                {
                    toDestroy.Add(_containerElement);
                }
                _containerElement = null;

                if (_contentElement != null)
                {
                    toDestroy.Add(_contentElement);
                }
                _contentElement = null;
            }

            // Destroy elements in reverse order of creation to ensure dependent elements
            // (children, references) are destroyed before the elements they depend on.
            toDestroy.Reverse();
            foreach (Element child in toDestroy)
            {
                child.DestroyHandle();
            }
        }

        private bool TryGetElement(IntPtr handle, out Element element)
        {
            lock (_elementsLock)
            {
                element = _elements.Find(e => e.Handle == handle);
            }
            return element != null;
        }

        private sealed class DeferredAction
        {
            public Action? Action;
            public TaskCompletionSource<bool>? TaskCompletionSource;
        }

        private readonly List<DeferredAction> _dispatchToNextUpdate = new();

        /// <summary>
        /// Dispatches the specified action to be executed in the next update.
        /// This is useful for deferring actions that need to happen in the update scope,
        /// that is after the vaBeginUpdateVolume and before the vaEndUpdateVolume calls.
        /// And example of such method is to acquire mesh buffer for the mesh element.
        /// </summary>
        /// <remarks> Generally, the creation, destroy of elements and setting element properties
        /// are allowed outside of the update scope, and therefore don't have to be dispatched.
        /// </remarks>
        public void DispatchToNextUpdate(Action value)
        {
            var deferredAction = new DeferredAction
            {
                Action = value,
                TaskCompletionSource = new TaskCompletionSource<bool>(),
            };

            lock (_dispatchToNextUpdate)
            {
                _dispatchToNextUpdate.Add(deferredAction);
            }

            if (_requestedUpdateMode == VaVolumeUpdateMode.OnDemand)
            {
                RequestUpdate();    // If there's no recurring update, request one.
            }
        }
    }
}
