#pragma once

#include <vaFlags.h>
#include "vaSession.h"
#include "elements/vaElement.inl"

namespace va {

    inline VaVolume CreateRawVolume(const va::Functions& pfn, VaSession session, VaSystemId systemId, bool restorable) {
        VaVolume volume{};
        VaVolumeCreateInfo createInfo{VA_TYPE_VOLUME_CREATE_INFO};
        createInfo.systemId = systemId;

        VaVolumeCreateWithRestoreConfigExt withRestoreConfig{VA_TYPE_VOLUME_CREATE_WITH_RESTORE_CONFIG_EXT};
        withRestoreConfig.restorable = restorable;

        createInfo.next = &withRestoreConfig;
        CHECK_VA(pfn.vaCreateVolume(session, &createInfo, &volume));
        return volume;
    }

    inline VaVolume RestoreRawVolume(const va::Functions& pfn, VaSession session, VaSystemId systemId, VaUuid volumeRestoreId) {
        VaVolume volume{};
        VaVolumeCreateInfo createInfo{VA_TYPE_VOLUME_CREATE_INFO};
        createInfo.systemId = systemId;

        VaVolumeCreateWithRestoreIdExt withRestoreId{VA_TYPE_VOLUME_CREATE_WITH_RESTORE_ID_EXT};
        withRestoreId.volumeRestoreId = volumeRestoreId;
        createInfo.next = &withRestoreId;

        CHECK_VA(pfn.vaCreateVolume(session, &createInfo, &volume));
        return volume;
    }

    inline Volume::Volume(va::VolumetricApp& app, bool restorable)
        : m_context(app.Context())
        , m_handle(CreateRawVolume(Context().pfn, Context().session.SessionHandle(), Context().session.CurrentSystemId(), restorable))
        , m_associatedSystemId(Context().session.CurrentSystemId())
        , m_container(*this)
        , m_content(*this) {
        CHECK(m_associatedSystemId != 0);
    }

    inline Volume::Volume(va::VolumetricApp& app, VaUuid volumeRestoreId)
        : m_context(app.Context())
        , m_handle(RestoreRawVolume(Context().pfn, Context().session.SessionHandle(), Context().session.CurrentSystemId(), volumeRestoreId))
        , m_associatedSystemId(Context().session.CurrentSystemId())
        , m_container(*this)
        , m_content(*this) {
        CHECK(m_associatedSystemId != 0);
    }

    inline Volume::~Volume() {
        DiscardDispatcherTasks();
        m_elements.Clear();

        if (m_handle != VA_NULL_HANDLE) {
            Context().pfn.vaDestroyVolume(m_handle);
        }
    }

    inline VaVolume Volume::VolumeHandle() const {
        return m_handle;
    }

    inline VolumeContainer& Volume::Container() {
        return m_container;
    }

    inline VolumeContent& Volume::Content() {
        return m_content;
    }

    inline va::VolumetricApp& Volume::App() const {
        return Context().app;
    }

    inline va::detail::AppContext& Volume::Context() const {
        return m_context;
    }

    inline VaVolumeState Volume::State() const {
        return m_state;
    }

    inline bool Volume::IsRunning() const {
        return m_state == VA_VOLUME_STATE_RUNNING;
    }

    inline bool Volume::IsClosed() const {
        return m_state == VA_VOLUME_STATE_CLOSED;
    }

    inline void Invoke(Volume& volume, std::function<void(Volume& volume)> action) {
        if (action) {
            action(volume);
        }
    }

    inline void Volume::VolumeStateChanged(const VaEventVolumeStateChanged& event) {
        CHECK(event.volume == VolumeHandle());
        TRACE("VolumeStateChanged: volume = %d, from %s to %s, action = %s", event.volume, va::ToString(m_state), va::ToString(event.state), va::ToString(event.action));
        m_state = event.state;
        switch (event.action) {
        case VA_VOLUME_STATE_ACTION_ON_READY:
            Update(1);
            break;

        case VA_VOLUME_STATE_ACTION_ON_CLOSE:
            DiscardDispatcherTasks();
            Invoke(*this, onClose);
            Context().session.RemoveVolume(VolumeHandle());
            break;

        case VA_VOLUME_STATE_ACTION_ON_PAUSE:
            Invoke(*this, onPause);
            break;

        case VA_VOLUME_STATE_ACTION_ON_RESUME:
            Invoke(*this, onResume);
            break;
        }
    }

    template <typename TClock, typename Period>
    inline void Volume::RequestUpdateAfter(const std::chrono::duration<TClock, Period>& onDemandUpdateDelay) {
        VaUpdateVolumeRequestInfo requestInfo{VA_TYPE_UPDATE_VOLUME_REQUEST_INFO};
        requestInfo.updateMode = VA_VOLUME_UPDATE_MODE_ON_DEMAND;
        auto durationNanoSeconds = std::chrono::duration_cast<std::chrono::nanoseconds>(onDemandUpdateDelay).count();
        requestInfo.onDemandUpdateDelay = static_cast<VaDuration>(durationNanoSeconds);

        VaUpdateVolumeRequestResult result{VA_TYPE_UPDATE_VOLUME_REQUEST_RESULT};
        CHECK_VA(Context().pfn.vaRequestUpdateVolume(VolumeHandle(), &requestInfo, &result));

        std::lock_guard<std::mutex> lock(m_dispatcherMutex);
        m_volumeUpdateMode = VA_VOLUME_UPDATE_MODE_ON_DEMAND;
    }

    inline void Volume::RequestUpdate(VaVolumeUpdateMode volumeUpdateMode) {
        VaUpdateVolumeRequestInfo requestInfo{VA_TYPE_UPDATE_VOLUME_REQUEST_INFO};
        requestInfo.updateMode = volumeUpdateMode;
        VaUpdateVolumeRequestResult result{VA_TYPE_UPDATE_VOLUME_REQUEST_RESULT};
        CHECK_VA(Context().pfn.vaRequestUpdateVolume(VolumeHandle(), &requestInfo, &result));

        std::lock_guard<std::mutex> lock(m_dispatcherMutex);
        m_volumeUpdateMode = volumeUpdateMode;
    }

    inline const FrameState& Volume::FrameState() const {
        return m_frameState;
    }

    inline void Volume::Update(VaFrameId frameId) {
        CHECK(IsRunning());

        VaUpdateVolumeFrameState frameState{VA_TYPE_UPDATE_VOLUME_FRAME_STATE};
        {
            VaUpdateVolumeBeginInfo beginInfo{VA_TYPE_UPDATE_VOLUME_BEGIN_INFO};
            beginInfo.frameId = frameId;
            CHECK_VA(Context().pfn.vaBeginUpdateVolume(VolumeHandle(), &beginInfo, &frameState));
        }

        // Frame state becomes current at successful return of begin update.
        m_frameState.isCurrent = true;
        m_frameState.frameId = frameId;
        m_frameState.frameTime = frameState.time;
        m_frameState.frameDuration = frameState.duration;

        std::vector<std::function<void()>> tasks;
        {
            std::lock_guard<std::mutex> lock(m_dispatcherMutex);
            tasks.swap(m_dispatchedTasks);
        }

        for (auto& task : tasks) {
            task();
        }

        if (frameId == 1) {
            m_isReady = true;
            if (onReady) {
                m_onReadyDispatched = true;
                Invoke(*this, onReady);
            }
        } else {
            Invoke(*this, onUpdate);
        }

        // Frame state is no longer current before calling end update.
        m_frameState.isCurrent = false;
        {
            VaUpdateVolumeEndInfo endInfo{VA_TYPE_UPDATE_VOLUME_END_INFO};
            endInfo.frameId = frameId;
            CHECK_VA(Context().pfn.vaEndUpdateVolume(VolumeHandle(), &endInfo));
        }
    }

    template <typename TElement>
    inline TElement* Volume::AddElement(std::unique_ptr<TElement>&& element) {
        TElement* ptr = element.get();
        m_elements.AddElement(std::move(element));
        return ptr;
    }

    template <typename TElement, typename... Args>
    inline TElement* Volume::CreateElement(Args&&... args) {
        auto element = std::make_unique<TElement>(*this, std::forward<Args>(args)...);
        return AddElement(std::move(element));
    }

    template <typename TElement>
    inline TElement* Volume::CreateElement() {
        auto element = std::make_unique<TElement>(*this);
        return AddElement(std::move(element));
    }

    inline void Volume::RemoveElement(Element* element) {
        m_elements.RemoveElement(element);
    }

    inline VaUuid Volume::GetRestoreId() const {
        VaUuid restoreId;
        CHECK_VA(Context().pfn.vaGetVolumeRestoreIdExt(VolumeHandle(), &restoreId));
        return restoreId;
    }

    inline void Volume::RequestClose() {
        if (!IsClosed()) {
            CHECK_VA(Context().pfn.vaRequestCloseVolume(VolumeHandle()));
        }
    }

    inline void Volume::DispatchToNextUpdate(std::function<void()>&& task) {
        bool needRequestUpdate = false;
        {
            std::lock_guard<std::mutex> lock(m_dispatcherMutex);
            m_dispatchedTasks.push_back(std::move(task));

            // If not running updates in loop, request another update.
            needRequestUpdate = (m_volumeUpdateMode == VA_VOLUME_UPDATE_MODE_ON_DEMAND);
        }

        if (needRequestUpdate) {
            RequestUpdate();
        }
    }

    inline void Volume::DiscardDispatcherTasks() {
        std::lock_guard<std::mutex> lock(m_dispatcherMutex);
        m_dispatchedTasks.clear();
    }

    inline void Volume::AdaptiveCardActionInvoked(const VaEventAdaptiveCardActionInvokedExt& event) {
        Element* element = m_elements.GetElementOrThrow(event.element);
        element->As<AdaptiveCardElement>().PollAdaptiveCardActionInvokedData();
    }

    inline void Volume::HandleVolumeRestoreResult(const VaEventVolumeRestoreResultExt& event) {
        m_restoreResultValue = event.volumeRestoreResult;
        m_restoreResultReceived = true;
        if (onRestoreResult) {
            m_onRestoreResultDispatched = true;
            onRestoreResult(*this, m_restoreResultValue);
        }
    }

    inline bool Volume::TryDispatchPendingOnReady() {
        // If the volume is ready but onReady wasn't dispatched (because callback was null at the time),
        // and now onReady is hooked up, dispatch it.
        if (m_isReady && !m_onReadyDispatched && onReady) {
            m_onReadyDispatched = true;
            Invoke(*this, onReady);
            return true;
        }
        return false;
    }

    inline bool Volume::TryDispatchPendingOnRestoreResult() {
        // If the restore result was received but onRestoreResult wasn't dispatched (because callback
        // was null at the time), and now onRestoreResult is hooked up, dispatch it.
        if (m_restoreResultReceived && !m_onRestoreResultDispatched && onRestoreResult) {
            m_onRestoreResultDispatched = true;
            onRestoreResult(*this, m_restoreResultValue);
            return true;
        }
        return false;
    }

    inline void Volume::HandleElementAsyncStateChanged(const VaEventElementAsyncStateChanged&) {
        // Query for data that has a list of elements that have changed their async state.
        VaChangedElementsGetInfo getInfo{VA_TYPE_CHANGED_ELEMENTS_GET_INFO};
        getInfo.filterFlags = VA_ELEMENT_CHANGE_FILTER_ASYNC_STATE;

        VaChangedElements changes{VA_TYPE_CHANGED_ELEMENTS};

        // Two call idiom. This call will tell us how many elements are available but we won't get the list of elements.
        CHECK_VA(Context().pfn.vaGetChangedElements(m_handle, &getInfo, &changes));

        while (changes.elementCountOutput > 0) {
            // Now we know how many elements are available, so we can allocate a buffer to hold them.
            auto changedElements = std::vector<VaElement>(changes.elementCountOutput);

            // This buffer will be filled with the actual elements in the second call.
            changes.elementCapacityInput = static_cast<uint32_t>(changedElements.size());
            changes.elements = changedElements.data();

            CHECK_VA(Context().pfn.vaGetChangedElements(m_handle, &getInfo, &changes));

            if (changes.elementCountOutput > 0) {
                for (const auto& elementHandle : changedElements) {
                    Element* element = nullptr;

                    if (m_elements.TryGetElement(elementHandle, &element)) {
                        // If there is a change, this will raise an event on the Element.
                        element->UpdateElementState();
                    }
                }
            }

            // Reset the struct to get new output count, to see if there are any more changes to process.
            changes = {VA_TYPE_CHANGED_ELEMENTS};

            // Two call idiom. This call will tell us how many elements are available but we won't get the list of elements.
            CHECK_VA(Context().pfn.vaGetChangedElements(m_handle, &getInfo, &changes));
        }
    }

    inline VolumeContainer::VolumeContainer(va::Volume& volume)
        : m_volume(volume) {}

    inline void VolumeContainer::EnsureElement() const {
        if (m_handle == VA_NULL_HANDLE) {
            VaElementCreateInfo createInfo{VA_TYPE_ELEMENT_CREATE_INFO};
            createInfo.elementType = VA_ELEMENT_TYPE_VOLUME_CONTAINER;
            CHECK_VA(m_volume.Context().pfn.vaCreateElement(m_volume.VolumeHandle(), &createInfo, &m_handle));
        }
    }

    inline void VolumeContainer::HandleContainerModeChanged(const VaEventVolumeContainerModeChangedExt& event) {
        CHECK(event.volume == m_volume.VolumeHandle());
        CHECK(event.element == m_handle);

        VaVolumeContainerModeFlagsExt changes = m_currentModes ^ event.currentModes;

        if (changes != 0) {
            m_currentModes = event.currentModes;

            if (changes & VA_VOLUME_CONTAINER_MODE_INTERACTIVE_MODE_EXT) {
                if (onInteractiveModeChanged) {
                    try {
                        onInteractiveModeChanged(m_currentModes & VA_VOLUME_CONTAINER_MODE_INTERACTIVE_MODE_EXT);
                    }
                    CATCH_TRACE_IGNORE("HandleContainerPropertiesChanged_onInteractiveModeChanged");
                }
            }

            if (changes & VA_VOLUME_CONTAINER_MODE_ONE_TO_ONE_MODE_EXT) {
                if (onOneToOneModeChanged) {
                    try {
                        onOneToOneModeChanged(m_currentModes & VA_VOLUME_CONTAINER_MODE_ONE_TO_ONE_MODE_EXT);
                    }
                    CATCH_TRACE_IGNORE("HandleContainerPropertiesChanged_onOneToOneModeChanged");
                }
            }

            if (changes & VA_VOLUME_CONTAINER_MODE_SHAREABLE_IN_TEAMS_EXT) {
                if (onSharingInTeamsChanged) {
                    try {
                        onSharingInTeamsChanged(m_currentModes & VA_VOLUME_CONTAINER_MODE_SHAREABLE_IN_TEAMS_EXT);
                    }
                    CATCH_TRACE_IGNORE("HandleContainerPropertiesChanged_onSharingInTeamsChanged");
                }
            }

            if (changes & VA_VOLUME_CONTAINER_MODE_UNBOUNDED_MODE_EXT) {
                if (onUnboundedModeChanged) {
                    try {
                        onUnboundedModeChanged(m_currentModes & VA_VOLUME_CONTAINER_MODE_UNBOUNDED_MODE_EXT);
                    }
                    CATCH_TRACE_IGNORE("HandleContainerPropertiesChanged_onUnboundedModeChanged");
                }
            }

            if (changes & VA_VOLUME_CONTAINER_MODE_SUBPART_MODE_EXT) {
                if (onSubpartModeChanged) {
                    try {
                        onSubpartModeChanged(m_currentModes & VA_VOLUME_CONTAINER_MODE_SUBPART_MODE_EXT);
                    }
                    CATCH_TRACE_IGNORE("HandleContainerPropertiesChanged_onSubpartModeChanged");
                }
            }
        }
    }

    inline void VolumeContainer::SetDisplayName(const char* displayName) {
        if (m_displayName != displayName) {
            m_displayName = displayName;
            EnsureElement();
            CHECK_VA(m_volume.Context().pfn.vaSetElementPropertyString(m_handle, VA_ELEMENT_PROPERTY_VOLUME_CONTAINER_DISPLAY_NAME, displayName));
        }
    }

    inline void VolumeContainer::SetRotationLock(VaVolumeRotationLockFlags rotationLock) {
        if (m_rotationLock != rotationLock) {
            m_rotationLock = rotationLock;
            EnsureElement();
            CHECK_VA(m_volume.Context().pfn.vaSetElementPropertyFlags(m_handle, VA_ELEMENT_PROPERTY_VOLUME_CONTAINER_ROTATION_LOCK, rotationLock));
        }
    }

    inline void VolumeContainer::AllowInteractiveMode(bool allow) {
        auto newMode = allow ? m_allowedMode | VA_VOLUME_CONTAINER_MODE_INTERACTIVE_MODE_EXT : m_allowedMode & ~VA_VOLUME_CONTAINER_MODE_INTERACTIVE_MODE_EXT;
        SetModeCapabilities(newMode);
    }

    inline void VolumeContainer::AllowOneToOneMode(bool allow) {
        auto newMode = allow ? m_allowedMode | VA_VOLUME_CONTAINER_MODE_ONE_TO_ONE_MODE_EXT : m_allowedMode & ~VA_VOLUME_CONTAINER_MODE_ONE_TO_ONE_MODE_EXT;
        SetModeCapabilities(newMode);
    }

    inline void VolumeContainer::AllowSharingInTeams(bool allow) {
        auto newMode = allow ? m_allowedMode | VA_VOLUME_CONTAINER_MODE_SHAREABLE_IN_TEAMS_EXT : m_allowedMode & ~VA_VOLUME_CONTAINER_MODE_SHAREABLE_IN_TEAMS_EXT;
        SetModeCapabilities(newMode);
    }

    inline void VolumeContainer::AllowUnboundedMode(bool allow) {
        auto newMode = allow ? m_allowedMode | VA_VOLUME_CONTAINER_MODE_UNBOUNDED_MODE_EXT : m_allowedMode & ~VA_VOLUME_CONTAINER_MODE_UNBOUNDED_MODE_EXT;
        SetModeCapabilities(newMode);
    }

    inline void VolumeContainer::AllowSubpartMode(bool allow) {
        auto newMode = allow ? m_allowedMode | VA_VOLUME_CONTAINER_MODE_SUBPART_MODE_EXT : m_allowedMode & ~VA_VOLUME_CONTAINER_MODE_SUBPART_MODE_EXT;
        SetModeCapabilities(newMode);
    }

    inline void VolumeContainer::SetModeCapabilities(VaVolumeContainerModeFlagsExt capabilities) {
        if (m_allowedMode != capabilities) {
            m_allowedMode = capabilities;
            EnsureElement();
            CHECK_VA(m_volume.Context().pfn.vaSetElementPropertyFlags(m_handle, VA_ELEMENT_PROPERTY_VOLUME_CONTAINER_MODE_CAPABILITIES_EXT, m_allowedMode));
        }
    }

    inline void VolumeContainer::SetThumbnailModelUri(const char* thumbnailModelUri) {
        if (m_thumbnailModelUri != thumbnailModelUri) {
            m_thumbnailModelUri = thumbnailModelUri;
            EnsureElement();
            CHECK_VA(m_volume.Context().pfn.vaSetElementPropertyString(m_handle, VA_ELEMENT_PROPERTY_VOLUME_CONTAINER_THUMBNAIL_MODEL_URI_EXT, thumbnailModelUri));
        }
    }

    inline void VolumeContainer::SetThumbnailIconUri(const char* thumbnailIconUri) {
        if (m_thumbnailIconUri != thumbnailIconUri) {
            m_thumbnailIconUri = thumbnailIconUri;
            EnsureElement();
            CHECK_VA(m_volume.Context().pfn.vaSetElementPropertyString(m_handle, VA_ELEMENT_PROPERTY_VOLUME_CONTAINER_THUMBNAIL_ICON_URI_EXT, thumbnailIconUri));
        }
    }

    inline VolumeContent::VolumeContent(va::Volume& volume)
        : m_volume(volume) {}

    inline void VolumeContent::EnsureElement() const {
        if (m_handle == VA_NULL_HANDLE) {
            VaElementCreateInfo createInfo{VA_TYPE_ELEMENT_CREATE_INFO};
            createInfo.elementType = VA_ELEMENT_TYPE_VOLUME_CONTENT;
            CHECK_VA(m_volume.Context().pfn.vaCreateElement(m_volume.VolumeHandle(), &createInfo, &m_handle));
        }
    }

    inline void VolumeContent::SetPosition(const VaVector3f& position) {
        if (m_position.x != position.x || m_position.y != position.y || m_position.z != position.z) {
            m_position = position;
            EnsureElement();
            CHECK_VA(m_volume.Context().pfn.vaSetElementPropertyVector3f(m_handle, VA_ELEMENT_PROPERTY_VOLUME_CONTENT_POSITION, &position));
        }
    }

    inline void VolumeContent::SetOrientation(const VaQuaternionf& orientation) {
        if (m_orientation.x != orientation.x || m_orientation.y != orientation.y || m_orientation.z != orientation.z || m_orientation.w != orientation.w) {
            m_orientation = orientation;
            EnsureElement();
            CHECK_VA(m_volume.Context().pfn.vaSetElementPropertyQuaternionf(m_handle, VA_ELEMENT_PROPERTY_VOLUME_CONTENT_ORIENTATION, &orientation));
        }
    }

    inline void VolumeContent::SetSize(float uniformSize) {
        SetSize(VaExtent3Df{uniformSize, uniformSize, uniformSize});
    }

    inline void VolumeContent::SetSize(const VaExtent3Df& size) {
        if (m_size.width != size.width || m_size.height != size.height || m_size.depth != size.depth) {
            m_size = size;
            EnsureElement();
            CHECK_VA(m_volume.Context().pfn.vaSetElementPropertyExtent3Df(m_handle, VA_ELEMENT_PROPERTY_VOLUME_CONTENT_SIZE, &size));
        }
    }

    inline void VolumeContent::SetSizeBehavior(VaVolumeSizeBehavior sizeBehavior) {
        if (m_sizeBehavior != sizeBehavior) {
            m_sizeBehavior = sizeBehavior;
            EnsureElement();
            CHECK_VA(m_volume.Context().pfn.vaSetElementPropertyEnum(m_handle, VA_ELEMENT_PROPERTY_VOLUME_CONTENT_SIZE_BEHAVIOR, sizeBehavior));
        }
    }

    inline float VolumeContent::GetActualScale() const {
        float value = 1;
        EnsureElement();
        CHECK_VA(m_volume.Context().pfn.vaGetElementPropertyFloat(m_handle, VA_ELEMENT_PROPERTY_VOLUME_CONTENT_ACTUAL_SCALE_EXT, &value));
        return value;
    }

    inline VaExtent3Df VolumeContent::GetActualSize() const {
        VaExtent3Df value{};
        EnsureElement();
        CHECK_VA(m_volume.Context().pfn.vaGetElementPropertyExtent3Df(m_handle, VA_ELEMENT_PROPERTY_VOLUME_CONTENT_ACTUAL_SIZE_EXT, &value));
        return value;
    }

    inline VaVector3f VolumeContent::GetActualPosition() const {
        VaVector3f value{};
        EnsureElement();
        CHECK_VA(m_volume.Context().pfn.vaGetElementPropertyVector3f(m_handle, VA_ELEMENT_PROPERTY_VOLUME_CONTENT_ACTUAL_POSITION_EXT, &value));
        return value;
    }

} // namespace va
