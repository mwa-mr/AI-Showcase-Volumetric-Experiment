#pragma once

#include <functional>
#include <memory>
#include <string>
#include <unordered_map>
#include <vaCtor.h>

namespace va {
    class Element;
    class Volume;
    namespace detail {
        struct AppContext;
    } // namespace detail
} // namespace va

namespace va {
    class Element : va::NonMovable {
    public:
        ~Element() override;

        VaElementType Type() const {
            return m_elementType;
        }

        template <typename TElement>
        TElement& As() {
            if (TElement::ElementType == m_elementType) {
                return *static_cast<TElement*>(this);
            }
            THROW("Incorrect element type cast.");
        }

        /// <summary>Check if the element is active without any pending async operation or errors.</summary>
        bool IsReady() const;

        /// <summary>Check if the element has pending async operation.</summary>
        bool IsPending() const;

        /// <summary>Check if the element has failed async operation with errors.</summary>
        bool HasError() const;

        /// <summary> Get the async errors if any. </summary>
        /// <remarks>The error is cached only for the frame when HasError() is turned into true.</remarks>
        void GetAsyncErrors(std::function<void(VaElementAsyncError, const char*)> onError) const;

        /// <summary>
        /// Set the callback that is invoked when the async state of the element changes.
        /// </summary>
        std::function<void(const VaElementAsyncState)> onAsyncStateChange;

        bool operator==(const Element& other) const {
            return m_handle == other.m_handle;
        }

    protected:
        // Immutable after creation
        const VaElementType m_elementType;
        va::Volume& m_volume;
        const VaElement m_handle{};

        Element(VaElementType type, VaElement handle, va::Volume& volume, VaElementAsyncState defaultAsyncState = VA_ELEMENT_ASYNC_STATE_READY);
        va::detail::AppContext& Context() const;
        VaSession SessionHandle() const;
        VaElement ElementHandle() const;
        VaVolume VolumeHandle() const;
        va::Volume& Volume() const;

        static VaElement CreateVaElement(va::Volume& volume, VaElementType elementType);

        VaElementAsyncState m_currentAsyncState{VA_ELEMENT_ASYNC_STATE_READY};

    private:
        friend class va::Volume;
        friend class va::detail::ElementList;
        friend struct VisualElement;

        void UpdateElementState();
    };

    template <VaElementType ElementType>
    struct ElementOfType : Element {
        static constexpr VaElementType ElementType = ElementType;

    protected:
        ElementOfType(VaElement handle, va::Volume& volume, VaElementAsyncState defaultAsyncState = VA_ELEMENT_ASYNC_STATE_READY)
            : Element(ElementType, handle, volume, defaultAsyncState) {}
    };

    struct ModelResource;

    struct VisualElement : ElementOfType<VA_ELEMENT_TYPE_VISUAL> {
        explicit VisualElement(va::Volume& volume);

        /// <summary>
        /// Creates a visual element associating with the given model resource.
        /// </summary>
        VisualElement(va::Volume& volume, const ModelResource& modelResource);

        /// <summary>
        /// Creates a visual element referencing a named node in the referenced visual element.
        /// </summary>
        VisualElement(va::Volume& volume, VisualElement& visualReference, const char* nodeName);

        void SetVisible(bool visible);
        void SetPosition(const VaVector3f& position);
        void SetOrientation(const VaQuaternionf& orientation);
        void SetScale(float homogeneousScale);
        void SetScale(const VaVector3f& scale);

        /// <summary>
        /// Associates a model resource with the visual element.
        /// A visual element doesn't render anything by default, until it's associated with a visual resource.
        /// </summary>
        void SetVisualResource(const va::ModelResource& modelResource);

        /// <summary>
        /// Sets the visual parent of this element. When visual parent is valid, regardless of linking to visual resource,
        /// the transform of this visual element is relative to the parent visual element instead of the volume origin space
        /// </summary>
        void SetVisualParent(const va::VisualElement& parentElement);

        /// <summary>
        /// Sets the visual reference of this element.  It is used together with node name property
        /// to reference a node in the parent visual element.
        /// </summary>
        void SetVisualReference(const va::VisualElement& visualReference);

        /// <summary>
        /// Sets the name of the node in the referenced visual element.
        /// When this property is set, the "visual resource" or "visual parent" property is ignored.
        /// If the given node name is not found in the reference visual element,
        ///     this visual element will reference nothing, render nothing and be in HasError state.
        /// </summary>
        void SetNodeName(const char* nodeName);
    };

    struct ModelResource : ElementOfType<VA_ELEMENT_TYPE_MODEL_RESOURCE> {
        explicit ModelResource(va::Volume& volume);
        ModelResource(va::Volume& volume, const std::string& modelUri);

        /// <summary>
        /// Sets the URI of a glTF 2.0 file and asynchronously loads the file into this model resource.
        /// Use the Element::IsReady() to check when the model has finished loading.
        /// </summary>
        void SetModelUri(const char* uri);

        /// <summary>
        /// Sets the URI of a glTF 2.0 file and asynchronously loads the file into this model resource.
        /// Use the Element::IsReady() to check when the model has finished loading.
        /// </summary>
        void SetModelUri(const std::string& uri);

    private:
        friend struct MeshResource;
        friend struct MaterialResource;
        friend struct TextureResource;
    };

    struct MeshBufferData {
        const VaMeshBufferDescriptorExt descriptor{};

        explicit MeshBufferData(VaMeshBufferDescriptorExt descriptor);
        bool IsReady() const;

        template <typename TItemType>
        TItemType* Ptr();

        uint32_t Count() const;

#if _HAS_CXX20
        template <typename TItemType>
        std::span<TItemType> AsSpan() {
            return std::span<TItemType>(Ptr<TItemType>(), Count());
        }
#endif

        void SetBufferData(uint8_t* buffer, uint64_t byteSize);

    private:
        friend struct MeshElement;
        uint8_t* _buffer{};
        uint64_t _byteSize{};
        uint32_t _count{};
    };

    struct MeshResource : ElementOfType<VA_ELEMENT_TYPE_MESH_RESOURCE_EXT> {
        MeshResource(va::Volume& volume,
                     va::ModelResource& parentModel,
                     uint32_t meshIndex,
                     uint32_t primitiveIndex,
                     const std::vector<VaMeshBufferDescriptorExt>& descriptors,
                     bool decoupleAccessors,
                     bool initializeData)
            : ElementOfType(CreateVaGltf2MeshResourceElement(parentModel, nullptr, meshIndex, primitiveIndex, descriptors, decoupleAccessors, initializeData),
                            volume,
                            VA_ELEMENT_ASYNC_STATE_PENDING /*defaultAsyncState*/) {}

        MeshResource(va::Volume& volume,
                     va::ModelResource& parentModel,
                     const std::string& nodeName,
                     uint32_t primitiveIndex,
                     const std::vector<VaMeshBufferDescriptorExt>& descriptors,
                     bool decoupleAccessors,
                     bool initializeData)
            : ElementOfType(CreateVaGltf2MeshResourceElement(parentModel, nodeName.c_str(), 0, primitiveIndex, descriptors, decoupleAccessors, initializeData), volume) {}

        // Acquires the mesh buffers and resize the buffers if required.
        // If buffers are acquired, the action is called with the buffers, and returns true.
        // If the action returns false, the buffers cannot be acquired and the action is skipped.
        bool WriteMeshBuffers(std::vector<MeshBufferData>& buffers, uint32_t indexCount, uint32_t vertexCount, std::function<void(std::vector<MeshBufferData>&)> action);

    private:
        static VaElement CreateVaGltf2MeshResourceElement(ModelResource& parent,
                                                          const char* nodeName,
                                                          uint32_t meshIndex,
                                                          uint32_t primitiveIndex,
                                                          const std::vector<VaMeshBufferDescriptorExt>& descriptors,
                                                          bool decoupleAccessors,
                                                          bool initializeData);
    };

    struct MaterialResource : ElementOfType<VA_ELEMENT_TYPE_MATERIAL_RESOURCE_EXT> {
        MaterialResource(va::Volume& volume, va::ModelResource& modelReference, const char* materialName);

        // At the moment, only PBR materials are supported
        // Will add "SetMaterialType" function when needed
        VaMaterialTypeExt MaterialType() const;

        void SetPbrBaseColorFactor(const VaColor4f& color);
        void SetPbrRoughnessFactor(float value);
        void SetPbrMetallicFactor(float value);

        void SetPbrBaseColorTexture(const va::TextureResource& textureResource);
        void SetPbrMetallicRoughnessTexture(const va::TextureResource& textureResource);
        void SetNormalTexture(const va::TextureResource& textureResource);
        void SetOcclusionTexture(const va::TextureResource& textureResource);
        void SetEmissiveTexture(const va::TextureResource& textureResource);
    };

    struct TextureResource : ElementOfType<VA_ELEMENT_TYPE_TEXTURE_RESOURCE_EXT> {
        TextureResource(va::Volume& volume);

        void SetImageUri(const char* value);
        void SetNormalScale(float value);
        void SetOcclusionStrength(float value);

    private:
        friend struct MaterialResource;
    };

    struct AdaptiveCardElement : ElementOfType<VA_ELEMENT_TYPE_ADAPTIVE_CARD_EXT> {
        typedef std::function<void(const std::string&, const std::string&)> ActionCallback;

        AdaptiveCardElement(va::Volume& volume)
            : ElementOfType(CreateVaElement(volume, ElementType), volume) {}

        void SetTemplate(const std::string& jsonTemplate);
        void SetData(const std::string& jsonData);
        void SetPlacement(int32_t placement);
        void SetVisibilityBehavior(int32_t behavior);
        void SetBackplateVisible(bool visible);

        /// <summary>
        /// Sets the callback when an action on the adaptive card is invoked.
        /// </summary>
        ActionCallback onAction;

    private:
        friend class va::Volume;
        void PollAdaptiveCardActionInvokedData();
    };

} // namespace va
