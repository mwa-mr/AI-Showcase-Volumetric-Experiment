#pragma once

namespace va {
    inline TextureResource::TextureResource(va::Volume& volume)
        : ElementOfType(CreateVaElement(volume, ElementType), volume) {}

    inline void TextureResource::SetImageUri(const char* value) {
        Context().pfn.vaSetElementPropertyString(ElementHandle(), VA_ELEMENT_PROPERTY_TEXTURE_IMAGE_URI_EXT, value);
        m_currentAsyncState = VA_ELEMENT_ASYNC_STATE_PENDING;
    }

    inline void TextureResource::SetNormalScale(float value) {
        Context().pfn.vaSetElementPropertyFloat(ElementHandle(), VA_ELEMENT_PROPERTY_TEXTURE_NORMAL_SCALE_EXT, value);
    }

    inline void TextureResource::SetOcclusionStrength(float value) {
        Context().pfn.vaSetElementPropertyFloat(ElementHandle(), VA_ELEMENT_PROPERTY_TEXTURE_OCCLUSION_STRENGTH_EXT, value);
    }

} // namespace va
