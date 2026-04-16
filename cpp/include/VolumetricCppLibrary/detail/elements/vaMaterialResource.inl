#pragma once

namespace va {
    inline MaterialResource::MaterialResource(va::Volume& volume, va::ModelResource& modelReference, const char* materialName)
        : ElementOfType(CreateVaElement(volume, ElementType), volume) {
        const auto& pfn = modelReference.Context().pfn;
        CHECK_VA(pfn.vaSetElementPropertyHandle(ElementHandle(), VA_ELEMENT_PROPERTY_MODEL_REFERENCE, modelReference.ElementHandle()));
        CHECK_VA(pfn.vaSetElementPropertyString(ElementHandle(), VA_ELEMENT_PROPERTY_GLTF2_MATERIAL_NAME_EXT, materialName));
    }

    inline VaMaterialTypeExt MaterialResource::MaterialType() const {
        int32_t result;
        Context().pfn.vaGetElementPropertyEnum(ElementHandle(), VA_ELEMENT_PROPERTY_MATERIAL_TYPE_EXT, &result);
        return static_cast<VaMaterialTypeExt>(result);
    }

    inline void MaterialResource::SetPbrBaseColorFactor(const VaColor4f& value) {
        Context().pfn.vaSetElementPropertyColor4f(ElementHandle(), VA_ELEMENT_PROPERTY_MATERIAL_PBR_BASE_COLOR_FACTOR_EXT, &value);
    }

    inline void MaterialResource::SetPbrRoughnessFactor(float value) {
        Context().pfn.vaSetElementPropertyFloat(ElementHandle(), VA_ELEMENT_PROPERTY_MATERIAL_PBR_ROUGHNESS_FACTOR_EXT, value);
    }

    inline void MaterialResource::SetPbrMetallicFactor(float value) {
        Context().pfn.vaSetElementPropertyFloat(ElementHandle(), VA_ELEMENT_PROPERTY_MATERIAL_PBR_METALLIC_FACTOR_EXT, value);
    }

    inline void MaterialResource::SetPbrBaseColorTexture(const va::TextureResource& textureResource) {
        Context().pfn.vaSetElementPropertyHandle(ElementHandle(), VA_ELEMENT_PROPERTY_MATERIAL_PBR_BASE_COLOR_TEXTURE_EXT, textureResource.ElementHandle());
    }

    inline void MaterialResource::SetPbrMetallicRoughnessTexture(const va::TextureResource& textureResource) {
        Context().pfn.vaSetElementPropertyHandle(ElementHandle(), VA_ELEMENT_PROPERTY_MATERIAL_PBR_METALLIC_ROUGHNESS_TEXTURE_EXT, textureResource.ElementHandle());
    }

    inline void MaterialResource::SetNormalTexture(const va::TextureResource& textureResource) {
        Context().pfn.vaSetElementPropertyHandle(ElementHandle(), VA_ELEMENT_PROPERTY_MATERIAL_NORMAL_TEXTURE_EXT, textureResource.ElementHandle());
    }

    inline void MaterialResource::SetOcclusionTexture(const va::TextureResource& textureResource) {
        Context().pfn.vaSetElementPropertyHandle(ElementHandle(), VA_ELEMENT_PROPERTY_MATERIAL_OCCLUSION_TEXTURE_EXT, textureResource.ElementHandle());
    }

    inline void MaterialResource::SetEmissiveTexture(const va::TextureResource& textureResource) {
        Context().pfn.vaSetElementPropertyHandle(ElementHandle(), VA_ELEMENT_PROPERTY_MATERIAL_EMISSIVE_TEXTURE_EXT, textureResource.ElementHandle());
    }

} // namespace va
