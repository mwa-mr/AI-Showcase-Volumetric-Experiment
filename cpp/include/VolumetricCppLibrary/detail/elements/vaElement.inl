#pragma once

#include <string>
#include <detail/vaSession.h>

namespace va {
    inline VaElement Element::CreateVaElement(va::Volume& volume, VaElementType elementType) {
        VaElement result;
        {
            VaElementCreateInfo createInfo{VA_TYPE_ELEMENT_CREATE_INFO, nullptr, elementType};
            CHECK_VA(volume.Context().pfn.vaCreateElement(volume.VolumeHandle(), &createInfo, &result));
        }
        return result;
    }

    inline Element::Element(VaElementType type, VaElement handle, va::Volume& volume, VaElementAsyncState defaultAsyncState)
        : m_elementType(type)
        , m_handle(handle)
        , m_volume(volume)
        , m_currentAsyncState(defaultAsyncState) {}

    inline Element::~Element() {
        if (m_handle) {
            Context().pfn.vaDestroyElement(m_handle);
        }
    }

    inline va::detail::AppContext& Element::Context() const {
        return m_volume.Context();
    }

    inline VaVolume Element::VolumeHandle() const {
        return m_volume.VolumeHandle();
    }

    inline VaSession Element::SessionHandle() const {
        return m_volume.Context().session.SessionHandle();
    }

    inline VaElement Element::ElementHandle() const {
        return m_handle;
    }

    inline va::Volume& Element::Volume() const {
        return m_volume;
    }

    inline void Element::UpdateElementState() {
        VaElementAsyncState newAsyncState;
        CHECK_VA(Context().pfn.vaGetElementPropertyEnum(m_handle, VA_ELEMENT_PROPERTY_ASYNC_STATE, (int32_t*)&newAsyncState));

        if (m_currentAsyncState != newAsyncState) {
            m_currentAsyncState = newAsyncState;

            try {
                if (this->onAsyncStateChange) {
                    this->onAsyncStateChange(m_currentAsyncState);
                }
            }
            CATCH_TRACE_IGNORE("UpdateElementState::ExceptionCaughtInCallback");
        }
    }

    inline bool Element::IsReady() const {
        return m_currentAsyncState == VA_ELEMENT_ASYNC_STATE_READY;
    }

    inline bool Element::IsPending() const {
        return m_currentAsyncState == VA_ELEMENT_ASYNC_STATE_PENDING;
    }

    inline bool Element::HasError() const {
        return m_currentAsyncState == VA_ELEMENT_ASYNC_STATE_ERROR;
    }

    inline void Element::GetAsyncErrors(std::function<void(VaElementAsyncError, const char*)> onError) const {
        VaElementAsyncErrorData data{VA_TYPE_ELEMENT_ASYNC_ERROR_DATA};
        while (true) {
            CHECK_VA(Context().pfn.vaGetNextElementAsyncError(m_handle, &data));
            if (data.error == VA_ELEMENT_ASYNC_ERROR_NO_MORE) {
                break;
            }

            try {
                onError(data.error, data.errorMessage);
            }
            CATCH_TRACE_IGNORE("GetAsyncErrors::ExceptionCaughtInCallback");
        }
    }

} // namespace va
