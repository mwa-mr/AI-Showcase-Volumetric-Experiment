import volumetric as va
import math
from datetime import datetime
import numpy as np
from pygltflib import GLTF2
from array import array
import logic.state_mutations as state_mutations
from utils.paths import get_models_path

def run_volumetric_experience(state):

    class _HeadphonesVolume(va.Volume):
        def __init__(self, app: va.VolumetricApp):
            super().__init__(app)

            # Headphones model properties
            self.app_name = "Headphones Configurator"
            self.modelUri = self.app.get_local_asset_uri(f"{get_models_path()}/Headphones.glb")
            self.model: va.ModelResource
            self.visual: va.VisualElement
            self.earcups: list[va.VisualElement] = []
            self.headband_mat: va.MaterialResource
            self.speakers_mat: va.MaterialResource
            self.last_headband_color: list[int] = [255, 255, 255, 255]
            self.last_speakers_color: list[int] = [255, 255, 255, 255]
            self.last_texture_on = 0

            # Accessories properties
            self.wingsUri = self.app.get_local_asset_uri(f"{get_models_path()}/Wings.glb")
            self.wingsMorphUri = f"{get_models_path()}/WingsMorph.glb"
            self.wingsModel: va.ModelResource
            self.wingsVisual: va.VisualElement
            self.earsUri = self.app.get_local_asset_uri(f"{get_models_path()}/Ears.glb")
            self.earsModel: va.ModelResource
            self.earsVisual: va.VisualElement
            self.cromoUri = self.app.get_local_asset_uri(f"{get_models_path()}/Cromo.glb")
            self.cromoModel: va.ModelResource
            self.cromoVisual: va.VisualElement
            self.accessories: list[va.VisualElement] = []
            self.last_active_accessories: list[int] = []

            # Mesh properties
            self.wingsMesh: va.MeshResource
            self.wingsActive = False
            self.meshVertexPositions: list[va.Vector3] = []
            self.meshVertexNormals: list[va.Vector3] = []
            self.meshVertexTangents: list[va.Vector4] = []
            self.morphVertexPositions: list[va.Vector3] = []
            self.morphVertexNormals: list[va.Vector3] = []
            self.morphVertexTangents: list[va.Vector4] = []
            self.vertexCount = 0
            self.blendedVertices: list[float] = []
            self.blendedNormals: list[float] = []
            self.blendedTangents: list[float] = []

            # Adaptive Card template & properties
            self.adaptive_card: va.AdaptiveCardElement
            self.adaptive_card_template: str = """
            {
                "type": "AdaptiveCard",
                "body": [],
                "actions": [
                    {
                        "type": "Action.Execute",
                        "iconUrl": "${icon}",
                        "verb": "shuffle"
                    }
                ],
                "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
                "version": "1.4"
            }
            """

            self.model_instantiated: bool = False

        # Perform actions when volume is ready
        def on_ready(self) -> None:

            # Set Volume display name
            self.container.set_display_name(self.app_name)

            # Set Volume properties and behaviour
            self.content.set_size_behavior(va.VA_VOLUME_SIZE_BEHAVIOR_FIXED)
            self.content.set_size(4, 4, 4)

            # Load headphones GLB model
            self.model = va.ModelResource(self, self.modelUri)
            # Create Visual Element with an associated model
            self.visual = va.VisualElement.create_with_visual_resource(self, self.model)

            # Reference model materials for later editing
            self.headband_mat = va.MaterialResource(self, self.model, "Mat_HeadBand")
            self.speakers_mat = va.MaterialResource(self, self.model, "Mat_speakers")

            # Load headphones accessories (ears, cromo, wings)
            self.wingsModel = va.ModelResource(self, self.wingsUri)
            self.wingsVisual = va.VisualElement.create_with_visual_resource(self, self.wingsModel)
            self.accessories.append(self.wingsVisual)

            self.earsModel = va.ModelResource(self, self.earsUri)
            self.earsVisual = va.VisualElement.create_with_visual_resource(self, self.earsModel)
            self.accessories.append(self.earsVisual)

            self.cromoModel = va.ModelResource(self, self.cromoUri)
            self.cromoVisual = va.VisualElement.create_with_visual_resource(self, self.cromoModel)
            self.accessories.append(self.cromoVisual)

            # Create mesh buffers for mesh editing
            self.get_morph_info(self.wingsMorphUri)
            buffer_descriptors = {
                va.VA_MESH_BUFFER_TYPE_INDEX_EXT: va.VA_MESH_BUFFER_FORMAT_UINT32_EXT,
                va.VA_MESH_BUFFER_TYPE_VERTEX_POSITION_EXT: va.VA_MESH_BUFFER_FORMAT_FLOAT3_EXT,
                va.VA_MESH_BUFFER_TYPE_VERTEX_NORMAL_EXT: va.VA_MESH_BUFFER_FORMAT_FLOAT3_EXT,
                va.VA_MESH_BUFFER_TYPE_VERTEX_TANGENT_EXT: va.VA_MESH_BUFFER_FORMAT_FLOAT4_EXT
            }
            # Create Mesh for later editing
            self.wingsMesh = va.MeshResource.create_with_mesh_index(self, self.wingsModel, 0, 0, buffer_descriptors, decouple_accessors=False, initialize_data=True)

            # Control model sub nodes by creating new Visual Elements
            for i in range(4):
                self.earcups.append(va.VisualElement.create_named_node(self, visual_reference=self.visual, node_name=f"EarCupsv{i + 1}"))
                if i != 0:
                    self.earcups[i].set_visible(False)

            # Hide GLB models
            self.visual.set_visible(False)
            for accessory in self.accessories:
                accessory.set_visible(False)

            # Create Adaptive Card and add listener for actions
            self.adaptive_card = va.AdaptiveCardElement(self, self.on_adaptive_card_action)
            self.adaptive_card.set_template(self.adaptive_card_template)
            self.adaptive_card.set_data(self.format_adaptive_card_data())

            # Configure volume update frequency
            self.request_update(va.VA_VOLUME_UPDATE_MODE_FULL_FRAMERATE)

        # Perform actions when the volume is updated
        def on_update(self) -> None:

            # Perform actions just one time after the model is ready
            if not self.model_instantiated and self.model.is_ready and self.visual.is_ready:
                self.model_instantiated = True
                self.visual.set_visible(True)
                state_mutations.update_deploy_button_state(state_mutations.DeployButtonState.DISABLED.value, None, None, state)

            # Apply changes to the 3D model if state has changed
            self.apply_state_to_3d(state)

            # Edit Mesh Buffers each loop
            self.wings_animation()

        # Perform actions when Volume is closed
        def on_close(self) -> None:
            state_mutations.update_deploy_button_state(state_mutations.DeployButtonState.ENABLED.value, None, None, state)
            self.app.request_exit()
            
        def srgb_to_linear(self, srgb_color):
            linear_components = []
            for index, component in enumerate(srgb_color):
                clamped_component = max(0, min(component, 255))
                srgb_value = clamped_component / 255.0
                # skip alpha channel conversion
                if index == 3:
                    linear_components.append(srgb_value)
                    continue
                if srgb_value <= 0.04045:
                    linear_components.append(srgb_value / 12.92)
                else:
                    linear_components.append(math.pow((srgb_value + 0.055) / 1.055, 2.4))
            return linear_components

        def apply_state_to_3d(self, state):
            headband_color = state.get("Headband", [255, 255, 255, 255])
            if headband_color != self.last_headband_color:
                self.last_headband_color = headband_color
                headband_color_linear = self.srgb_to_linear(headband_color)
                self.headband_mat.set_pbr_base_color_factor(*headband_color_linear)

            speakers_color = state.get("Speakers", [255, 255, 255, 255])
            if speakers_color != self.last_speakers_color:
                self.last_speakers_color = speakers_color
                speakers_color_linear = self.srgb_to_linear(speakers_color)
                self.speakers_mat.set_pbr_base_color_factor(*speakers_color_linear)

            self.texture_on = state.get("Texture_idx", 0)
            if self.texture_on != self.last_texture_on:
                self.last_texture_on = self.texture_on
                for i, earcup in enumerate(self.earcups):
                    earcup.set_visible(False)
                    if i == self.texture_on:
                        earcup.set_visible(True)

            self.active_accessories = state.get("Active_accessories", [])
            if self.active_accessories != self.last_active_accessories:
                self.last_active_accessories = self.active_accessories
                for i, accessory in enumerate(self.accessories):
                    if (i == 0):
                            self.wingsActive = i in self.active_accessories
                    if i in self.active_accessories:
                        accessory.set_visible(True)
                    else:
                        accessory.set_visible(False)

        # Format Adaptive Card
        def format_adaptive_card_data(self) -> str:
            iconUrl = self.app.get_local_asset_uri("../assets/Shuffle.png")
            return f'{{"icon":"{iconUrl}"}}'

        # Respond to Adaptive Card actions
        def on_adaptive_card_action(self, verb: str):
            if verb == "shuffle":
                state_mutations.shuffle(state)

        # Edit Mesh Buffers to animate a mesh between its default position and a morph
        def wings_animation(self):

            if self.wingsModel.is_ready and self.wingsMesh.is_ready and self.wingsActive and self.wings_has_morph:
                # Smooth ping-pong between 0 and 1
                velocity = 5.0
                now = datetime.now()
                seconds = now.hour * 3600 + now.minute * 60 + now.second + now.microsecond / 1e6
                blend = (math.sin(seconds * velocity) * 0.5) + 0.5

                with self.wingsMesh.create_mesh_buffer_writer([
                    va.VA_MESH_BUFFER_TYPE_VERTEX_POSITION_EXT,
                    va.VA_MESH_BUFFER_TYPE_VERTEX_NORMAL_EXT,
                    va.VA_MESH_BUFFER_TYPE_VERTEX_TANGENT_EXT
                ]) as writer:

                    for i in range(0, self.vertexCount):

                        # Positions
                        basePos = self.meshVertexPositions[i]
                        deltaPos = self.morphVertexPositions[i]
                        finalPos = basePos + deltaPos * blend
                        self.blendedVertices[i] = finalPos

                        # Normals
                        baseNorm = self.meshVertexNormals[i]
                        deltaNorm = self.morphVertexNormals[i]
                        finalNorm = baseNorm + deltaNorm * blend
                        self.blendedNormals[i] = finalNorm

                        # Tangents
                        baseTangent = self.meshVertexTangents[i]
                        deltaTangent = self.morphVertexTangents[i]
                        finalTangent = baseTangent.copy()
                        finalTangent[:3] = baseTangent[:3] + deltaTangent * blend
                        self.blendedTangents[i] = finalTangent

                writer.write(va.VA_MESH_BUFFER_TYPE_VERTEX_POSITION_EXT, self.blendedVertices.ravel().tobytes(), 0)
                writer.write(va.VA_MESH_BUFFER_TYPE_VERTEX_NORMAL_EXT, self.blendedNormals.ravel().tobytes(), 0)
                writer.write(va.VA_MESH_BUFFER_TYPE_VERTEX_TANGENT_EXT, self.blendedTangents.ravel().tobytes(), 0)

        # Get morph information of a model to edit Mesh Buffers (pygltflib library dependency)
        def get_morph_info(self, uri):

            def read_glb_buffer(glb_path):
                with open(glb_path, "rb") as f:
                    data = f.read()
                magic = data[:4]
                if magic != b'glTF':
                    raise ValueError("Not a GLB file")
                json_chunk_length = int.from_bytes(data[12:16], 'little')
                bin_header_start = 20 + json_chunk_length
                bin_chunk_length = int.from_bytes(data[bin_header_start:bin_header_start+4], 'little')
                bin_chunk = data[bin_header_start+8:bin_header_start+8+bin_chunk_length]
                return bin_chunk

            def get_accessor_data(gltf, accessor_idx, bin_chunk):
                accessor = gltf.accessors[accessor_idx]
                bufferView = gltf.bufferViews[accessor.bufferView]
                dtype = np.float32
                ncomp = {"VEC3": 3, "VEC4": 4, "SCALAR": 1}.get(accessor.type, 1)
                offset = (bufferView.byteOffset or 0) + (accessor.byteOffset or 0)
                count = accessor.count * ncomp
                arr = np.frombuffer(bin_chunk, dtype=dtype, count=count, offset=offset)
                return arr.reshape((-1, ncomp))

            gltf = GLTF2().load(uri)
            bin_chunk = read_glb_buffer(uri)

            for node in gltf.nodes:
                # Access the mesh of the model
                if node.mesh is not None:
                    mesh = gltf.meshes[node.mesh]
                    for prim in mesh.primitives:

                        # Access vertex data
                        position_id = getattr(prim.attributes, "POSITION", None)
                        if position_id is not None:
                            self.meshVertexPositions = get_accessor_data(gltf, position_id, bin_chunk)

                        normal_id = getattr(prim.attributes, "NORMAL", None)
                        if normal_id is not None:
                            self.meshVertexNormals = get_accessor_data(gltf, normal_id, bin_chunk)

                        tangent_id = getattr(prim.attributes, "TANGENT", None)
                        if tangent_id is not None:
                            self.meshVertexTangents = get_accessor_data(gltf, tangent_id, bin_chunk)

                        # Check if the primitive has any morph targets (blendshapes)
                        if prim.targets:
                            print(f"Mesh '{mesh.name}' has {len(prim.targets)} blendshapes.")

                            for target in prim.targets:
                                # Morph positions
                                morph_pos = target.get("POSITION", None)
                                if morph_pos is not None:
                                    self.morphVertexPositions = get_accessor_data(gltf, morph_pos, bin_chunk)
                                # Morph normals
                                morph_norm = target.get("NORMAL", None)
                                if morph_norm is not None:
                                    self.morphVertexNormals = get_accessor_data(gltf, morph_norm, bin_chunk)
                                # Morph tangents
                                morph_tan = target.get("TANGENT", None)
                                if morph_tan is not None:
                                    self.morphVertexTangents = get_accessor_data(gltf, morph_tan, bin_chunk)

                            # If morph targets are found, prepare buffers for blending
                            self.vertexCount = min(len(self.meshVertexPositions), len(self.morphVertexPositions))

                            self.blendedVertices = np.zeros((self.vertexCount, 3), dtype=np.float32)
                            self.blendedNormals = np.zeros((self.vertexCount, 3), dtype=np.float32)
                            self.blendedTangents = np.zeros((self.vertexCount, 4), dtype=np.float32)

                        else:
                            print("No blendshapes found in model")

        @property
        def wings_has_morph(self):
            return (
                len(self.blendedVertices) > 0 and
                len(self.blendedNormals) > 0 and
                len(self.blendedTangents) > 0
            )

    # Create the Volumetric App instance with required extensions
    app = va.VolumetricApp(
        "Product Configurator", [va.VA_EXT_GLTF2_MODEL_RESOURCE_EXTENSION_NAME,
                                 va.VA_EXT_MATERIAL_RESOURCE_EXTENSION_NAME,
                                 va.VA_EXT_ADAPTIVE_CARD_ELEMENT_EXTENSION_NAME,
                                 va.VA_EXT_MESH_EDIT_EXTENSION_NAME]
    )

    # Register the HeadphonesVolume as the main volume
    app.run(lambda _:  _HeadphonesVolume(app))
