// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable

namespace Microsoft.MixedReality.Volumetric
{
    using Detail;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api = Detail.Api;

    /// <summary>
    /// VolumetricApp is the main entry point for volumetric applications.
    /// It manages the session and provides a simple interface for handling volumetric events.
    /// </summary>
    public class VolumetricApp
    {
        /// <summary>
        /// Get the current API version. This is an invariable value that won't change until the library is updated.
        /// </summary>
        public static readonly VaVersion CurrentApiVersion = Api.ApiVersion;

        private readonly string _appName;
        private readonly VaVersion _apiVersion = Api.ApiVersion;
        private readonly string[] _requiredExtensions;
        private readonly string[] _optionalExtensions;
        private readonly VaVolumeRestoreBehaviorExt _volumeRestoreBehavior;
        private readonly VaSessionWaitForSystemBehavior _waitForSystemBehavior;

        private readonly object _lock = new object();
        private bool _started;
        private bool _stopped;
        private bool _onStartDispatched; // Track if OnStart was dispatched
        private string? _fatalErrorMessage; // Used to store fatal error messages that can be retrieved later. When not null, the app should quit.

        private readonly Detail.Session _session;
        internal Detail.Session Session => _session;

        /// <summary>
        /// Constructs a new VolumetricApp instance.
        /// </summary>
        /// <param name="appName">The name of the application.</param>
        /// <param name="apiVersion">The API version to use. If null, the current API version will be used.</param>
        /// <param name="requiredExtensions">An array of required extensions. If null, no required extensions will be used.</param>
        /// <param name="optionalExtensions">An array of optional extensions. If null, no optional extensions will be used.</param>
        /// <remarks>
        /// The app name must not be empty. If the app name is null or empty, an ArgumentException will be thrown.
        /// If any of the requireExtensions is not supported by the runtime, an Api.LibraryException will be thrown and therefore the app will not be created.
        /// If any of the optionalExtensions is not supported by the runtime, it will be ignored.  The app will still be created but the optional extensions will not be available.
        /// </remarks>
        public VolumetricApp(
            string appName,
            VaVersion? apiVersion = null,
            string[]? requiredExtensions = null,
            string[]? optionalExtensions = null,
            VaVolumeRestoreBehaviorExt volumeRestoreBehavior = VaVolumeRestoreBehaviorExt.NoRestore,
            VaSessionWaitForSystemBehavior waitForSystemBehavior = VaSessionWaitForSystemBehavior.RetryWithUserCancel)
        {
            if (string.IsNullOrWhiteSpace(appName))
            {
                throw new ArgumentException("App name must not be empty.", nameof(appName));
            }

            _appName = appName;
            _apiVersion = apiVersion ?? CurrentApiVersion;
            _requiredExtensions = requiredExtensions ?? Array.Empty<string>();
            _optionalExtensions = optionalExtensions ?? Array.Empty<string>();
            _volumeRestoreBehavior = volumeRestoreBehavior;
            _waitForSystemBehavior = waitForSystemBehavior;

            _session = Initialize();
            _session.OnSystemChanged += OnSystemChanged;
            _session.OnSessionStopped += OnSessionStopped;
        }

        /// <summary>
        /// Gets the name of the application.
        /// This name is imutable for the lifetime of this class.
        /// </summary>
        public string AppName => _appName;

        /// <summary>
        /// Gets the API version of the application.
        /// This version is imutable for the lifetime of this class.
        /// </summary>
        public VaVersion ApiVersion => _apiVersion;

        /// <summary>
        /// Gets whether the given extension is enabled for this VolumetricApp.
        /// The result is true if the extension is enabled, false otherwise.
        /// This result is imutable for the lifetime of this class.
        /// </summary>
        public bool IsExtensionEnabled(string extension)
        {
            return _session.Extensions.IsEnabled(extension);
        }

        /// <summary>
        /// This event is raised once when the app is started, i.e. when the first system is connected.
        /// In this event handler, a valid system Id is already available.
        /// The application can start creating new volumes and contents inside it.
        /// This event is guaranteed to be raised exactly once before any other app events.
        /// If subscribed after the app is already started, the callback will be invoked on the next event processing cycle.
        /// </summary>
        public event Action<VolumetricApp>? OnStart;

        /// <summary>
        /// This event is raised when the app is disconnected from the system.
        /// In this event handler, the system Id is already set to 0.
        /// The application cannot operate on volumes or contents inside it when system is disconnected.
        /// </summary>
        public event Action<VolumetricApp>? OnDisconnect;

        /// <summary>
        /// This event is raised when the app is reconnected to the system.
        /// In this event handler, a valid system Id is already available.
        /// This even is only raised after the app already successfully started and the system is disconnected earlier
        ///  and a second system is connected with a different system Id.
        /// Note that everytime the system is disconnected and reconnected, there is a distinct system Id.
        /// </summary>
        public event Action<VolumetricApp>? OnReconnect;

        /// <summary>
        /// This event is raised when the app is stopped.
        /// After this event is raised, the app is terminated and cannot be used anymore.
        /// Application should clean  up any resources related to this app in this event handler.
        /// Note that the destroy of VolumetricApp is managed by the library and will be destroyed after this event.
        /// The application can request the app to stop by calling RequestExit method.
        /// </summary>
        public event Action<VolumetricApp>? OnStop;

        /// <summary>
        /// This event is raised when when the app encounters a fatal error and is about to exit.
        /// When this event is raised, other events such as onStop or onClose will not be raised.
        /// Application should only use this event to log the error and exit gracefully.
        /// </summary>
        public event Action<string>? OnFatalError;

        /// <summary>
        /// Platform request the app to restore a new volume using the given volumeRestoreId.
        /// The app must properly restore the volume's app state in relate to this restore id.
        /// If the app choose to ignore the request, the volume restore Id will be removed by Platform.
        /// </summary>
        public event Action<VolumetricApp, VaUuid>? OnVolumeRestoreRequest;

        /// <summary>
        /// Platform notifies the app that the volume restore id is invalidated.
        /// </summary>
        public event Action<VolumetricApp, VaUuid>? OnVolumeRestoreIdInvalidated;

        /// <summary>
        /// Gets whether the app is started.
        /// This value is false until the first system is connected.
        /// After the first system is connected, this value is true and will not change.
        /// </summary>
        public bool IsStarted => _started;

        /// <summary>
        /// Gets whether the app is connected to a system.
        /// This value is true if the app is connected to a system and a valid system Id is available.
        /// This value is false if the app is disconnected from a system.
        /// </summary>
        public bool IsConnected => _session.SystemId != 0;

        /// <summary>
        /// Gets whether the app is stopped.
        /// This value is true if the app is stopped and cannot be used anymore.
        /// Once this value becomes true, it will remain true and will not change anymore.
        /// </summary>
        public bool IsStopped => _stopped;

        /// <summary>
        /// Gets a list of active volume objects in this app.
        /// This list does not include volumes that are already closed or destroyed.
        /// This list is empty if there are no active volumes in this app.
        /// </summary>
        public IReadOnlyList<Volume> Volumes => _session.ActiveVolumes();

        private bool ShouldContinue()
        {
            lock (_lock)
            {
                if (_fatalErrorMessage != null)
                {
                    // If there's a fatal error, we should stop the app.
                    return false;
                }

                if (_stopped)
                {
                    return false; // App is already stopped and there's no turnning back.
                }

                if (!_started)
                {
                    return true; // App is not yet initialized, wait for the first system connection.
                }

                return true;
            }
        }

        /// <summary>
        /// Run the volumetric app event loop in the current thread.
        /// This method will not return until the app is stopped or exited.
        /// </summary>
        public int Run(Action<VolumetricApp>? onStart = null)
        {
            Thread.CurrentThread.Name = $"VolumetricApp - {_appName}";

            onStart.IfHasValue(v => this.OnStart += v);

            while (ShouldContinue())
            {
                try
                {
                    _session.ProcessEventsThenWait();
                }
                catch (Exception ex)
                {
                    // This is unhandled exception that if thrown to the app, it likely will crash the app.
                    // We log the error and store the full exception (including stack trace) so that the app can check it later and quit gracefully.
                    _fatalErrorMessage = ex.ToString();
                }
            }
            Terminate();
            return _fatalErrorMessage == null ? 0 : -1;
        }

        /// <summary>
        /// Run the volumetric app event loop in a new thread.
        /// This method will return immediately and the app will run in the thread of the returned Task.
        /// The returned Task will not complete until the app is stopped or exited.
        /// </summary>
        public Task RunAsync()
        {
            return Task.Run(() => Run());
        }

        /// <summary>
        /// Poll and process all available events and then yield to the caller
        /// </summary>
        /// <returns>Returns true if the event loop should continue.
        /// Returns false if app should exit the event loop and terminate this VolumetricApp.
        /// </returns>
        public bool PollEvents()
        {
            if (ShouldContinue())
            {
                try
                {
                    _session.ProcessEventsThenYield();
                }
                catch (Exception ex)
                {
                    // This is unhandled exception that if thrown to the app, it likely will crash the app.
                    // We log the error and store the full exception (including stack trace) so that the app can check it later and quit gracefully.
                    _fatalErrorMessage = ex.ToString();

                    Terminate();
                    return false; // Exit the event loop due to fatal error.
                }

                return true;
            }
            else
            {
                Terminate();
                return false;
            }
        }

        /// <summary>
        /// Request the app to exit the event loop and terminate this VolumetricApp.
        /// This method will return immediately after making the exit request.
        /// This method does not exit the app and instead it will rely on the app to continue the event loop,
        /// Request app to exit also implies to request close all volumes in this app.
        /// The application will first recieve the events to close each volume, and then recieve the
        /// app stop event.  The app should clean up resource properly in these events
        /// and after the stop event is handed it will be destroyed by the library.
        /// </summary>
        public void RequestExit()
        {
            lock (_lock)
            {
                _session.RequestStop();
            }
        }

        /// <summary>
        /// Marks the volume with the given restore id as not restorable.
        /// </summary>
        public void RemoveRestorableVolume(VaUuid volumeRestoreId)
        {
            Api.CheckResult(Api.vaRemoveRestorableVolumeExt(Session.Handle, volumeRestoreId));
        }

        private void OnSessionStopped(Detail.Session runtime)
        {
            Terminate();
        }

        private void OnSystemChanged(Detail.Session runtime)
        {
            if (_session.SystemId != 0)
            {
                if (!_started)
                {
                    _started = true;
                    if (OnStart != null)
                    {
                        _onStartDispatched = true;
                        OnStart.Invoke(this);
                    }
                }
                else
                {
                    this.OnReconnect?.Invoke(this);
                }
            }
            else
            {
                this.OnDisconnect?.Invoke(this);
            }
        }

        private Detail.Session Initialize()
        {
            Api.PFN_vaGetFunctionPointer getFunctionPointer = Detail.Loader.LoadRuntime();
            if (getFunctionPointer is null)
            {
                throw new Api.LibraryException("There's no session satisfies the loader requirements.");
            }

            IntPtr function;
            Api.CheckResult(getFunctionPointer(IntPtr.Zero, "vaCreateSession", out function));
            Api.PFN_vaCreateSession vaCreateSession = Api.ToDelegate<Api.PFN_vaCreateSession>(function);

            Detail.SessionExtensions extensions = new();
            extensions.Initialize(getFunctionPointer, _requiredExtensions, _optionalExtensions);

            if (extensions.MissingRequiredExtensions.Count > 0)
            {
                throw new Api.LibraryException("Missing required extensions: " + string.Join(", ", extensions.MissingRequiredExtensions));
            }

            IntPtr session = Detail.Session.CreateHandle(
                vaCreateSession,
                _appName ?? "VolumetricApp",
                _apiVersion,
                extensions.EnabledExtensions.Keys.ToArray(),
                (Api.VaVolumeRestoreBehaviorExt)_volumeRestoreBehavior,
                (Detail.Api.VaSessionWaitForSystemBehavior)_waitForSystemBehavior);

            InitializeFunctionPointers(session, getFunctionPointer, extensions);

            return new Detail.Session(this, session, extensions);
        }

        internal virtual void InitializeFunctionPointers(IntPtr session, Api.PFN_vaGetFunctionPointer getFunctionPointer, SessionExtensions extensions)
        {
            Api.InitializeFunctionPointers(session, getFunctionPointer, extensions);
        }

        internal void HandleVolumeRestoreRequest(Api.VaEventVolumeRestoreRequestExt restoreRequest)
        {
            OnVolumeRestoreRequest?.Invoke(this, restoreRequest.volumeRestoreId);
        }

        internal void HandleVolumeRestoreIdInvalidated(Api.VaEventVolumeRestoreIdInvalidatedExt restoreIdInvalidated)
        {
            OnVolumeRestoreIdInvalidated?.Invoke(this, restoreIdInvalidated.volumeRestoreId);
        }

        /// <summary>
        /// Dispatches pending OnStart callback if the app is started but OnStart hasn't been invoked yet.
        /// This handles the case where OnStart is subscribed after the app has already started.
        /// </summary>
        internal void TryDispatchPendingOnStart()
        {
            if (_started && !_onStartDispatched && OnStart != null)
            {
                _onStartDispatched = true;
                OnStart?.Invoke(this);
            }
        }

        private void Terminate()
        {
            bool processTermination = false;
            lock (_lock)
            {
                if (_stopped)
                {
                    return; // Already stopped, no need to stop again.
                }
                else
                {
                    // There's no turning back after this point.
                    // This App object is terminated and can only be destroyed.
                    _stopped = true;
                    processTermination = true;
                }
            }

            if (_fatalErrorMessage != null)
            {
                Trace.LogError(() => $"Fatal error: {_fatalErrorMessage}");

                // If there's a fatal error, notify the app and terminate it without cleaning up.
                // because cleanup might fail due to the fatal error.
                this.OnFatalError?.Invoke(_fatalErrorMessage);
                return;
            }

            if (processTermination)
            {
                foreach (var volume in _session.ActiveVolumes())
                {
                    _session.RemoveVolume(volume);
                }

                this.OnStop?.Invoke(this);

                _session.Destroy();
            }
        }

        /// <summary>
        /// Get the asset URI for the given relative path to this app.
        /// </summary>
        /// <param name="relativePath">A string for a valid relative path in Windows filepath format.</param>
        /// <returns>Returns a string for a valid Uri that can be used for Uri properties.</returns>
        /// <exception cref="Api.LibraryException">This exception might be thrown if the given relative path is not resolved to a valid file.</exception>
        public static string GetAssetUri(string relativePath)
        {
            string assetDir = Path.Combine(AppContext.BaseDirectory, relativePath);
            string assetPath = Path.GetFullPath(assetDir);
            if (!File.Exists(assetPath))
            {
                throw new Api.LibraryException($"Asset file '{assetPath}' not found.");
            }
            return new Uri(assetPath).AbsoluteUri;
        }
    }
}
