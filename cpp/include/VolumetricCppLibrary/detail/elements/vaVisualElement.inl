#pragma once

namespace va {

    inline VisualElement::VisualElement(va::Volume& volume)
        : ElementOfType(CreateVaElement(volume, ElementType), volume) {}

    inline VisualElement::VisualElement(va::Volume& volume, const ModelResource& modelResource)
        : ElementOfType(CreateVaElement(volume, ElementType), volume) {
        SetVisualResource(modelResource);
    }

    inline VisualElement::VisualElement(va::Volume& volume, VisualElement& visualReference, const char* nodeName)
        : ElementOfType(CreateVaElement(volume, ElementType), volume) {
        SetVisualReference(visualReference);
        SetNodeName(nodeName);
    }

    inline void VisualElement::SetVisible(bool visible) {
        CHECK_VA(Context().pfn.vaSetElementPropertyBool(m_handle, VA_ELEMENT_PROPERTY_VISIBLE, visible));
    }

    inline void VisualElement::SetPosition(const VaVector3f& position) {
        CHECK_VA(Context().pfn.vaSetElementPropertyVector3f(m_handle, VA_ELEMENT_PROPERTY_POSITION, &position));
    }

    inline void VisualElement::SetOrientation(const VaQuaternionf& orientation) {
        CHECK_VA(Context().pfn.vaSetElementPropertyQuaternionf(m_handle, VA_ELEMENT_PROPERTY_ORIENTATION, &orientation));
    }

    inline void VisualElement::SetScale(float homogeneousScale) {
        SetScale(VaVector3f{homogeneousScale, homogeneousScale, homogeneousScale});
    }

    inline void VisualElement::SetScale(const VaVector3f& scale) {
        CHECK_VA(Context().pfn.vaSetElementPropertyVector3f(m_handle, VA_ELEMENT_PROPERTY_SCALE, &scale));
    }

    inline void VisualElement::SetVisualResource(const va::ModelResource& modelResource) {
        CHECK_VA(Context().pfn.vaSetElementPropertyHandle(m_handle, VA_ELEMENT_PROPERTY_VISUAL_RESOURCE, modelResource.ElementHandle()));
        m_currentAsyncState = VA_ELEMENT_ASYNC_STATE_PENDING;
    }

    inline void VisualElement::SetVisualParent(const va::VisualElement& parentVisual) {
        CHECK_VA(Context().pfn.vaSetElementPropertyHandle(m_handle, VA_ELEMENT_PROPERTY_VISUAL_PARENT, parentVisual.ElementHandle()));
        m_currentAsyncState = VA_ELEMENT_ASYNC_STATE_PENDING;
    }

    inline void VisualElement::SetVisualReference(const va::VisualElement& visualReference) {
        CHECK_VA(Context().pfn.vaSetElementPropertyHandle(m_handle, VA_ELEMENT_PROPERTY_VISUAL_REFERENCE, visualReference.ElementHandle()));
        m_currentAsyncState = VA_ELEMENT_ASYNC_STATE_PENDING;
    }

    inline void VisualElement::SetNodeName(const char* nodeName) {
        CHECK_VA(Context().pfn.vaSetElementPropertyString(m_handle, VA_ELEMENT_PROPERTY_GLTF2_NODE_NAME_EXT, nodeName));
        m_currentAsyncState = VA_ELEMENT_ASYNC_STATE_PENDING;
    }

} // namespace va
