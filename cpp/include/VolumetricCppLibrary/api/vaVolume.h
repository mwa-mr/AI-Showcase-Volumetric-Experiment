#pragma once

#include <functional>
#include <thread>
#include <vaCtor.h>
#include <vaMath.h>
#include <detail/components/vaElementList.h>

namespace va {
    class Element;
    namespace detail {
        struct AppContext;
    } // namespace detail

    struct FrameState {
        bool isCurrent{};
        VaFrameId frameId{};
        VaTime frameTime{};
        VaDuration frameDuration{};
    };

    class VolumeContainer {
    public:
        /// <summary>
        /// Set the display name of the volume container.
        /// </summary>
        /// <param name="displayName">A Utf8 string for Volume display name.</param>
        void SetDisplayName(const char* displayName);

        /// <summary>
        /// Set the rotation lock flags for the volume container.
        /// </summary>
        void SetRotationLock(VaVolumeRotationLockFlags rotationLock);

        /// <summary>
        /// Set the URI for the thumbnail model of the volume container.
        /// The thumbnail model is used to represent the volume in system UIs, such as the volume summary view.
        /// It should be a URI to a Gltf2 model that represents the volume.
        /// The model should be small and optimized for quick loading.
        /// If this property is not set, set to nullptr or set to an empty string,
        /// the platform will take a snapshot of the volume content when needed and used it as default thumbnail model.
        /// </summary>
        void SetThumbnailModelUri(const char* thumbnailModelUri);

        /// <summary>
        /// Set the URI for the thumbnail icon of the volume container.
        /// The thumbnail icon is used to represent the volume in system UIs together with the display name.
        /// The thumbnail icon must be a square PNG image with size between 32x32 pixels to 256x256 pixels.
        /// By providing a 256px icon, it ensures the system only ever scales your icon down, never up.
        /// The image may have transparent pixels.
        /// If this property is not set, set to null, set to an empty string or failed to load as PNG file,
        /// the platform will present the volume with a generic thumbnail icon.
        /// </summary>
        void SetThumbnailIconUri(const char* thumbnailIconUri);

        // <summary>
        // Set whether the volume is allowed to be in the interactive mode.
        // By default, it is disallowed and the user cannot switch to the interactive mode.
        // The hand inputs will only be delivered to the app when the user enters the interactive mode.
        // To properly handle hand inputs, the app must first allow this mode.
        // </summary>
        void AllowInteractiveMode(bool allow);

        // <summary>
        // Set whether the volume is allowed to be in the one-to-one mode.
        // By default, it is allowed and the user can switch to the one-to-one mode.
        // If disallowed, the user cannot switch this volume to the one-to-one mode.
        // </summary>
        void AllowOneToOneMode(bool allow);

        // <summary>
        // Set whether the volume is allowed to be shared in teams.
        // By default, it is allowed and the user can share this volume in teams.
        // If disallowed, the user cannot share this volume in teams.
        // </summary>
        void AllowSharingInTeams(bool allow);

        // <summary>
        // Set whether the volume is allowed to be in the unbounded mode.
        // By default, it is allowed and the user can switch to the unbounded mode.
        // If disallowed, the user cannot switch this volume to the unbounded mode.
        // </summary>
        void AllowUnboundedMode(bool allow);

        // <summary>
        // Set whether the volume is allowed to be in the subpart mode.
        // By default, it is allowed and the user can switch to the subpart mode.
        // If disallowed, the user cannot switch this volume to the subpart mode.
        // </summary>
        void AllowSubpartMode(bool allow);

        // <summary>
        // Notify when the volume enters or exits the interactive mode.
        // </summary>
        std::function<void(bool interactiveMode)> onInteractiveModeChanged;

        // <summary>
        // Notify when the volume enters or exits the one-to-one mode.
        // </summary>
        std::function<void(bool oneToOneModel)> onOneToOneModeChanged;

        // <summary>
        // Notify when the volume enters or exits the sharing in teams mode.
        // </summary>
        std::function<void(bool sharingInTeams)> onSharingInTeamsChanged;

        // <summary>
        // Notify when the volume enters or exits the unbounded mode.
        // </summary>
        std::function<void(bool unboundedMode)> onUnboundedModeChanged;

        // <summary>
        // Notify when the volume enters or exits the subpart mode.
        // </summary>
        std::function<void(bool subpartMode)> onSubpartModeChanged;

    private:
        friend class va::Volume;
        friend class va::detail::Session;
        VolumeContainer(va::Volume& volume);
        void EnsureElement() const;
        void HandleContainerModeChanged(const VaEventVolumeContainerModeChangedExt& event);
        void SetModeCapabilities(VaVolumeContainerModeFlagsExt capabilities);

        va::Volume& m_volume;
        mutable VaElement m_handle{};

        // Cached property value for difference check
        std::string m_displayName;
        VaVolumeRotationLockFlags m_rotationLock = VA_VOLUME_ROTATION_LOCK_NONE;
        std::string m_thumbnailModelUri;
        std::string m_thumbnailIconUri;

        VaVolumeContainerModeFlagsExt m_allowedMode = VA_VOLUME_CONTAINER_MODE_DEFAULT_ALLOWED_EXT;
        VaVolumeContainerModeFlagsExt m_currentModes = VA_VOLUME_CONTAINER_MODE_NONE_EXT;
    };

    class VolumeContent {
    public:
        void SetPosition(const VaVector3f& position);
        void SetOrientation(const VaQuaternionf& orientation);
        void SetSize(float size);
        void SetSize(const VaExtent3Df& size);
        void SetSizeBehavior(VaVolumeSizeBehavior sizeBehavior);

        float GetActualScale() const;
        VaExtent3Df GetActualSize() const;
        VaVector3f GetActualPosition() const;
        VaQuaternionf GetActualOrientation() const;

    private:
        friend class va::Volume;
        explicit VolumeContent(va::Volume& volume);
        void EnsureElement() const;

        va::Volume& m_volume;
        mutable VaElement m_handle{};

        // Cached property value for difference check
        VaVector3f m_position = va::vector::zero;
        VaQuaternionf m_orientation = va::quaternion::identity;
        VaExtent3Df m_size = va::size::zero;
        VaVolumeSizeBehavior m_sizeBehavior = VA_VOLUME_SIZE_BEHAVIOR_AUTO_SIZE;
    };

    class Volume : va::NonMovable {
    public:
        /// <summary>
        /// Create a new volume with the given app.
        /// </summary>
        /// <param name="app">The app to associate with the volume.</param>
        /// <param name="restorable">Configure the volume to be restorable.
        /// If the app is created with restoreBehavior set to NO_RESTORE, this flag is ignored and the volume is not restorable.
        /// Otherwise, a volume is restorable by default, and app should handle onRestoreResult event properly.
        /// If the app opt-out of restoration, app can set restorable to false when creating the volume.
        /// </param>
        explicit Volume(va::VolumetricApp& app, bool restorable = true);

        /// <summary>
        /// Create a new volume with the given app and restore id.
        /// This allows the app to resource the volume from a previous session.
        /// </summary>
        Volume(va::VolumetricApp& app, VaUuid volumeRestoreId);

        ~Volume() override;

        va::VolumetricApp& App() const;

        VaVolumeState State() const;
        bool IsRunning() const;
        bool IsClosed() const;
        const FrameState& FrameState() const;

        /// <summary>
        /// The event that is raised once when the volume is ready to be used.
        /// This is raised for the very first frame after the volume is created.
        /// The application typically can start creating elements and set properties in this event.
        /// This event is guaranteed to be raised exactly once before any onUpdate events.
        /// If subscribed after the volume is already running, the callback will be invoked on the next event processing cycle.
        /// </summary>
        std::function<void(Volume& volume)> onReady;

        /// <summary>
        /// The event that is raised when the volume is updated.
        /// This is the update request for the application to set properties or manage elements.
        /// The very first frame update is raised in OnReady event.
        /// The application can setup the cadence of the volume update using RequestUpdate method.
        /// If the application does not call RequestUpdate, the volume will not be updated,
        /// except for the first frame that is always raised as OnReady event.
        /// </summary>
        std::function<void(Volume& volume)> onUpdate;

        /// <summary>
        /// The event that is raised when the volume is paused.
        /// This can be raised when the volume is paused by the user or by the system.
        /// For example, when the user opens the volume management UI and therefore all volumes are hidden.
        /// There will be no update event raised for the volume while it is paused.
        /// </summary>
        std::function<void(Volume& volume)> onPause;

        /// <summary>
        /// The event that is raised when the volume is resumed.
        /// This can be raised when the volume is resumed by the user or by the system.
        /// For example, when the user closes the volume management UI and therefore all volumes are visible again.
        /// Any outstanding update requests will be processed after the volume is resumed.
        /// </summary>
        std::function<void(Volume& volume)> onResume;

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
        std::function<void(Volume& volume)> onClose;

        /// <summary>
        /// The event that is raised when the volume restore result is available.
        /// If subscribed after the restore result has already been received, the callback will be invoked on the next event processing cycle.
        /// </summary>
        std::function<void(Volume& volume, VaVolumeRestoredResultExt)> onRestoreResult;

        /// <summary>
        /// Request frame updates for the volume.  The onUpdate event will be triggered accordingly.
        /// If provide with no parameter or "ON_DEMAND" mode, only one update is raised until requested again.
        /// Otherwise, the frame update event will be raised repeatedly according to the given mode.
        /// </summary>
        /// <remarks>
        /// This function doesn't have to be in any begin/end update scope.
        /// Only the latest "request update" function call will be considered.
        /// </remarks>
        void RequestUpdate(VaVolumeUpdateMode volumeUpdateMode = VA_VOLUME_UPDATE_MODE_ON_DEMAND);

        /// <summary>
        /// Request the on-demand frame updates for the volume with a given delay.
        /// The new frame update will be raised after the given delay.
        /// </summary>
        /// <remarks>
        /// This function doesn't have to be in any begin/end update scope.
        /// The delay timing is not guaranteed and it's considered as a hint to the platform.
        /// The platform will allocate the actual frame update event when platform resource allows it.
        /// Only the latest "request update" function call will be considered.
        /// </remarks>
        template <typename TClock, typename Period>
        void RequestUpdateAfter(const std::chrono::duration<TClock, Period>& onDemandUpdateDelay);

        /// <summary>
        /// Dispatch the given task to the next frame update between the begin and end of the frame.
        /// This allows the task to run without VA_ERROR_VOLUME_UPDATE_OUT_OF_SCOPE.
        /// The given task is moved into the dispatcher and will be discarded after the task is executed.
        /// If the current update mode is on demand, this function will also trigger an one-off onUpdate.
        /// </summary>
        void DispatchToNextUpdate(std::function<void()>&& task);

        /// <summary>
        /// Create an element and add it to the volume.
        /// The first argument of any Element type must be the volume itself.
        /// The rest of the arguments are passed to the element constructor.
        /// </summary>
        template <typename TElement, typename... Args>
        TElement* CreateElement(Args&&... args);

        /// <summary>
        /// Create an element and add it to the volume.
        /// Since element must at least take this volume as parent,
        ///   this overloads by default uses the element Ctor with va::Volume& as the only argument.
        /// </summary>
        template <typename TElement>
        TElement* CreateElement();

        void RemoveElement(Element* element);

        void RequestClose();

        /// <summary>
        /// Get the container properties of the volume.
        /// </summary>
        VolumeContainer& Container();

        /// <summary>
        /// Get the content properties of the volume.
        /// </summary>
        VolumeContent& Content();

        /// <summary>
        /// Get the restore id of the volume.  This id is unique for each volume when the volume is configured to support restore.
        /// </summary>
        /// <returns>A valid Uuid for this volume, or zero uuid if the volume is not restorable.</returns>
        VaUuid GetRestoreId() const;

    private:
        va::detail::AppContext& m_context;
        va::detail::ElementList m_elements;
        const VaVolume m_handle;

        std::mutex m_dispatcherMutex;
        std::vector<std::function<void()>> m_dispatchedTasks;
        VaVolumeUpdateMode m_volumeUpdateMode = VA_VOLUME_UPDATE_MODE_ON_DEMAND;

        VaSystemId m_associatedSystemId{};
        VaVolumeState m_state{};
        va::FrameState m_frameState{};
        bool m_isReady{false};
        bool m_onReadyDispatched{false};

        bool m_restoreResultReceived{false};
        bool m_onRestoreResultDispatched{false};
        VaVolumeRestoredResultExt m_restoreResultValue{};

        VolumeContainer m_container;
        VolumeContent m_content;

        friend class va::detail::Session;
        friend class va::Element;
        friend class va::VolumetricApp;
        friend class va::Element;
        friend class va::VolumeContainer;
        friend class va::VolumeContent;

        va::detail::AppContext& Context() const;
        VaVolume VolumeHandle() const;
        void VolumeStateChanged(const VaEventVolumeStateChanged& event);
        void AdaptiveCardActionInvoked(const VaEventAdaptiveCardActionInvokedExt& event);
        void ProcessFutures();
        void Update(VaFrameId frameId);
        void DiscardDispatcherTasks();
        void HandleVolumeRestoreResult(const VaEventVolumeRestoreResultExt& event);
        void HandleElementAsyncStateChanged(const VaEventElementAsyncStateChanged& event);
        bool TryDispatchPendingOnReady();
        bool TryDispatchPendingOnRestoreResult();

        template <typename TElement>
        TElement* AddElement(std::unique_ptr<TElement>&& element);
    };
} // namespace va
