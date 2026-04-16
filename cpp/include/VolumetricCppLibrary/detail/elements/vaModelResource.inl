#pragma once

namespace va {
    inline ModelResource::ModelResource(va::Volume& volume)
        : ElementOfType(CreateVaElement(volume, ElementType), volume) {}

    inline ModelResource::ModelResource(va::Volume& volume, const std::string& modelUri)
        : ElementOfType(CreateVaElement(volume, ElementType), volume) {
        SetModelUri(modelUri.c_str());
    }

    inline void ModelResource::SetModelUri(const char* uri) {
        CHECK_VA(Context().pfn.vaSetElementPropertyString(m_handle, VA_ELEMENT_PROPERTY_GLTF2_MODEL_URI_EXT, uri));
        m_currentAsyncState = VA_ELEMENT_ASYNC_STATE_PENDING;
    }

    inline void ModelResource::SetModelUri(const std::string& uri) {
        SetModelUri(uri.c_str());
    }

} // namespace va
