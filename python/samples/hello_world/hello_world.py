'''
This sample demonstrates how to load a GLTF model in a volume.
'''
import sys
import os
import volumetric as va


class _HelloWorld(va.Volume):
    def on_ready(self) -> None:
        script_dir = os.path.dirname(os.path.abspath(__file__))
        _uri = self.app.get_local_asset_uri(os.path.join(script_dir, "../../../assets/world.glb"))
        _model = va.ModelResource(self, _uri)
        _visual = va.VisualElement(self, _model)

    def on_close(self) -> None:
        # This sample has only one volume. Hence, the app exits when the user closes the volume.
        self.app.request_exit()


if __name__ == '__main__':
    app = va.VolumetricApp("Python Hello World", [va.VA_EXT_GLTF2_MODEL_RESOURCE_EXTENSION_NAME])
    app.on_start = lambda _: _HelloWorld(app)
    sys.exit(app.run())
