#pragma once

namespace va {
    namespace detail {
        inline Session::Session(VaSession handle, va::VolumetricApp& app, std::unique_ptr<va::Functions>&& functions, std::unique_ptr<va::Extensions>&& extensions)
            : m_handle(handle)
            , m_functions(std::move(functions))
            , m_extensions(std::move(extensions))
            , m_context(std::make_unique<va::detail::AppContext>(app, *this, *m_extensions.get(), *m_functions.get())) {}

        inline Session::~Session() {
            // Volumes need to be released before destroying the session.
            m_volumes.clear();

            if (m_handle != VA_NULL_HANDLE) {
                Context().pfn.vaDestroySession(m_handle);
            }
        }

        inline VaSession Session::SessionHandle() const {
            return m_handle;
        }

        inline va::detail::AppContext& Session::Context() const {
            return *m_context;
        }

        inline VaSystemId Session::CurrentSystemId() const {
            return m_currentSystemId;
        }

        template <typename TVolume>
        inline TVolume* Session::AddVolume(std::unique_ptr<TVolume>&& volume) {
            static_assert(std::is_base_of<Volume, TVolume>::value, "TVolume must be derived from va::Volume");
            std::lock_guard<std::mutex> lock(m_mutex);
            m_volumes.emplace_back(std::move(volume));
            return static_cast<TVolume*>(m_volumes.back().get());
        }

        inline void Session::RemoveVolume(VaVolume volume) {
            std::lock_guard<std::mutex> lock(m_mutex);
            auto it = std::find_if(m_volumes.begin(), m_volumes.end(), [volume](const auto& v) { return v->VolumeHandle() == volume; });
            if (it == m_volumes.end()) {
                THROW("The given volume cannot be found. Is it already removed?");
            }
            m_volumes.erase(it);
        }

        inline void Session::RemoveRestorableVolume(const VaUuid& restoreId) {
            CHECK_VA(Context().pfn.vaRemoveRestorableVolumeExt(SessionHandle(), &restoreId));
        }

        inline Volume* Session::GetVolumeOrThrow(VaVolume volume) {
            std::lock_guard<std::mutex> lock(m_mutex);
            auto it = std::find_if(m_volumes.begin(), m_volumes.end(), [volume](const auto& v) { return v->VolumeHandle() == volume; });
            if (it == m_volumes.end()) {
                THROW("Volume not found.");
            }
            return it->get();
        }

        inline Volume* Session::GetVolumeOrDefault(VaVolume volume) {
            std::lock_guard<std::mutex> lock(m_mutex);
            auto it = std::find_if(m_volumes.begin(), m_volumes.end(), [volume](const auto& v) { return v->VolumeHandle() == volume; });
            return (it == m_volumes.end()) ? nullptr : it->get();
        }

        inline std::vector<Volume*> Session::AllActiveVolumes() const {
            std::lock_guard<std::mutex> lock(m_mutex);
            std::vector<Volume*> volumes;
            for (auto&& volume : m_volumes) {
                if (!volume->IsClosed())
                    volumes.push_back(volume.get());
            }
            return volumes;
        }

        inline void Session::ProcessEventsThenWait() {
            // Use vaWaitEvent with a 200ms timeout to allow periodic checking for pending work
            // (e.g., late event subscriptions) even when no platform events are arriving.
            constexpr VaDuration timeoutNs = 200'000'000; // 200ms in nanoseconds
            VaEventWaitInfo waitInfo{VA_TYPE_EVENT_WAIT_INFO};
            waitInfo.timeout = timeoutNs;

            VaEventDataBuffer eventData{VA_TYPE_EVENT_DATA_BUFFER};
            VaResult result = CHECK_VA(Context().pfn.vaWaitEvent(SessionHandle(), &waitInfo, &eventData));

            if (result == VA_SUCCESS) {
                ProcessEvent(eventData);
            }
            // VA_TIMEOUT_EXPIRED: eventData is not valid, nothing to process.

            // Always check for pending callbacks (late onStart/onReady subscriptions).
            // This handles both the timeout case (no events) and the event case
            // (events are flowing but subscription happened between frames).
            ProcessPendingCallbacks();
        }

        inline void Session::ProcessPendingCallbacks() {
            // Check for late app onStart subscription
            Context().app.TryDispatchPendingOnStart();

            // Check for late volume onReady and onRestoreResult subscriptions
            for (Volume* volume : AllActiveVolumes()) {
                volume->TryDispatchPendingOnRestoreResult();
                volume->TryDispatchPendingOnReady();
            }
        }

        inline void Session::ProcessEventsThenYield() {
            VaEventDataBuffer eventData = {VA_TYPE_EVENT_DATA_BUFFER};
            while (true) {
                eventData.type = VA_TYPE_EVENT_DATA_BUFFER;
                auto result = CHECK_VA(Context().pfn.vaPollEvent(SessionHandle(), &eventData));
                if (result == VA_EVENT_UNAVAILABLE) {
                    break;
                }
                ProcessEvent(eventData);

                if (eventData.type == VA_TYPE_EVENT_UPDATE_VOLUME) {
                    // Always yield after a volume update.
                    break;
                }
            }

            // Always check for pending callbacks (late onStart/onReady subscriptions)
            // after processing events, regardless of whether any events were available.
            ProcessPendingCallbacks();
        }

        inline void Session::ProcessEvent(const VaEventDataBuffer& eventData) {
            // Allow derived apps to handle custom event types first
            if (Context().app.OnPreprocessEvent(eventData.type, &eventData)) {
                return;
            }

            if (eventData.type == VA_TYPE_EVENT_CONNECTED_SYSTEM_CHANGED) {
                auto event = reinterpret_cast<const VaEventConnectedSystemChanged&>(eventData);
                ProcessConnectedSystemChanged(event);
            } else if (eventData.type == VA_TYPE_EVENT_VOLUME_STATE_CHANGED) {
                auto event = reinterpret_cast<const VaEventVolumeStateChanged&>(eventData);
                auto volume = GetVolumeOrThrow(event.volume);
                TRACE("Volume[%x] state is changed, new state = %s, action = %s", (void*)event.volume, va::ToString(event.state), va::ToString(event.action));
                volume->VolumeStateChanged(event);
            } else if (eventData.type == VA_TYPE_EVENT_UPDATE_VOLUME) {
                auto event = reinterpret_cast<const VaEventUpdateVolume&>(eventData);
                auto volume = GetVolumeOrThrow(event.volume);
                if (volume->IsRunning()) {
                    volume->Update(event.frameId);
                }
            } else if (eventData.type == VA_TYPE_EVENT_ADAPTIVE_CARD_ACTION_INVOKED_EXT) {
                auto event = reinterpret_cast<const VaEventAdaptiveCardActionInvokedExt&>(eventData);
                auto volume = GetVolumeOrThrow(event.volume);
                volume->AdaptiveCardActionInvoked(event);
            } else if (eventData.type == VA_TYPE_EVENT_SESSION_STOPPED) {
                TRACE("%s, VA_TYPE_EVENT_SESSION_STOPPED", __FUNCTION__);
                Context().app.Terminate();
            } else if (eventData.type == VA_TYPE_EVENT_VOLUME_RESTORE_RESULT_EXT) {
                auto event = reinterpret_cast<const VaEventVolumeRestoreResultExt&>(eventData);
                auto volume = GetVolumeOrThrow(event.volume);
                volume->HandleVolumeRestoreResult(event);
            } else if (eventData.type == VA_TYPE_EVENT_VOLUME_RESTORE_REQUEST_EXT) {
                auto event = reinterpret_cast<const VaEventVolumeRestoreRequestExt&>(eventData);
                Context().app.HandleVolumeRestoreRequest(event);
            } else if (eventData.type == VA_TYPE_EVENT_VOLUME_RESTORE_ID_INVALIDATED_EXT) {
                auto event = reinterpret_cast<const VaEventVolumeRestoreIdInvalidatedExt&>(eventData);
                Context().app.HandleVolumeRestoreIdInvalidated(event);
            } else if (eventData.type == VA_TYPE_EVENT_ELEMENT_ASYNC_STATE_CHANGED) {
                auto event = reinterpret_cast<const VaEventElementAsyncStateChanged&>(eventData);
                auto volume = GetVolumeOrThrow(event.volume);
                volume->HandleElementAsyncStateChanged(event);
            } else if (eventData.type == VA_TYPE_EVENT_VOLUME_CONTAINER_MODE_CHANGED_EXT) {
                auto event = reinterpret_cast<const VaEventVolumeContainerModeChangedExt&>(eventData);
                auto volume = GetVolumeOrThrow(event.volume);
                volume->Container().HandleContainerModeChanged(event);
            } else {
                TRACE("%s, type = %s", __FUNCTION__, va::ToString(eventData.type));
            }
        }

        inline void Session::ProcessConnectedSystemChanged(VaEventConnectedSystemChanged& event) {
            if (m_currentSystemId != event.systemId) {
                if (event.systemId != 0) {
                    TRACE("A new system is connected, old = %u, new = %u", m_currentSystemId, event.systemId);
                } else {
                    TRACE("The current system is disconnected, old = %u, new = %u", m_currentSystemId, event.systemId);
                }
                m_currentSystemId = event.systemId;
                Context().app.HandleSystemChanged();
            }
        }

        inline void Session::PollFutures() {
            auto activeVolumes = AllActiveVolumes();
            for (auto& volume : activeVolumes) {
                volume->ProcessFutures();
            }
        }

        inline void Session::RequestStop() {
            Context().pfn.vaRequestStopSession(SessionHandle());
        }
    } // namespace detail
} // namespace va
