// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable

namespace Microsoft.MixedReality.Volumetric
{
    using System;
    using Api = Detail.Api;

    /// <summary>
    /// TextureResource represents a texture resource that can be applied to a material in the volume.
    /// It allows setting properties such as image URI, normal scale, and occlusion strength.
    /// It can be used to change the texture properties of a material resource.
    /// </summary>
    public class TextureResource : Element
    {
        /// <summary>
        /// Creating an TextureResource referencing the texture inside a material resource.
        /// This element can be used to change the texture properties of a material resource.
        /// </summary>
        public TextureResource(Volume volume)
        : base(VaElementType.TextureResourceExt, volume, (type, volume) => CreateElement(type, volume))
        {
        }

        /// <summary>
        /// Sets the URI of the texture image.
        /// The URI should point to an image file that can be used as a texture.
        /// </summary>
        public void SetImageUri(string value)
        {
            Api.CheckResult(Api.vaSetElementPropertyString(Handle, Api.VaElementProperty.VA_ELEMENT_PROPERTY_TEXTURE_IMAGE_URI_EXT, value));
            _asyncState = VaElementAsyncState.Pending;
        }

        /// <summary>
        /// Sets the normal scale for the texture.
        /// The normal scale is a float value that indicates how much the normal map should affect the surface normals.
        /// The default value is 1.0 which means the normal map will have its full effect.
        /// </summary>
        public void SetNormalScale(float value)
        {
            Api.CheckResult(Api.vaSetElementPropertyFloat(Handle, Api.VaElementProperty.VA_ELEMENT_PROPERTY_TEXTURE_NORMAL_SCALE_EXT, value));
        }

        /// <summary>
        /// Sets the occlusion strength for the texture.
        /// The occlusion strength is a float value that indicates how much the occlusion map should affect the surface occlusion.
        /// The default value is 1.0 which means the occlusion map will have its full effect.
        /// </summary>
        public void SetOcclusionStrength(float value)
        {
            Api.CheckResult(Api.vaSetElementPropertyFloat(Handle, Api.VaElementProperty.VA_ELEMENT_PROPERTY_TEXTURE_OCCLUSION_STRENGTH_EXT, value));
        }
    }
}
