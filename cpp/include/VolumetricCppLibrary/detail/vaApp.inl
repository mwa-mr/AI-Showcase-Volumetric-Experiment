#pragma once

#include <vaError.h>
#include "vaSession.h"

namespace va {
    namespace detail {
        PFN_vaGetFunctionPointer LoadRuntime(VaVersion appApiVersion);
    } // namespace detail

    inline VolumetricApp::VolumetricApp(AppCreateInfo createInfo, VaSession session, std::unique_ptr<va::Functions>&& functions, std::unique_ptr<va::Extensions>&& extensions)
        : m_createInfo(std::move(createInfo))
        , m_session(std::make_unique<va::detail::Session>(session, *this, std::move(functions), std::move(extensions))) {}

    template <typename TApp>
    inline std::unique_ptr<TApp> CreateVolumetricApp(AppCreateInfo&& createInfo) {
        PFN_vaGetFunctionPointer getFunctionPointer = va::detail::LoadRuntime(createInfo.apiVersion);
        if (!getFunctionPointer) {
            TRACE("ERROR: No available session satisfies the loader requirements.");
            return nullptr;
        }

        PFN_vaEnumerateExtensions vaEnumerateExtensions{};
        PFN_vaCreateSession vaCreateSession{};
        CHECK_VA(getFunctionPointer(0, "vaEnumerateExtensions", (PFN_vaVoidFunction*)&vaEnumerateExtensions));
        CHECK_VA(getFunctionPointer(0, "vaCreateSession", (PFN_vaVoidFunction*)&vaCreateSession));
        THROW_IF_NULL(vaEnumerateExtensions);
        THROW_IF_NULL(vaCreateSession);

        uint32_t count = 0;
        CHECK_VA(vaEnumerateExtensions(0, &count, nullptr));
        std::vector<VaExtensionProperties> supportedExtensions(count, {VA_TYPE_EXTENSION_PROPERTIES});
        CHECK_VA(vaEnumerateExtensions((uint32_t)supportedExtensions.size(), &count, supportedExtensions.data()));

        // Filter the required and optional extensions based on the supported extensions of the session.
        // Only retain the extensions that are supported by the session.
        std::unique_ptr<Extensions> extensions = std::make_unique<Extensions>(supportedExtensions, createInfo.requiredExtensions, createInfo.optionalExtensions);

        // If there are missing required extensions, the app cannot be created.
        // Throw the same error as the session would throw, should the missing extensions are passed to it.
        if (!extensions->MissingRequiredExtensions.empty()) {
            TRACE("ERROR: missing required extensions:");
            for (const auto& ext : extensions->MissingRequiredExtensions) {
                TRACE("  - %s", ext);
            }
            THROW_VARESULT(VA_ERROR_EXTENSION_NOT_PRESENT, "vaCreateSession: Missing required extensions.");
        }

        VaSession session = VA_NULL_HANDLE;
        {
            VaSessionCreateInfo vaCreateInfo = {VA_TYPE_SESSION_CREATE_INFO};
            vaCreateInfo.applicationInfo.apiVersion = createInfo.apiVersion;
            createInfo.applicationName.copy(vaCreateInfo.applicationInfo.applicationName, VA_MAX_APPLICATION_NAME_SIZE);
            std::string("Volumetric C++ Library").copy(vaCreateInfo.applicationInfo.libraryName, VA_MAX_LIBRARY_NAME_SIZE);
            vaCreateInfo.applicationInfo.applicationVersion = 1;
            vaCreateInfo.waitForSystemBehavior = createInfo.waitForSystemBehavior;
            vaCreateInfo.enabledExtensionCount = (uint32_t)extensions->EnabledExtensions.size();
            vaCreateInfo.enabledExtensionNames = extensions->EnabledExtensions.data();

            VaSessionCreateWithVolumeRestoreBehaviorExt volumeRestoreBehaviorInfo = {VA_TYPE_SESSION_CREATE_WITH_VOLUME_RESTORE_BEHAVIOR_EXT};
            volumeRestoreBehaviorInfo.restoreBehavior = createInfo.volumeRestoreBehavior;
            vaCreateInfo.next = &volumeRestoreBehaviorInfo;

            CHECK_VA(vaCreateSession(&vaCreateInfo, &session));
        }

        std::unique_ptr<Functions> functions = std::make_unique<Functions>(session, getFunctionPointer);

        return std::make_unique<TApp>(std::move(createInfo), session, std::move(functions), std::move(extensions));
    }

    inline std::unique_ptr<VolumetricApp> CreateVolumetricApp(AppCreateInfo&& createInfo) {
        return CreateVolumetricApp<VolumetricApp>(std::move(createInfo));
    }

    inline va::detail::AppContext& VolumetricApp::Context() const {
        return m_session->Context();
    }

    inline VaSession VolumetricApp::SessionHandle() const {
        return m_session->SessionHandle();
    }

    inline int VolumetricApp::Run() {
        while (ShouldContinue()) {
            try {
                m_session->ProcessEventsThenWait();
            } catch (const std::exception& e) {
                HandleFatalError(e.what());
            } catch (...) {
                HandleFatalError("Unknown exception occurred.");
            }
        };
        Terminate();
        return m_exitCode;
    }

    inline int VolumetricApp::Run(std::function<void(va::VolumetricApp&)>&& _onStarted) {
        onStart = std::move(_onStarted);
        return Run();
    }

    inline std::future<void> VolumetricApp::RunAsync() {
        return std::async(std::launch::async, [this]() { Run(); });
    }

    inline bool VolumetricApp::IsStarted() const {
        return m_started;
    }

    inline bool VolumetricApp::IsStopped() const {
        return m_stopped;
    }

    inline bool VolumetricApp::IsConnected() const {
        return m_session->CurrentSystemId() != 0;
    }

    inline bool VolumetricApp::PollEvents() {
        if (ShouldContinue()) {
            try {
                m_session->ProcessEventsThenYield();
            } catch (const std::exception& e) {
                HandleFatalError(e.what());
                return false;
            } catch (...) {
                HandleFatalError("Unknown exception occurred.");
                return false;
            }
            return true;
        } else {
            Terminate();
            return false;
        }
    }

    template <typename F, typename... Args>
    inline void Invoke(F&& callback, Args&&... args) {
        if (callback) {
            std::forward<F>(callback)(std::forward<Args>(args)...);
        }
    }

    inline bool VolumetricApp::ShouldContinue() const {
        if (m_exitCode != 0) {
            return false; // Exit code is set, fatal error happened, stop the app.
        }

        if (m_stopped) {
            return false; // App is already stopped and there's no turning back.
        }

        if (!m_started) {
            return true; // App is not yet initialized, wait for the first system connection.
        }

        return true;
    }

    inline void VolumetricApp::HandleSystemChanged() {
        if (m_session->CurrentSystemId() != 0) {
            if (!m_started) {
                m_started = true;
                if (onStart) {
                    m_onStartDispatched = true;
                    Invoke(onStart, *this);
                }
            } else {
                Invoke(onReconnect, *this);
            }
        } else {
            Invoke(onDisconnect, *this);
        }
    }

    inline void VolumetricApp::RequestExit() {
        m_session->RequestStop();
    }

    template <typename TVolume, typename... Args>
    inline TVolume* VolumetricApp::CreateVolume(Args&&... args) {
        return m_session->AddVolume(std::make_unique<TVolume>(*this, std::forward<Args>(args)...));
    }

    inline void VolumetricApp::RemoveVolume(Volume* volume) {
        m_session->RemoveVolume(volume->VolumeHandle());
    }

    inline void VolumetricApp::RemoveRestorableVolume(const VaUuid& restoreId) {
        m_session->RemoveRestorableVolume(restoreId);
    }

    inline void VolumetricApp::HandleVolumeRestoreRequest(const VaEventVolumeRestoreRequestExt& event) {
        Invoke(onRestoreVolumeRequest, *this, event.volumeRestoreId);
    }

    inline void VolumetricApp::HandleVolumeRestoreIdInvalidated(const VaEventVolumeRestoreIdInvalidatedExt& event) {
        Invoke(onVolumeRestoreIdInvalidated, *this, event.volumeRestoreId);
    }

    inline void VolumetricApp::Terminate() {
        // There's no turning back after this point.
        // This App object is terminated and can only be destroyed.
        bool expected = false;
        if (m_stopped.compare_exchange_strong(expected, true)) {
            if (m_exitCode != 0) {
                Invoke(onFatalError, m_errorMessage.c_str());
                return; // Terminated due to a fatal error, skip cleanup.
            }

            for (auto& volume : m_session->AllActiveVolumes()) {
                m_session->RemoveVolume(volume->VolumeHandle());
            }

            Invoke(onStop);
        }
    }

    inline void VolumetricApp::HandleFatalError(const char* errorMessage) {
        TRACE("FATAL ERROR: %s", errorMessage);

        int expected = 0;
        if (m_exitCode.compare_exchange_strong(expected, -1)) {
            // Only store the first fatal error and exit app with error.
            m_errorMessage = errorMessage;
        }
    }

    inline bool VolumetricApp::OnPreprocessEvent(VaStructureType /*eventType*/, const void* /*eventData*/) {
        // Default implementation does not handle any events
        return false;
    }

    inline bool VolumetricApp::TryDispatchPendingOnStart() {
        // If the app is started but onStart wasn't dispatched (because callback was null at the time),
        // and now onStart is hooked up, dispatch it.
        if (m_started && !m_onStartDispatched && onStart) {
            m_onStartDispatched = true;
            Invoke(onStart, *this);
            return true;
        }
        return false;
    }
} // namespace va
