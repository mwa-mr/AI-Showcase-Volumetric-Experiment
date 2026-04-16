#pragma once

#include <vaCtor.h>

namespace va {
    class VolumetricApp;
    class Volume;
    namespace detail {
        struct AppContext;
    }

    namespace detail {
        class Session : va::NonMovable {
        public:
            Session(VaSession handle, VolumetricApp& app, std::unique_ptr<va::Functions>&& functions, std::unique_ptr<va::Extensions>&& extensions);
            ~Session() override;

            va::detail::AppContext& Context() const;
            VaSystemId CurrentSystemId() const;

        private:
            friend class va::VolumetricApp;
            friend class va::Volume;
            friend class va::Element;
            friend struct va::detail::AppContext;

            void ProcessEventsThenWait();
            void ProcessEventsThenYield();
            void ProcessEvent(const VaEventDataBuffer& eventData);
            void ProcessConnectedSystemChanged(VaEventConnectedSystemChanged& event);
            void ProcessPendingCallbacks();
            void PollFutures();
            VaSession SessionHandle() const;
            std::vector<Volume*> AllActiveVolumes() const;
            Volume* GetVolumeOrThrow(VaVolume volume);
            Volume* GetVolumeOrDefault(VaVolume volume);

            template <typename TVolume>
            TVolume* AddVolume(std::unique_ptr<TVolume>&& volume);
            void RemoveVolume(VaVolume volume);
            void RequestStop();

            void RemoveRestorableVolume(const VaUuid& restoreId);

        private:
            const std::unique_ptr<va::Functions> m_functions;
            const std::unique_ptr<va::Extensions> m_extensions;
            const std::unique_ptr<va::detail::AppContext> m_context;
            const VaSession m_handle;
            VaSystemId m_currentSystemId = 0;

            mutable std::mutex m_mutex;
            std::vector<std::unique_ptr<va::Volume>> m_volumes;
        };
    } // namespace detail
} // namespace va
