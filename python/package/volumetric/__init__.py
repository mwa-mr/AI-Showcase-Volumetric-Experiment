"""
This is a Python package for Volumetric Api Library.
"""

from .api import *

_prefixes = ("VA_EXT_", "VA_VOLUME_", "VA_MESH_", "VA_FRAME_TIMING_")
_export_types = list(
    filter(lambda x: any(x.startswith(_prefixes)
           for prefix in _prefixes), dir())
)

_export_classes = [
    "AdaptiveCardElement",
    "MaterialResource",
    "MeshBufferWriter",
    "MeshResource",
    "ModelResource",
    "VisualElement",
    "Volume",
    "VolumeContainer",
    "VolumeContent",
    "VolumetricApp",
]

# Export symbols
__all__ = _export_types + _export_classes
