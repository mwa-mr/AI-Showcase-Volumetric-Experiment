// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable

namespace Microsoft.MixedReality.Volumetric
{
    /// <summary>
    /// ModelResource represents a 3D model resource that can be used in the volume.
    /// A model resource itself won't be rendered until it is associated with a visual element.
    /// A model resource can be assigned to multiple visual elements, allowing for reuse of the same model being rendered in different places.
    /// </summary>
    public class ModelResource : Element
    {
        /// <summary>
        /// Creates a new model resource in the specified volume with the given URI.
        /// The URI should point to a glTF 2.0 model file.
        /// </summary>
        public ModelResource(Volume volume, string? uri = default)
            : base(VaElementType.ModelResource, volume, CreateElement)
        {
            SetModelUri(uri);
        }

        /// <summary>
        /// Sets the URI of a glTF 2.0 file and asynchronously loads the file into this model resource.
        /// Use the Element.IsReady to check when the model has finished loading.
        /// </summary>
        public void SetModelUri(string? uri)
        {
            SetPropertyString(VaElementProperty.Gltf2ModelUriExt, uri ?? string.Empty);
            _asyncState = VaElementAsyncState.Pending;
        }
    }
}
