'''
This sample demonstrates how to create a spinning cube using the Volumetric SDK.
It also shows hot to edit a submesh using a MeshBufferWriter.
'''
import math
import time
import os
from array import array
import volumetric as va


class SpinningCube(va.Volume):
    '''
    This sample class derives Volume class to display a spinning cube.
    Its top-face spins in opposite direction.
    '''

    def __init__(self, _app: va.VolumetricApp):
        super().__init__(_app)
        self.container.set_display_name("Python Spinning Cube")
        self.content.set_size(2, 2, 2)
        self.content.set_size_behavior(va.VA_VOLUME_SIZE_BEHAVIOR_FIXED)
        self.visual: va.VisualElement = None
        self.model: va.ModelResource = None
        self.mesh: va.MeshResource = None
        self.angle: int = 0

    def on_ready(self) -> None:
        script_dir = os.path.dirname(os.path.abspath(__file__))
        uri = self.app.get_local_asset_uri(os.path.join(script_dir, "../../../assets/BoxTextured.glb"))
        self.model = va.ModelResource(self, uri=uri)
        self.visual = va.VisualElement.create_with_visual_resource(self, visual_resource=self.model)
        self.request_update(va.VA_VOLUME_UPDATE_MODE_FULL_FRAMERATE)

        buffer_descriptors = {
            va.VA_MESH_BUFFER_TYPE_INDEX_EXT: va.VA_MESH_BUFFER_FORMAT_UINT32_EXT,
            va.VA_MESH_BUFFER_TYPE_VERTEX_POSITION_EXT: va.VA_MESH_BUFFER_FORMAT_FLOAT3_EXT,
            va.VA_MESH_BUFFER_TYPE_VERTEX_NORMAL_EXT: va.VA_MESH_BUFFER_FORMAT_FLOAT3_EXT}
        self.mesh = va.MeshResource.create_with_mesh_index(self, self.model, 0, 0, buffer_descriptors, decouple_accessors=False, initialize_data=True)

    def on_update(self) -> None:
        # print("SpinningCube.onVolumeUpdate")
        seconds = time.time()
        self.angle = seconds * 3.14 / 2  # rotate 90 degrees per second
        self.visual.set_orientation(0, math.sin(self.angle / 2.0), 0, math.cos(self.angle / 2.0))
        scale: float = (math.cos(seconds) + 3.0) / 4.0
        self.visual.set_scale(scale, scale, scale)

        if self.mesh.is_ready:
            with self.mesh.create_mesh_buffer_writer([
                va.VA_MESH_BUFFER_TYPE_INDEX_EXT,
                va.VA_MESH_BUFFER_TYPE_VERTEX_POSITION_EXT
            ]) as writer:
                t = -3 * time.time()
                ct = math.cos(t)
                st = math.sin(t)
                # 1m cube, so the radius is 0.5
                r = 0.5

# fmt: off
                # x' = x cos(t) - y sin(t)
                # y' = y cos(t) + x sin(t)
                vertices = array('f', [
                    -r * ct - -r * st, -r * ct + -r * st, r,  # v0 -> (-r, -r)
                     r * ct - -r * st, -r * ct +  r * st, r,  # v1 -> ( r, -r)
                    -r * ct -  r * st,  r * ct + -r * st, r,  # v2 -> (-r,  r)
                     r * ct -  r * st,  r * ct +  r * st, r   # v3 -> ( r,  r)
                ])
# fmt: on
                writer.write(
                    va.VA_MESH_BUFFER_TYPE_VERTEX_POSITION_EXT, vertices, 0)

    def on_close(self) -> None:
        self.model = None
        self.visual = None
        self.mesh = None
        # This sample has only one volume. Hence, the app exits when the user closes the volume.
        self.app.request_exit()


if __name__ == '__main__':
    app = va.VolumetricApp("Python Spinning Cube", [va.VA_EXT_GLTF2_MODEL_RESOURCE_EXTENSION_NAME,
                                                    va.VA_EXT_MESH_EDIT_EXTENSION_NAME])
    app.run(lambda _: SpinningCube(app))
    print("Done.")
