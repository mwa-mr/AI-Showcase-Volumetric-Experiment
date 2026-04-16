// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable

namespace Microsoft.MixedReality.Volumetric
{
    /// <summary>
    /// VisualElement represents a 3D object in the volume space.
    /// It can be associated with a model resource, positioned, oriented, scaled, and made visible or invisible.
    /// Visual elements can also reference nodes in other visual elements, allowing for complex hierarchies and relationships.
    /// </summary>
    public class VisualElement : Element
    {
        /// <summary>
        /// Creates a new visual element as an empty transform node (no model resource).
        /// Useful as a grouping parent for other visual elements.
        /// </summary>
        public VisualElement(Volume volume)
            : base(VaElementType.Visual, volume, CreateElement)
        {
        }

        /// <summary>
        /// Creates a new visual element with associated model resource.
        /// </summary>
        public VisualElement(Volume volume, ModelResource model)
            : base(VaElementType.Visual, volume, CreateElement)
        {
            SetVisualResource(model);
        }

        /// <summary>
        /// Creates a new visual element referencing to the named node in the referenced visual element.
        /// </summary>
        public VisualElement(Volume volume, VisualElement visualReference, string nodeName)
            : base(VaElementType.Visual, volume, CreateElement)
        {
            SetVisualReference(visualReference);
            SetNodeName(nodeName);
        }

        /// <summary>
        /// Sets the position of the visual element.
        ///     - When "parent visual" and "named node" are set, the position is relative to the named node in the parent visual element.
        ///     - When "parent visual" is set but "named node" is not, the position is relative to the parent visual element.
        ///     - Otherwise, the "position" is in the volume space.
        /// </summary>
        public void SetPosition(in VaVector3f value)
        {
            SetPropertyVector3f(VaElementProperty.Position, in value);
        }

        /// <summary>
        /// Sets the orientation of the visual element.
        ///     - When "parent visual" and "named node" are set, the orientation is relative to the named node in the parent visual element.
        ///     - When "parent visual" is set but "named node" is not, the orientation is relative to the parent visual element.
        ///     - Otherwise, the "orientation" is in the volume space.
        /// </summary>
        public void SetOrientation(in VaQuaternionf value)
        {
            SetPropertyQuaternionf(VaElementProperty.Orientation, in value);
        }

        /// <summary>
        /// Sets the unitform scale of the visual element.
        /// The scale is applied to the element before applying the orientation and position transforms.
        /// </summary>
        public void SetScale(float value)
        {
            SetPropertyVector3f(VaElementProperty.Scale, new VaVector3f() { x = value, y = value, z = value });
        }

        /// <summary>
        /// Sets the scale of the visual element.
        /// The scale is applied to the element before applying the orientation and position transforms.
        /// </summary>
        public void SetScale(in VaVector3f value)
        {
            SetPropertyVector3f(VaElementProperty.Scale, in value);
        }

        /// <summary>
        /// Sets the visibility of the visual element.
        /// </summary>
        public void SetVisible(bool value)
        {
            SetPropertyBool(VaElementProperty.Visible, value);
        }

        /// <summary>
        /// Associates a model resource with the visual element.
        /// A visual element doesn't render anything by default, until it's associated with a visual resource.
        /// </summary>
        public void SetVisualResource(ModelResource value)
        {
            SetPropertyElement(VaElementProperty.VisualResource, value);
            _asyncState = VaElementAsyncState.Pending;
        }

        /// <summary>
        /// Sets the visual parent of this element. When visual parent is valid, regardless of linking to visual resource,
        /// the transform of this visual element is relative to the parent visual element instead of the volume origin space
        /// </summary>
        public void SetVisualParent(VisualElement value)
        {
            SetPropertyElement(VaElementProperty.VisualParent, value);
            _asyncState = VaElementAsyncState.Pending;
        }

        /// <summary>
        /// Sets the visual reference of this element.  It is used together with node name property
        /// to reference a node in the parent visual element.
        /// </summary>
        public void SetVisualReference(VisualElement value)
        {
            SetPropertyElement(VaElementProperty.VisualReference, value);
            _asyncState = VaElementAsyncState.Pending;
        }

        /// <summary>
        /// Sets the name of the node in the referenced visual element.
        /// When this property is set, the "visual resource" or "visual parent" property is ignored.
        /// If the given node name is not found in the reference visual element,
        ///     this visual element will reference nothing, render nothing and be in HasError state.
        /// </summary>
        public void SetNodeName(string value)
        {
            SetPropertyString(VaElementProperty.Gltf2NodeNameExt, value);
            _asyncState = VaElementAsyncState.Pending;
        }
    }
}
