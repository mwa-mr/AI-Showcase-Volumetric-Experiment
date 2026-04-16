
#pragma once

#include <functional>
#include <iterator>

namespace va {
    namespace detail {
        inline uint32_t ItemCount(uint64_t byteCount, size_t sizeOfItem) {
            // TODO: do we need any size math protection here?
            return static_cast<uint32_t>(byteCount / sizeOfItem);
        }

        inline size_t ItemSize(VaMeshBufferFormatExt bufferFormat) {
            switch (bufferFormat) {
            case VA_MESH_BUFFER_FORMAT_UINT16_EXT:
                return sizeof(uint16_t);
            case VA_MESH_BUFFER_FORMAT_UINT32_EXT:
                return sizeof(uint32_t);
            case VA_MESH_BUFFER_FORMAT_FLOAT_EXT:
                return sizeof(float);
            case VA_MESH_BUFFER_FORMAT_FLOAT2_EXT:
                return sizeof(float) * 2;
            case VA_MESH_BUFFER_FORMAT_FLOAT3_EXT:
                return sizeof(float) * 3;
            case VA_MESH_BUFFER_FORMAT_FLOAT4_EXT:
                return sizeof(float) * 4;
            default:
                THROW("Unknown buffer type.");
            }
        }

        template <typename TItemType>
        inline bool HasMatchingFormat(VaMeshBufferFormatExt bufferFormat) {
            switch (bufferFormat) {
            case VA_MESH_BUFFER_FORMAT_UINT16_EXT:
                return std::is_same<TItemType, uint16_t>::value;
            case VA_MESH_BUFFER_FORMAT_UINT32_EXT:
                return std::is_same<TItemType, uint32_t>::value;
            case VA_MESH_BUFFER_FORMAT_FLOAT_EXT:
                return std::is_same<TItemType, float>::value;
            case VA_MESH_BUFFER_FORMAT_FLOAT2_EXT:
                return std::is_same<TItemType, VaVector2f>::value;
            case VA_MESH_BUFFER_FORMAT_FLOAT3_EXT:
                return std::is_same<TItemType, VaVector3f>::value || std::is_same<TItemType, VaColor3f>::value || std::is_same<TItemType, float>::value;
            case VA_MESH_BUFFER_FORMAT_FLOAT4_EXT:
                return std::is_same<TItemType, VaVector4f>::value || std::is_same<TItemType, VaColor4f>::value || std::is_same<TItemType, float>::value;
            default:
                return false;
            }
        }

    } // namespace detail

    inline MeshBufferData::MeshBufferData(VaMeshBufferDescriptorExt descriptor)
        : descriptor(descriptor) {}

    inline bool MeshBufferData::IsReady() const {
        return _buffer != nullptr;
    }

    template <typename TItemType>
    inline TItemType* MeshBufferData::Ptr() {
        CHECK(detail::HasMatchingFormat<TItemType>(descriptor.bufferFormat));
        return reinterpret_cast<TItemType*>(_buffer);
    }

    template <>
    inline void* MeshBufferData::Ptr() {
        return reinterpret_cast<void*>(_buffer);
    }

    inline uint32_t MeshBufferData::Count() const {
        return _count;
    }

    inline void MeshBufferData::SetBufferData(uint8_t* buffer, uint64_t byteSize) {
        _buffer = buffer;
        _byteSize = byteSize;
        _count = detail::ItemCount(byteSize, detail::ItemSize(descriptor.bufferFormat));
    }

    inline bool
    MeshResource::WriteMeshBuffers(std::vector<MeshBufferData>& buffers, uint32_t indexCount, uint32_t vertexCount, std::function<void(std::vector<MeshBufferData>&)> action) {
        if (!IsReady()) {
            // Cannot acquire buffer until the element becomes ready.
            return false;
        }

        std::vector<VaMeshBufferTypeExt> bufferTypes;
        std::transform(
            buffers.begin(), buffers.end(), std::back_insert_iterator<std::vector<VaMeshBufferTypeExt>>{bufferTypes}, [](auto& buffer) { return buffer.descriptor.bufferType; });

        VaMeshBufferAcquireInfoExt acquireInfo = {VA_TYPE_MESH_BUFFER_ACQUIRE_INFO_EXT};
        acquireInfo.bufferTypeCount = (uint32_t)bufferTypes.size();
        acquireInfo.bufferTypes = bufferTypes.data();

        VaMeshBufferResizeInfoExt resizeInfo = {VA_TYPE_MESH_BUFFER_RESIZE_INFO_EXT};
        if (indexCount > 0 && vertexCount > 0) {
            resizeInfo.indexCount = indexCount;
            resizeInfo.vertexCount = vertexCount;
            acquireInfo.next = &resizeInfo;
        }

        // Allocate space for the result. There will be one result per acquireInfo.bufferTypes.
        std::vector<VaMeshBufferDataExt> meshBufferData;
        meshBufferData.resize(acquireInfo.bufferTypeCount);
        VaMeshBufferAcquireResultExt acquireResult = {VA_TYPE_MESH_BUFFER_ACQUIRE_RESULT_EXT};
        acquireResult.bufferCount = acquireInfo.bufferTypeCount;
        acquireResult.buffers = meshBufferData.data();

        CHECK_VA(Context().pfn.vaAcquireMeshBufferExt(ElementHandle(), &acquireInfo, &acquireResult));

        CHECK(buffers.size() == acquireResult.bufferCount);
        for (size_t k = 0; k < buffers.size() && k < acquireResult.bufferCount; ++k) {
            CHECK(buffers[k].descriptor.bufferType == acquireResult.buffers[k].bufferDescriptor.bufferType);
            CHECK(buffers[k].descriptor.bufferFormat == acquireResult.buffers[k].bufferDescriptor.bufferFormat);

            buffers[k].SetBufferData(acquireResult.buffers[k].buffer, acquireResult.buffers[k].bufferByteSize);
        }

        action(buffers);

        VaMeshBufferReleaseInfoExt releaseInfo = {VA_TYPE_MESH_BUFFER_RELEASE_INFO_EXT};
        CHECK_VA(Context().pfn.vaReleaseMeshBufferExt(ElementHandle(), &releaseInfo));

        return true;
    }

    inline VaElement MeshResource::CreateVaGltf2MeshResourceElement(ModelResource& parent,
                                                                    const char* nodeName,
                                                                    uint32_t meshIndex,
                                                                    uint32_t primitiveIndex,
                                                                    const std::vector<VaMeshBufferDescriptorExt>& descriptors,
                                                                    bool decoupleAccessors,
                                                                    bool initializeData) {
        CHECK(parent.Type() == VA_ELEMENT_TYPE_MODEL_RESOURCE);
        VaElement result;
        {
            VaGltf2MeshResourceIndexInfoExt indexInfo{VA_TYPE_GLTF2_MESH_RESOURCE_INDEX_INFO_EXT};
            indexInfo.modelResource = parent.ElementHandle();
            indexInfo.nodeName = nodeName;
            indexInfo.meshIndex = meshIndex;
            indexInfo.meshPrimitiveIndex = primitiveIndex;
            indexInfo.decoupleAccessors = decoupleAccessors;

            VaMeshResourceInitBuffersInfoExt buffersInfo{VA_TYPE_MESH_RESOURCE_INIT_BUFFERS_INFO_EXT};
            buffersInfo.bufferDescriptorCount = (uint32_t)descriptors.size();
            buffersInfo.bufferDescriptors = descriptors.data();
            buffersInfo.initializeData = initializeData;
            indexInfo.next = &buffersInfo;

            VaElementCreateInfo createInfo{VA_TYPE_ELEMENT_CREATE_INFO};
            createInfo.elementType = VA_ELEMENT_TYPE_MESH_RESOURCE_EXT;
            createInfo.next = &indexInfo;
            CHECK_VA(parent.Context().pfn.vaCreateElement(parent.VolumeHandle(), &createInfo, &result));
        }
        return result;
    }
} // namespace va
