'''
This sample demonstrates how to use VolumetricApp to display a flock of boids.
'''
import os
import math

import cProfile
from pstats import Stats
from boids_flat import Boids
from typing import Sequence

import volumetric as va

NUM_BOIDS = 50
FRAMERATE = 72


class BoidsVolume(va.Volume):
    '''
    This sample class derives Volume class to display a flock of boids.
    '''
    uri: str = None
    boids: Boids = None
    model: va.ModelResource = None
    boid_visuals: list[va.VisualElement] = []

    def __init__(self, app: va.VolumetricApp, boids, uri: str):
        super().__init__(app)

        self.boids = boids
        self.uri: str = uri
        self.container.set_display_name("Boids Volumetric in Python")
        self.container.set_rotation_lock(va.VA_VOLUME_ROTATION_LOCK_X | va.VA_VOLUME_ROTATION_LOCK_Z)
        self.content.set_size(0.5, 0.5, 0.5)
        self.content.set_size_behavior(va.VA_VOLUME_SIZE_BEHAVIOR_FIXED)

    def on_ready(self) -> 1:
        self.model = va.ModelResource(self, uri=self.uri)
        self.request_update(va.VA_VOLUME_UPDATE_MODE_FULL_FRAMERATE)

        for _ in range(0, self.boids.num_boids):
            visual = va.VisualElement.create_with_visual_resource(self, visual_resource=self.model)
            visual.set_scale(2, 2, 2)
            self.boid_visuals.append(visual)

    def _normalize_quaternion(self, q: Sequence[float]) -> list[float]:
        n = math.sqrt(sum(x * x for x in q))
        return [x / n for x in q] if n > 0 else [0, 0, 0, 1]

    def on_update(self) -> None:
        if not self.boids.running:
            self.request_close()
            return

        self.boids.update()

        if self.boid_visuals is not None:
            for i in range(0, self.boids.num_boids):
                # Need to flip y axis because pygame is using OpenGL convention
                self.boid_visuals[i].set_position(self.boids.boids[i].position.x, -self.boids.boids[i].position.y, self.boids.boids[i].position.z)
                q = self._normalize_quaternion(self.boids.boids[i].orientation)
                self.boid_visuals[i].set_orientation(q[0], -q[1], q[2], q[3])

    def on_close(self) -> None:
        print("VolumetricApp.on_close")
        self.boid_visuals = None
        self.model = None
        # This sample has only one volume. Hence, the app exits when the user closes the volume.
        self.app.request_exit()


def _main():
    app = va.VolumetricApp("Python Boids", [va.VA_EXT_GLTF2_MODEL_RESOURCE_EXTENSION_NAME], wait_for_system_behavior=va.VA_SESSION_WAIT_FOR_SYSTEM_BEHAVIOR_RETRY_SILENTLY)
    app.run(_on_start)
    print("Done.")


def _on_start(app: va.VolumetricApp) -> None:
    print("on_start")
    # TODO: get framerate from vapp APIs
    boids = Boids(NUM_BOIDS, FRAMERATE)
    uri = "file:///" + os.path.abspath(os.path.join(os.path.dirname(__file__), "boid.glb")).replace("\\", "/")
    print(f"uri={uri}")
    BoidsVolume(app, boids, uri)


if __name__ == '__main__':
    do_profiling: bool = False
    if do_profiling:
        with cProfile.Profile() as pr:
            _main()

        with open('profiling_stats.txt', 'w', encoding="utf-8") as stream:
            stats = Stats(pr, stream=stream)
            stats.strip_dirs()
            stats.sort_stats('time')
            stats.dump_stats('.prof_stats')
            stats.print_stats()
    else:
        _main()
