#pragma once

#include <vaVersion.h>
#include <vaFunctions.h>
#include <vaExtensions.h>

namespace va {
    class VolumetricApp;
    class Volume;
    namespace detail {
        struct AppContext;
        class Session;
    } // namespace detail

    struct AppCreateInfo {
        /// <summary>
        /// Specify the name of the application, which typically identifies the application.
        /// </summary>
        std::string applicationName;

        /// <summary>
        /// Specify a list of required extensions that must be supported for this application.
        /// Applications typically specify minimum list of extensions that are required for better compatibility.
        /// </summary>
        std::vector<std::string> requiredExtensions = {};

        /// <summary>
        /// Specify the version of the API that the application expects.
        /// This value typically should be VA_CURRENT_API_VERSION when the app is compiled.
        /// </summary>
        VaVersion apiVersion = VA_CURRENT_API_VERSION;

        /// <summary>
        /// Specify a list of extensions that's optional for the application to use.
        /// Applications typically inspect if the optional extension is supported and fallback gracefully.
        /// </summary>
        std::vector<std::string> optionalExtensions = {};

        /// <summary>
        /// Specify the volume restore behavior in this application.
        /// For all volumes created with "restorable = true", they will share the same restore behavior.
        /// </summary>
        /// <remarks>
        /// If the behavior is NO_RESTORE, which is default, all volumes are not restorable after they are closed.
        /// If the behavior is BY_APP, all volumes are restorable by the app when creating the volume with volumeRestoreId
        /// If the behavior is BY_PLATFORM_MULTIPLE_VOLUMES all volumes are restorable by the platform. Apps will received
        ///     VaEventVolumeRestoreRequestExt event when the app reconnects to the platform if there are volumes to restore.
        /// A volume can opt-out of restore after closing behavior by setting restorable to false when creating the volume.
        /// </remarks>
        VaVolumeRestoreBehaviorExt volumeRestoreBehavior = VA_VOLUME_RESTORE_BEHAVIOR_NO_RESTORE_EXT;

        /// <summary>
        /// When the application starts, if the ImmersiveAddOn is not running, this flag controls the behavior of the runtime.
        /// </summary>
        /// <remarks>
        /// If the behavior is _RETRY_WITH_USER_CANCEL, which is default, the runtime will keep trying to connect and show a 2d dialog to
        /// the user. If the user cancels the dialog, the retry loop exists and onStop is raised.
        /// If the behavior is _RETRY_SILENTLY, the runtime will keep trying to connect without showing any dialog.
        /// If the behavior is _NO_WAIT, the runtime will immediately raise onStop.
        /// </remarks>
        VaSessionWaitForSystemBehavior waitForSystemBehavior = VA_SESSION_WAIT_FOR_SYSTEM_BEHAVIOR_RETRY_WITH_USER_CANCEL;
    };

    /// <summary>
    /// A factory function to create a new volumetric app using the given configuration.
    /// </summary>
    template <typename TApp>
    std::unique_ptr<TApp> CreateVolumetricApp(AppCreateInfo&& createInfo);

    /// <summary>
    /// A shorthand for CreateVolumetricApp<`va::VolumetricApp`>().
    /// </summary>
    std::unique_ptr<VolumetricApp> CreateVolumetricApp(AppCreateInfo&& createInfo);

    class VolumetricApp : va::NonMovable {
    public:
        /// <summary>
        /// Constructor for a volumetric app using raw instance handle.
        /// Application typically does not need to call this directly, instead use CreateVolumetricApp().
        /// </summary>
        VolumetricApp(AppCreateInfo createInfo, VaSession session, std::unique_ptr<va::Functions>&& functions, std::unique_ptr<va::Extensions>&& extensions);

        /// <summary>
        /// Run the volumetric app until all volumes are closed or app.Close()
        /// </summary>
        int Run();

        /// <summary>
        /// Run the volumetric app until all volumes are closed or app.Close()
        /// This is a shorthand for Run() with onStart callback setup in one call.
        /// </summary>
        int Run(std::function<void(va::VolumetricApp&)>&& onStart);

        /// <summary>
        /// Run the volumetric app in a background thread and immediately return to the caller
        /// </summary>
        [[nodiscard]] std::future<void> RunAsync();

        /// <summary>
        /// Poll and process all available events and then yield to the caller
        /// </summary>
        /// <returns>Returns true if continue app loop. Returns false if app is exiting.</returns>
        bool PollEvents();

        /// <summary>
        /// Request to exit the app by closing all volumes.
        /// The app will continue processing the remaining volume lifecycle events and then exit.
        /// </summary>
        void RequestExit();

        /// <summary>
        /// Creates a new volume of type <c>TVolume</c> in the app.
        /// </summary>
        /// <param name="TVolume"> must have a constructor with the signature
        /// <c>TVolume(va::VolumetricApp&amp;, Args&amp;&amp;...)</c>.
        /// This app together with input arguments are forwarded to the <c>TVolume</c> constructor.
        /// </param>
        /// <returns>
        /// A raw pointer to the created volume. The app will take ownership of the volume object.
        /// </returns>
        template <typename TVolume, typename... Args>
        TVolume* CreateVolume(Args&&... args);

        /// <summary>
        /// Remove a volume from the app.
        /// </summary>
        void RemoveVolume(Volume* volume);

        /// <summary>
        /// Called once when the first volumetric system is connected and the app is ready to run.
        /// In this callback, a valid system Id is already available.
        /// The application can start creating new volumes and contents inside it.
        /// This callback is guaranteed to be raised exactly once before any other app events.
        /// If subscribed after the app is already started, the callback will be invoked on the next event processing cycle.
        /// </summary>
        std::function<void(va::VolumetricApp&)> onStart;

        /// <summary>
        /// Called after all volumes are closed and the session is about to be destroyed.
        /// </summary>
        std::function<void()> onStop;

        /// <summary>
        /// This event is raised when when the app encounters a fatal error and is about to exit.
        /// When this event is raised, other events such as onStop or onClose will not be raised.
        /// Application should only use this event to log the error and exit gracefully.
        /// </summary>
        std::function<void(const char* errorMessage)> onFatalError;

        /// <summary>
        /// Called when the app is disconnected from the volumetric system.
        /// </summary>
        std::function<void(va::VolumetricApp&)> onDisconnect;

        /// <summary>
        /// Called when the app is reconnected to the volumetric system.
        /// </summary>
        std::function<void(va::VolumetricApp&)> onReconnect;

        /// <summary>
        /// Platform request the app to restore a new volume using the given volumeRestoreId.
        /// The app must properly restore the volume's app state in relate to this restore id.
        /// If the app choose to ignore the request, the volume restore Id will be removed by Platform.
        /// </summary>
        std::function<void(va::VolumetricApp&, VaUuid)> onRestoreVolumeRequest;

        /// <summary>
        /// Platform notifies the app that the volume restore id is invalidated.
        /// </summary>
        std::function<void(va::VolumetricApp&, VaUuid)> onVolumeRestoreIdInvalidated;

        bool IsStarted() const;
        bool IsConnected() const;
        bool IsStopped() const;

        /// <summary>
        /// Remove the restorable volume with the given restore id.
        /// For volumes with restore behavior set to NO_RESTORE
        ///     Platform will forget the volume automatically when the volume is closed.
        /// For volumes with restore behavior set to RESTORE_BY_PLATFORM_*
        ///     Platform will forget the volume automatically if the app ignores the restore request event.
        /// For volumes with restore behavior set to RESTORE_BY_APP
        ///     Platform will remember the volume until the app call RemoveRestorableVolume
        ///     or when the user manually removes the volume restore info from the settings app.
        /// </summary>
        /// <remarks>
        /// If the given restore id is invalid, this function does nothing instead of throwing an exception.
        /// </remarks>
        void RemoveRestorableVolume(const VaUuid& restoreId);

    protected:
        va::detail::AppContext& Context() const;
        VaSession SessionHandle() const;

        /// <summary>
        /// Called before standard event processing. Derived classes can override to handle custom event types.
        /// </summary>
        /// <param name="eventType">The type of the event</param>
        /// <param name="eventData">Pointer to the raw event data buffer</param>
        /// <returns>True if the event was handled and should not be processed further, false otherwise</returns>
        virtual bool OnPreprocessEvent(VaStructureType eventType, const void* eventData);

    private:
        const std::unique_ptr<va::detail::Session> m_session;
        const AppCreateInfo m_createInfo;
        bool m_started = false;
        bool m_onStartDispatched = false;
        std::atomic_bool m_stopped = false;
        uint64_t m_relaunchToken = 0;

        std::atomic<int> m_exitCode = 0;
        std::string m_errorMessage;

    private:
        friend struct va::detail::AppContext;
        friend class va::detail::Session;
        friend class va::Volume;
        bool ShouldContinue() const;
        void HandleSystemChanged();
        void HandleVolumeRestoreRequest(const VaEventVolumeRestoreRequestExt& event);
        void HandleVolumeRestoreIdInvalidated(const VaEventVolumeRestoreIdInvalidatedExt& event);
        void Terminate();
        void HandleFatalError(const char* errorMessage);
        bool TryDispatchPendingOnStart();
    };
}; // namespace va
