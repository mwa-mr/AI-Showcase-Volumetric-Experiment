'''
This sample demonstrates how to create multiple volumes in a single app.
Each volume loads a different GLTF model and rotates at different speeds.
'''

import math
import time
import os
import volumetric as va


class SpinningModel(va.Volume):
    '''
    This sample class derives Volume class to load a GLTF model and spin it at given frame rate.
    '''

    def __init__(self, app: va.VolumetricApp, update_mode: int, uri: str):
        super().__init__(app)
        self.visual: va.VisualElement = None
        self.model: va.ModelResource = None
        self.angle: int = 0
        self.update_mode: int = update_mode
        self.uri: str = uri

    def on_ready(self) -> None:
        self.model = va.ModelResource(self, uri=self.uri)
        self.visual = va.VisualElement.create_with_visual_resource(self, visual_resource=self.model)
        self.request_update(self.update_mode)

    def on_update(self) -> None:
        seconds = time.time()
        self.angle = seconds * 3.14  # rotate 180 degrees per second
        self.visual.set_orientation(0, math.sin(self.angle / 2.0), 0, math.cos(self.angle / 2.0))
        scale: float = (math.cos(seconds) + 3.0) / 4.0
        self.visual.set_scale(scale, scale, scale)

    def on_close(self) -> None:
        self.visual = None
        self.model = None


def _handle_volume_collection_changed(app: va.VolumetricApp) -> None:
    # This sample has multiple volumes. Hence, the app exits when the user closes all volumes.
    if not app.has_active_volume():
        app.request_exit()


def _handle_app_start(app: va.VolumetricApp) -> None:
    print("on_start")
    script_dir = os.path.dirname(os.path.abspath(__file__))
    uri_1 = app.get_local_asset_uri(os.path.join(script_dir, "../../../assets/BoomBox.glb"))
    uri_2 = app.get_local_asset_uri(os.path.join(script_dir, "../../../assets/Duck.glb"))
    SpinningModel(app, va.VA_VOLUME_UPDATE_MODE_QUARTER_FRAMERATE, uri_1)
    SpinningModel(app, va.VA_VOLUME_UPDATE_MODE_FULL_FRAMERATE, uri_2)


def _main():
    app = va.VolumetricApp("Python Multiple Volumes", [va.VA_EXT_GLTF2_MODEL_RESOURCE_EXTENSION_NAME])
    app.on_start = _handle_app_start
    app.on_volume_collection_changed = _handle_volume_collection_changed
    app.on_fatal_error = lambda app, error: print(f"Fatal error: {error}")
    app.run()
    print("Done.")


if __name__ == '__main__':
    _main()
