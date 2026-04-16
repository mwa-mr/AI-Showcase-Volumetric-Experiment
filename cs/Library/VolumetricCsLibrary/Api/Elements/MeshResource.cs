// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable

namespace Microsoft.MixedReality.Volumetric
{
    using Detail;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Api = Detail.Api;

    /// <summary>
    /// MeshBufferData represents the data of a mesh buffer in a mesh resource.
    /// </summary>
    public class MeshBufferData
    {
        /// <summary>
        /// Gets the descriptor of the mesh buffer.
        /// The descriptor contains information about the buffer format, item size, and other properties.
        /// </summary>
        public VaMeshBufferDescriptorExt Descriptor { get; private set; }

        /// <summary>
        /// Gets the pointer to the buffer data.
        /// The buffer is a memory block that contains the actual mesh data.
        /// </summary>
        public IntPtr Buffer { get; private set; }

        /// <summary>
        /// Gets the size of the buffer in bytes.
        /// This is the total size of the memory block allocated for the buffer.
        /// </summary>
        public UInt64 ByteSize { get; private set; }

        /// <summary>
        /// Gets the number of items in the buffer.
        /// This is calculated as the total byte size divided by the item size defined in the descriptor.
        /// </summary>
        public UInt64 ItemCount { get; private set; }

        /// <summary>
        /// Gets the size of each item in the buffer in bytes.
        /// This is derived from the buffer format defined in the descriptor.
        /// </summary>
        public uint ItemSize { get; private set; }

        internal MeshBufferData(VaMeshBufferDataExt vaData)
        {
            this.Descriptor = vaData.bufferDescriptor;
            this.Buffer = vaData.buffer;
            this.ByteSize = vaData.bufferByteSize;
            this.ItemSize = Descriptor.bufferFormat.BufferItemSize();
            this.ItemCount = this.ByteSize / this.ItemSize;
        }
    }

    internal static class MeshBufferExtensions
    {
        public static uint BufferItemSize(this VaMeshBufferFormatExt bufferFormat)
        {
            switch (bufferFormat)
            {
                case VaMeshBufferFormatExt.Uint16:
                    return sizeof(UInt16);
                case VaMeshBufferFormatExt.Uint32:
                    return sizeof(UInt32);
                case VaMeshBufferFormatExt.Float:
                    return sizeof(float);
                case VaMeshBufferFormatExt.Float2:
                    return sizeof(float) * 2;
                case VaMeshBufferFormatExt.Float3:
                    return sizeof(float) * 3;
                case VaMeshBufferFormatExt.Float4:
                    return sizeof(float) * 4;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    /// <summary>
    /// MeshResource represents a mesh resource in the volume.
    /// It can be used to access and manipulate mesh buffers associated with a model.
    /// </summary>
    public class MeshResource : Element
    {
        /// <summary>
        /// Creates a reference to the mesh resource in the specified model with the given mesh and primitive indices.
        /// The mesh index and primitive index refers to the mesh in the model following the glTF 2.0 specification.
        /// </summary>
        /// <param name="model">The model resource that contains the mesh.</param>
        /// <param name="meshIndex">The index of the mesh in the model.</param>
        /// <param name="primitiveIndex">The index of the primitive in the mesh.</param>
        /// <param name="decoupleAccessors">If true, decouples the accessors from the mesh resource.  If false, the accessors can be shared.</param>
        /// <param name="initializeData">If true, initializes the mesh buffers with the data from gltf model.  This feature is only available when
        ///  the MeshResource is created and associated with the ModelResource before the gltf model is loaded in the ModeResource.
        ///  otherwise, this parameter is ignored and the mesh buffer won't be initialized.</param>
        public MeshResource(ModelResource model, UInt32 meshIndex, UInt32 primitiveIndex, IReadOnlyList<VaMeshBufferDescriptorExt> descriptors, bool decoupleAccessors, bool initializeData)
        : base(VaElementType.MeshResourceExt, model.Volume,
              (type, volume) => CreateRawVaMeshResource(type, volume, model, null, meshIndex, primitiveIndex, descriptors, decoupleAccessors, initializeData), defaultAsyncState: VaElementAsyncState.Pending)
        {
        }

        /// <summary>
        /// Creates a reference to the mesh resource in the specified model with the given node name, mesh and primitive indices.
        /// The node name refers to a named node in the model's glTF structure.
        /// The mesh index and primitive index refers to the mesh in the model following the glTF 2.0 specification.
        /// </summary>
        /// <param name="model">The model resource that contains the mesh.</param>
        /// <param name="nodeName">The name of the node in the model's glTF structure.</param>
        /// <param name="primitiveIndex">The index of the primitive in the mesh.</param>
        /// <param name="descriptors">The descriptors for the mesh buffers.</param>
        /// <param name="decoupleAccessors">If true, decouples the accessors from the mesh resource.  If false, the accessors can be shared.</param>
        /// <param name="initializeData">If true, initializes the mesh buffers with the data from gltf model.  This feature is only available when
        ///  the MeshResource is created and associated with the ModelResource before the gltf model is loaded in the ModeResource.
        ///  otherwise, this parameter is ignored and the mesh buffer won't be initialized.</param>
        public MeshResource(ModelResource model, string nodeName, UInt32 primitiveIndex, IReadOnlyList<VaMeshBufferDescriptorExt> descriptors, bool decoupleAccessors, bool initializeData)
        : base(VaElementType.MeshResourceExt, model.Volume,
              (type, volume) => CreateRawVaMeshResource(type, volume, model, nodeName, 0, primitiveIndex, descriptors, decoupleAccessors, initializeData), defaultAsyncState: VaElementAsyncState.Pending)
        {
        }

        /// <summary>
        /// This method is used to write mesh buffers for the mesh resource.
        /// It acquires the mesh buffers, executes the provided action on them, and then releases the buffers.
        /// If the mesh buffer cannot be acquired, it returns false and the action is not executed.
        /// This method is a shorthand for writing mesh buffers without resizing the mesh buffers.
        /// </summary>
        public bool WriteMeshBuffers(IReadOnlyList<VaMeshBufferTypeExt> bufferTypes, Action<IReadOnlyList<MeshBufferData>> action)
        {
            return WriteMeshBuffers(bufferTypes, 0, 0, action);
        }

        /// <summary>
        /// This method is used to write mesh buffers for the mesh resource.
        /// It acquires the mesh buffers, executes the provided action on them, and then releases the buffers.
        /// If the mesh buffer cannot be acquired, it returns false and the action is not executed.
        /// This method allows resizing the mesh buffers by specifying the index and vertex counts.
        /// If the index or vertex count is zero, the corresponding buffer type will not be resized.
        /// </summary>
        public bool WriteMeshBuffers(
            IReadOnlyList<VaMeshBufferTypeExt> bufferTypes,
            uint indexCount,
            uint vertexCount,
            Action<IReadOnlyList<MeshBufferData>> action)
        {
            if (!IsReady)
            {
                return false; // Cannot be written to until the element is ready
            }

            IntPtr bufferTypesPtr = IntPtr.Zero;
            IntPtr resizeInfoPtr = IntPtr.Zero;
            IntPtr bufferResultPtr = IntPtr.Zero;
            IntPtr modifiedInfosPtr = IntPtr.Zero;
            try
            {
                Api.VaMeshBufferAcquireInfoExt acquireInfo = new();
                {
                    var sizeOfEnumValue = Marshal.SizeOf<int>();
                    bufferTypesPtr = Marshal.AllocHGlobal(sizeOfEnumValue * bufferTypes.Count);
                    for (int i = 0; i < bufferTypes.Count; i++)
                    {
                        Marshal.WriteInt32(checked((IntPtr)(bufferTypesPtr.ToInt64() + sizeOfEnumValue * i)), (int)bufferTypes[i]);
                    }

                    acquireInfo.type = Api.VaStructureType.VA_TYPE_MESH_BUFFER_ACQUIRE_INFO_EXT;
                    acquireInfo.bufferTypes = bufferTypesPtr;
                    acquireInfo.bufferTypeCount = (uint)bufferTypes.Count;
                }

                Api.VaMeshBufferResizeInfoExt resizeInfo = new();
                if (indexCount > 0 && vertexCount > 0)
                {
                    resizeInfo.type = Api.VaStructureType.VA_TYPE_MESH_BUFFER_RESIZE_INFO_EXT;
                    resizeInfo.indexCount = indexCount;
                    resizeInfo.vertexCount = vertexCount;

                    resizeInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(resizeInfo));
                    Marshal.StructureToPtr(resizeInfo, resizeInfoPtr, false);
                    acquireInfo.next = resizeInfoPtr;
                }

                Api.VaMeshBufferAcquireResultExt acquireResult = new();
                {
                    bufferResultPtr = Marshal.AllocHGlobal(Marshal.SizeOf<VaMeshBufferDataExt>() * bufferTypes.Count);
                    acquireResult.type = Api.VaStructureType.VA_TYPE_MESH_BUFFER_ACQUIRE_RESULT_EXT;
                    acquireResult.buffers = bufferResultPtr;
                    acquireResult.bufferCount = (uint)bufferTypes.Count;
                }

                Api.CheckResult(Api.vaAcquireMeshBufferExt(Handle, acquireInfo, out acquireResult));

                List<MeshBufferData> meshBuffers = new List<MeshBufferData>();
                for (int i = 0; i < bufferTypes.Count; i++)
                {
                    VaMeshBufferDataExt data = Marshal.PtrToStructure<VaMeshBufferDataExt>(bufferResultPtr + i * Marshal.SizeOf<VaMeshBufferDataExt>());
                    meshBuffers.Add(new MeshBufferData(data));
                }

                try
                {
                    action(meshBuffers);
                }
                catch (Exception ex)
                {
                    Trace.LogError(() => $"Exception when the App process mesh buffers: {ex.Message}");

                    // Pass the exception to upper level
                    throw;
                }

                Api.VaMeshBufferReleaseInfoExt releaseInfo = new();
                releaseInfo.type = Api.VaStructureType.VA_TYPE_MESH_BUFFER_RELEASE_INFO_EXT;

                Api.CheckResult(Api.vaReleaseMeshBufferExt(Handle, releaseInfo));
            }
            finally
            {
                Marshal.FreeHGlobal(bufferTypesPtr);
                Marshal.FreeHGlobal(resizeInfoPtr);
                Marshal.FreeHGlobal(bufferResultPtr);
                Marshal.FreeHGlobal(modifiedInfosPtr);
            }
            return true;
        }

        private static IntPtr CreateRawVaMeshResource(
            VaElementType type,
            Volume volume,
            ModelResource model,
            string? nodeName,
            UInt32 meshIndex,
            UInt32 primitiveIndex,
            IReadOnlyList<VaMeshBufferDescriptorExt> descriptors,
            bool decoupleAccessors,
            bool initializeData)
        {
            var createInfo = new Api.VaElementCreateInfo
            {
                type = Api.VaStructureType.VA_TYPE_ELEMENT_CREATE_INFO,
                elementType = Api.VaElementType.VA_ELEMENT_TYPE_MESH_RESOURCE_EXT,
            };

            var indexInfo = new Api.VaGltf2MeshResourceIndexInfoExt
            {
                type = Api.VaStructureType.VA_TYPE_GLTF2_MESH_RESOURCE_INDEX_INFO_EXT,
                modelResource = model.Handle,
                nodeName = nodeName ?? string.Empty,
                meshIndex = meshIndex,
                meshPrimitiveIndex = primitiveIndex,
                decoupleAccessors = decoupleAccessors ? (VaBool32)1 : (VaBool32)0
            };

            var buffersInfo = new Api.VaMeshResourceInitBuffersInfoExt();
            {
                buffersInfo.type = Api.VaStructureType.VA_TYPE_MESH_RESOURCE_INIT_BUFFERS_INFO_EXT;
                buffersInfo.initializeData = initializeData ? (VaBool32)1 : (VaBool32)0;
            }

            IntPtr indexInfoPtr = IntPtr.Zero;
            IntPtr bufferInfoPtr = IntPtr.Zero;
            IntPtr descriptorsPtr = IntPtr.Zero;
            try
            {
                indexInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(indexInfo));
                Marshal.StructureToPtr(indexInfo, indexInfoPtr, false);
                buffersInfo.next = indexInfoPtr;

                var stride = Marshal.SizeOf<VaMeshBufferDescriptorExt>();
                descriptorsPtr = Marshal.AllocHGlobal(stride * descriptors.Count);
                for (int i = 0; i < descriptors.Count; i++)
                {
                    Marshal.StructureToPtr(descriptors[i], checked((IntPtr)(descriptorsPtr.ToInt64() + stride * i)), false);
                }
                buffersInfo.bufferDescriptors = descriptorsPtr;
                buffersInfo.bufferDescriptorCount = (uint)descriptors.Count;
                bufferInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(buffersInfo));

                Marshal.StructureToPtr(buffersInfo, bufferInfoPtr, false);
                createInfo.next = bufferInfoPtr;

                IntPtr handle;
                Api.CheckResult(Api.vaCreateElement(volume.Handle, createInfo, out handle));
                Trace.LogInfo(() => $"create_mesh_resource: element = {handle}");
                return handle;
            }
            finally
            {
                Marshal.FreeHGlobal(indexInfoPtr);
                Marshal.FreeHGlobal(bufferInfoPtr);
                Marshal.FreeHGlobal(descriptorsPtr);
            }
        }
    }
}
