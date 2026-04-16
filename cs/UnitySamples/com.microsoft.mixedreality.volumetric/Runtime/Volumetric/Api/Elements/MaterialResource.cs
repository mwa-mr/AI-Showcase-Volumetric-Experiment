// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable

namespace Microsoft.MixedReality.Volumetric
{
    using Api = Detail.Api;

    /// <summary>
    /// MaterialResource represents a material that can be applied to a model in the volume.
    /// Application can set various properties of the material such as base color, metallic factor, roughness factor,
    /// and textures for base color, metallic roughness, normal, occlusion, and emissive.
    /// </summary>
    public class MaterialResource : Element
    {
        /// <summary>
        /// Creates a reference to the material resource in the specified model with the given material name.
        /// </summary>
        public MaterialResource(ModelResource model, string materialName)
        : base(VaElementType.MaterialResourceExt, model.Volume,
                  (type, volume) => CreateElement(type, volume))
        {
            Api.CheckResult(Api.vaSetElementPropertyHandle(Handle, Api.VaElementProperty.VA_ELEMENT_PROPERTY_MODEL_REFERENCE, model.Handle));
            Api.CheckResult(Api.vaSetElementPropertyString(Handle, Api.VaElementProperty.VA_ELEMENT_PROPERTY_GLTF2_MATERIAL_NAME_EXT, materialName));
        }

        /// <summary>
        /// Sets the base color factor of the PBR material.
        /// The base color factor is a color value that multiplies to the base color of the material.
        /// The default value is (1.0, 1.0, 1.0, 1.0) which means no change to the base color.
        /// The color value is not-premultiplied with alpha channel and in linear color space.
        /// Here A = 1 means opaque, A = 0 means fully transparent.
        /// </summary>
        public void SetBaseColorFactor(in VaColor4f value)
        {
            this.SetPropertyColor4f(VaElementProperty.MaterialPbrBaseColorFactorExt, value);
        }

        /// <summary>
        /// Sets the metallic factor of the PBR material.
        /// The metallic factor is a value between 0.0 and 1.0 that indicates how metallic the material is.
        /// The default value is 0.0 which means the material is not metallic.
        /// </summary>
        public void SetMetallicFactor(float value)
        {
            this.SetPropertyFloat(VaElementProperty.MaterialPbrMetallicFactorExt, value);
        }

        /// <summary>
        /// Sets the roughness factor of the PBR material.
        /// The roughness factor is a value between 0.0 and 1.0 that indicates how rough the material is.
        /// The default value is 0.0 which means the material is smooth.
        /// </summary>
        public void SetRoughnessFactor(float value)
        {
            this.SetPropertyFloat(VaElementProperty.MaterialPbrRoughnessFactorExt, value);
        }

        /// <summary>
        /// Sets the texture resource for the base color of the PBR material.
        /// The texture is used to modulate the base color of the material.
        /// </summary>
        /// <remarks>
        /// From Gltf 2.0 spec: 
        /// The first three components (RGB) **MUST** be encoded with the sRGB transfer function. 
        /// They specify the base color of the material. If the fourth component (A) is present, 
        /// it represents the linear alpha coverage of the material. Otherwise, the alpha coverage 
        /// is equal to `1.0`. The `material.alphaMode` property specifies how alpha is interpreted. 
        /// The stored texels **MUST NOT** be premultiplied. When undefined, the texture **MUST** 
        /// be sampled as having `1.0` in all components.
        /// </remarks>
        public void SetPbrBaseColorTexture(in TextureResource value)
        {
            this.SetPropertyElement(VaElementProperty.MaterialPbrBaseColorTextureExt, value);
        }

        /// <summary>
        /// Sets the texture resource for the metallic and roughness factors of the PBR material.
        /// The texture is used to modulate the metallic and roughness factors of the material.
        /// </summary>
        /// <remarks>
        /// From Gltf 2.0 spec:
        /// The metalness values are sampled from the B channel. The roughness values are sampled
        ///  from the G channel. These values **MUST** be encoded with a linear transfer function.
        ///  If other channels are present (R or A), they **MUST** be ignored for metallic-roughness
        ///  calculations. When undefined, the texture **MUST** be sampled as having `1.0` in G and B components.
        /// </remarks>
        public void SetPbrMetallicRoughnessTexture(in TextureResource value)
        {
            this.SetPropertyElement(VaElementProperty.MaterialPbrMetallicRoughnessTextureExt, value);
        }

        /// <summary>
        /// Sets the normal texture resource for the PBR material.
        /// </summary>
        /// <remarks>
        /// From Gltf 2.0 spec:
        /// The tangent space normal texture. The texture encodes RGB components with linear 
        /// transfer function. Each texel represents the XYZ components of a normal vector 
        /// in tangent space. The normal vectors use the convention +X is right and +Y is up. 
        /// +Z points toward the viewer. If a fourth component (A) is present, it **MUST** be
        ///  ignored. When undefined, the material does not have a tangent space normal texture.
        /// </remarks>
        public void SetNormalTexture(in TextureResource value)
        {
            this.SetPropertyElement(VaElementProperty.MaterialNormalTextureExt, value);
        }

        /// <summary>
        /// Sets the occlusion texture resource for the PBR material.
        /// </summary>
        /// <remarks>
        /// From Gltf 2.0 spec:
        /// The occlusion values are linearly sampled from the R channel. Higher values indicate 
        /// areas that receive full indirect lighting and lower values indicate no indirect lighting. 
        /// If other channels are present (GBA), they **MUST** be ignored for occlusion calculations. 
        /// When undefined, the material does not have an occlusion texture.
        /// </remarks>
        public void SetOcclusionTexture(in TextureResource value)
        {
            this.SetPropertyElement(VaElementProperty.MaterialOcclusionTextureExt, value);
        }

        /// <summary>
        /// Sets the emissive texture resource for the PBR material.
        /// </summary>
        /// <remarks>
        /// From Gltf 2.0 spec:
        /// It controls the color and intensity of the light being emitted by the material. 
        /// This texture contains RGB components encoded with the sRGB transfer function. 
        /// If a fourth component (A) is present, it **MUST** be ignored. 
        /// When undefined, the texture **MUST** be sampled as having `1.0` in RGB components.
        /// </remarks>
        public void SetEmissiveTexture(in TextureResource value)
        {
            this.SetPropertyElement(VaElementProperty.MaterialEmissiveTextureExt, value);
        }
    }
}
