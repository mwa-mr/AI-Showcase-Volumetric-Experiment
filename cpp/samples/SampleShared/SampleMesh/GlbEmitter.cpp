#include <string>
#include <filesystem>
#include "GlbEmitter.h"

#define TINYGLTF_NO_INCLUDE_JSON
#define TINYGLTF_IMPLEMENTATION
#define STB_IMAGE_IMPLEMENTATION
#define STB_IMAGE_WRITE_IMPLEMENTATION
#include <nlohmann/json.hpp>
#include <tinygltf/tiny_gltf.h>

#include <SampleMesh/MeshData.h>
#include <vaError.h>   // For TRACE
#include <vaWindows.h> // For GetLocalAssetPath

static std::string s_normalMapFilename = "WaveNormalMap.png";

// Function to generate a temporary file path with .glb extension
std::string GenerateTempGLBFilePath() {
    wchar_t tempFilePath[MAX_PATH];

    // Get the path of the temporary folder
    DWORD dwRetVal = GetTempPathW(MAX_PATH, tempFilePath);
    if (dwRetVal == 0 || dwRetVal > MAX_PATH) {
        TRACE("Error: Failed to retrieve temporary folder path.");
        return "";
    }

    // Generate a unique temporary file name with .glb extension
    wchar_t tempFileName[MAX_PATH];
    UINT uRetVal = GetTempFileNameW(tempFilePath, L"GLB", 0, tempFileName);
    if (uRetVal == 0) {
        TRACE("Error: Failed to generate temporary file name.");
        return "";
    }

    // Replace .glb extension to the generated file name
    auto path = std::filesystem::path(tempFileName).replace_extension(".glb");
    std::filesystem::rename(tempFileName, path);
    return va::wide_to_utf8(path.wstring());
}

void EmitGLB(const std::vector<sample::MeshDataRef>& meshRefs, const std::string& outputPath, bool doubleSided, bool shareAttributeAccessors) {
    tinygltf::Model model;

    tinygltf::Scene scene;

    const int bufferViewCount = 6;
    const int accessorCount = 6;

    for (int i = 0; i < meshRefs.size(); i++) {
        tinygltf::Buffer buffer;

        const auto& meshRef = meshRefs[i];

        // Calculate total buffer size and allocate buffer memory
        const size_t totalBufferSize = meshRef.indexCount * sizeof(uint32_t)     // index
                                     + meshRef.vertexCount * sizeof(VaVector3f)  // position
                                     + meshRef.vertexCount * sizeof(VaVector3f)  // normal
                                     + meshRef.vertexCount * sizeof(VaVector4f)  // tangent
                                     + meshRef.vertexCount * sizeof(VaColor4f)   // color
                                     + meshRef.vertexCount * sizeof(VaVector2f); // uv

        buffer.data.resize(totalBufferSize);

        size_t offset = 0;

        // Copy index data
        std::memcpy(buffer.data.data() + offset, meshRef.indexBuffer.data(), meshRef.indexCount * sizeof(uint32_t));
        offset += meshRef.indexCount * sizeof(uint32_t);

        // Copy position data
        std::memcpy(buffer.data.data() + offset, meshRef.positionBuffer.data(), meshRef.vertexCount * sizeof(VaVector3f));
        offset += meshRef.vertexCount * sizeof(VaVector3f);

        // Copy normal data
        std::memcpy(buffer.data.data() + offset, meshRef.normalBuffer.data(), meshRef.vertexCount * sizeof(VaVector3f));
        offset += meshRef.vertexCount * sizeof(VaVector3f);

        // Copy tangent data
        std::memcpy(buffer.data.data() + offset, meshRef.tangentBuffer.data(), meshRef.vertexCount * sizeof(VaVector4f));
        offset += meshRef.vertexCount * sizeof(VaVector4f);

        // Copy color data
        std::memcpy(buffer.data.data() + offset, meshRef.colorBuffer.data(), meshRef.vertexCount * sizeof(VaColor4f));
        offset += meshRef.vertexCount * sizeof(VaColor4f);

        // Copy uv data
        std::memcpy(buffer.data.data() + offset, meshRef.uvBuffer.data(), meshRef.vertexCount * sizeof(VaVector2f));

        // Add buffer to the model
        model.buffers.push_back(buffer);

        const int bufferIndex = i;

        // Configure buffer views
        tinygltf::BufferView indexBufferView;
        indexBufferView.buffer = bufferIndex;
        indexBufferView.byteOffset = 0;
        indexBufferView.byteLength = meshRef.indexCount * sizeof(uint32_t);
        indexBufferView.target = TINYGLTF_TARGET_ELEMENT_ARRAY_BUFFER;
        model.bufferViews.push_back(indexBufferView);
        const int indexBufferViewIndex = bufferViewCount * i + 0;

        tinygltf::BufferView positionBufferView;
        positionBufferView.buffer = bufferIndex;
        positionBufferView.byteOffset = indexBufferView.byteLength;
        positionBufferView.byteLength = meshRef.vertexCount * sizeof(VaVector3f);
        positionBufferView.target = TINYGLTF_TARGET_ARRAY_BUFFER;
        model.bufferViews.push_back(positionBufferView);
        const int positionBufferViewIndex = bufferViewCount * i + 1;

        tinygltf::BufferView normalBufferView;
        normalBufferView.buffer = bufferIndex;
        normalBufferView.byteOffset = positionBufferView.byteOffset + positionBufferView.byteLength;
        normalBufferView.byteLength = meshRef.vertexCount * sizeof(VaVector3f);
        normalBufferView.target = TINYGLTF_TARGET_ARRAY_BUFFER;
        model.bufferViews.push_back(normalBufferView);
        const int normalBufferViewIndex = bufferViewCount * i + 2;

        tinygltf::BufferView tangentBufferView;
        tangentBufferView.buffer = bufferIndex;
        tangentBufferView.byteOffset = normalBufferView.byteOffset + normalBufferView.byteLength;
        tangentBufferView.byteLength = meshRef.vertexCount * sizeof(VaVector4f);
        tangentBufferView.target = TINYGLTF_TARGET_ARRAY_BUFFER;
        model.bufferViews.push_back(tangentBufferView);
        const int tangentBufferViewIndex = bufferViewCount * i + 3;

        tinygltf::BufferView colorBufferView;
        colorBufferView.buffer = bufferIndex;
        colorBufferView.byteOffset = tangentBufferView.byteOffset + tangentBufferView.byteLength;
        colorBufferView.byteLength = meshRef.vertexCount * sizeof(VaColor4f);
        colorBufferView.target = TINYGLTF_TARGET_ARRAY_BUFFER;
        model.bufferViews.push_back(colorBufferView);
        const int colorBufferViewIndex = bufferViewCount * i + 4;

        tinygltf::BufferView uvBufferView;
        uvBufferView.buffer = bufferIndex;
        uvBufferView.byteOffset = colorBufferView.byteOffset + colorBufferView.byteLength;
        uvBufferView.byteLength = meshRef.vertexCount * sizeof(VaVector2f);
        uvBufferView.target = TINYGLTF_TARGET_ARRAY_BUFFER;
        model.bufferViews.push_back(uvBufferView);
        const int uvBufferViewIndex = bufferViewCount * i + 5;

        // Configure accessors
        tinygltf::Accessor indexAccessor;
        indexAccessor.bufferView = indexBufferViewIndex;
        indexAccessor.byteOffset = 0;
        indexAccessor.componentType = TINYGLTF_COMPONENT_TYPE_UNSIGNED_INT;
        indexAccessor.count = meshRef.indexCount;
        indexAccessor.type = TINYGLTF_TYPE_SCALAR;
        model.accessors.push_back(indexAccessor);
        const int indexAccessorIndex = accessorCount * i + 0;

        tinygltf::Accessor positionAccessor;
        positionAccessor.bufferView = positionBufferViewIndex;
        positionAccessor.byteOffset = 0;
        positionAccessor.componentType = TINYGLTF_COMPONENT_TYPE_FLOAT;
        positionAccessor.count = meshRef.vertexCount;
        positionAccessor.type = TINYGLTF_TYPE_VEC3;

        // Calculate min and max values for the position accessor
        VaVector3f minPos = {FLT_MAX, FLT_MAX, FLT_MAX};
        VaVector3f maxPos = {-FLT_MAX, -FLT_MAX, -FLT_MAX};

        for (const auto& pos : meshRef.positionBuffer) {
            minPos.x = std::min(minPos.x, pos.x);
            minPos.y = std::min(minPos.y, pos.y);
            minPos.z = std::min(minPos.z, pos.z);
            maxPos.x = std::max(maxPos.x, pos.x);
            maxPos.y = std::max(maxPos.y, pos.y);
            maxPos.z = std::max(maxPos.z, pos.z);
        }

        // Set min and max values for the position accessor
        positionAccessor.minValues = {minPos.x, minPos.y, minPos.z};
        positionAccessor.maxValues = {maxPos.x, maxPos.y, maxPos.z};

        model.accessors.push_back(positionAccessor);
        const int positionAccessorIndex = shareAttributeAccessors ? 1 : accessorCount * i + 1;

        tinygltf::Accessor normalAccessor;
        normalAccessor.bufferView = normalBufferViewIndex;
        normalAccessor.byteOffset = 0;
        normalAccessor.componentType = TINYGLTF_COMPONENT_TYPE_FLOAT;
        normalAccessor.count = meshRef.vertexCount;
        normalAccessor.type = TINYGLTF_TYPE_VEC3;
        model.accessors.push_back(normalAccessor);
        const int normalAccessorIndex = shareAttributeAccessors ? 2 : accessorCount * i + 2;

        tinygltf::Accessor tangentAccessor;
        tangentAccessor.bufferView = tangentBufferViewIndex;
        tangentAccessor.byteOffset = 0;
        tangentAccessor.componentType = TINYGLTF_COMPONENT_TYPE_FLOAT;
        tangentAccessor.count = meshRef.vertexCount;
        tangentAccessor.type = TINYGLTF_TYPE_VEC4;
        model.accessors.push_back(tangentAccessor);
        const int tangentAccessorIndex = shareAttributeAccessors ? 3 : accessorCount * i + 3;

        tinygltf::Accessor colorAccessor;
        colorAccessor.bufferView = colorBufferViewIndex;
        colorAccessor.byteOffset = 0;
        colorAccessor.componentType = TINYGLTF_COMPONENT_TYPE_FLOAT;
        colorAccessor.count = meshRef.vertexCount;
        colorAccessor.type = TINYGLTF_TYPE_VEC4;
        model.accessors.push_back(colorAccessor);
        const int colorAccessorIndex = shareAttributeAccessors ? 4 : accessorCount * i + 4;

        tinygltf::Accessor uvAccessor;
        uvAccessor.bufferView = uvBufferViewIndex;
        uvAccessor.byteOffset = 0;
        uvAccessor.componentType = TINYGLTF_COMPONENT_TYPE_FLOAT;
        uvAccessor.count = meshRef.vertexCount;
        uvAccessor.type = TINYGLTF_TYPE_VEC2;
        model.accessors.push_back(uvAccessor);
        const int uvAccessorIndex = shareAttributeAccessors ? 5 : accessorCount * i + 5;

        // Create a normal map texture component for the material
        const int imageIndex = 0;
        const int samplerIndex = 0;
        const int textureIndex = 0;
        if (i == 0) {
            tinygltf::Image image;
            image.uri = s_normalMapFilename;
            model.images.push_back(image);

            tinygltf::Sampler sampler;
            sampler.magFilter = TINYGLTF_TEXTURE_FILTER_LINEAR;
            sampler.minFilter = TINYGLTF_TEXTURE_FILTER_LINEAR_MIPMAP_LINEAR;
            sampler.wrapS = TINYGLTF_TEXTURE_WRAP_REPEAT;
            sampler.wrapT = TINYGLTF_TEXTURE_WRAP_REPEAT;
            model.samplers.push_back(sampler);

            tinygltf::Texture texture;
            texture.source = imageIndex;
            texture.sampler = samplerIndex;
            model.textures.push_back(texture);
        }

        // Create default PBR material
        tinygltf::Material material;
        material.doubleSided = doubleSided;
        material.name = "Material" + std::to_string(i);
        material.normalTexture.index = textureIndex;
        model.materials.push_back(material);
        const int materialIndex = i;

        // Configure primitive
        tinygltf::Primitive primitive;
        primitive.mode = TINYGLTF_MODE_TRIANGLES;
        primitive.indices = indexAccessorIndex;
        primitive.attributes["POSITION"] = positionAccessorIndex;
        primitive.attributes["NORMAL"] = normalAccessorIndex;
        primitive.attributes["TANGENT"] = tangentAccessorIndex;
        primitive.attributes["COLOR_0"] = colorAccessorIndex;
        primitive.attributes["TEXCOORD_0"] = uvAccessorIndex;
        primitive.material = materialIndex;

        tinygltf::Mesh mesh;
        mesh.primitives.push_back(primitive);
        model.meshes.push_back(mesh);
        const int meshIndex = i;

        // Configure the node
        tinygltf::Node node;
        node.name = "Node" + std::to_string(i);
        // Position the i-th mesh in a circle placement
        float theta = 2.0f * 3.14f * i / meshRefs.size();
        node.translation = {1 - 2.f * cosf(theta), 2.f * sinf(theta), 0.0f};
        node.mesh = meshIndex;
        model.nodes.push_back(node);
        const int nodeIndex = i;

        scene.nodes.push_back(nodeIndex);
    }

    // Configure the scene
    model.scenes.push_back(scene);
    model.defaultScene = 0;

    // Serialize the model to GLB format
    tinygltf::TinyGLTF gltfConverter;
    std::string error, warning;
    const bool result = gltfConverter.WriteGltfSceneToFile(&model, outputPath, true, true, true, true);

    if (!result) {
        TRACE("Failed to write GLB file: %s", error.c_str());
        return;
    }

    TRACE("GLB file written: %s", outputPath.c_str());

    const auto imagePath = va::windows::GetLocalAssetPath(s_normalMapFilename);
    const auto outputFolder = std::filesystem::path(outputPath).parent_path() / s_normalMapFilename;
    try {
        std::filesystem::copy_file(imagePath, outputFolder, std::filesystem::copy_options::overwrite_existing);
    } catch (const std::filesystem::filesystem_error& e) {
        TRACE("Failed to copy image file: %s", e.what());
    }
}
