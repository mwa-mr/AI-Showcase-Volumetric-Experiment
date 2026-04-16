'''
Volumetric API for Python
'''

import __main__
from typing import Optional
from collections.abc import Callable as Callable
from pathlib import Path as _Path
from threading import Lock as _Lock, Thread as _Thread
from sys import executable as _executable
import asyncio as _asyncio
import sys as _sys
import os as _os
import inspect as _inspect
import uuid
import warnings as _warnings

# pylint: disable=fixme
# pylint: disable=wildcard-import
# pylint: disable=unused-wildcard-import

from .x64.volumetric_c_module import *
from .api_gen import *

# TODO: scope the following pylint disable to a sperate py file
# pylint: disable=undefined-variable

# pylint: disable=protected-access
# pylint: disable=unnecessary-pass
# pylint: disable=too-many-arguments
# pylint: disable=too-many-positional-arguments
# pylint: disable=too-few-public-methods
# pylint: disable=too-many-instance-attributes

# TODO: type hinting is work in progress

# TODO: autogenerate types from spec.xml
VaSystemId = int
VaVolumeState = int
VaElementType = int
VaVolumeUpdateMode = int
VaElementAsyncStateExt = int
VaVolumeRestoredResultExt = int

# handles
VaElement = int
VaSession = int
VaRuntime = int
VaVolume = int

VA_NULL_HANDLE = 0

VA_EXT_ADAPTIVE_CARD_ELEMENT_EXTENSION_NAME = "VA_EXT_adaptive_card_element"
VA_EXT_GLTF2_MODEL_RESOURCE_EXTENSION_NAME = "VA_EXT_gltf2_model_resource"
VA_EXT_LOCATE_JOINTS_EXTENSION_NAME = "VA_EXT_locate_joints"
VA_EXT_LOCATE_SPACES_EXTENSION_NAME = "VA_EXT_locate_spaces"
VA_EXT_MATERIAL_RESOURCE_EXTENSION_NAME = "VA_EXT_material_resource"
VA_EXT_MESH_EDIT_EXTENSION_NAME = "VA_EXT_mesh_edit"
VA_EXT_TEXTURE_RESOURCE_EXTENSION_NAME = "VA_EXT_texture_resource"
VA_EXT_VOLUME_CONTAINER_MODES_EXTENSION_NAME = "VA_EXT_volume_container_modes"
VA_EXT_VOLUME_CONTENT_CONTAINER_EXTENSION_NAME = "VA_EXT_volume_content_container"
VA_EXT_VOLUME_RESTORE_EXTENSION_NAME = "VA_EXT_volume_restore"
VA_EXT_VOLUME_CONTAINER_THUMBNAIL_EXTENSION_NAME = "VA_EXT_volume_container_thumbnail"


def va_trace_verbose(msg: str) -> None:
    if PyVa_TraceLoggingProviderEnabled():
        PyVa_TraceVerbose(msg)  # ETW event


def va_trace_info(msg: str) -> None:
    if PyVa_TraceLoggingProviderEnabled():
        PyVa_TraceInfo(msg)  # ETW event


def va_trace_error(msg: str) -> None:
    if PyVa_TraceLoggingProviderEnabled():
        PyVa_TraceError(msg)  # ETW event


def va_trace_warning(msg: str) -> None:
    if PyVa_TraceLoggingProviderEnabled():
        PyVa_TraceWarning(msg)  # ETW event

def va_trace_start(msg: str) -> None:
    """
    Start a trace event with the given message.
    This function is used to mark the start of a trace event.
    Should be used in conjunction with `va_trace_stop` to mark the end of the trace event.
    """
    if PyVa_TraceLoggingProviderEnabled():
        PyVa_TraceStart(msg)  # ETW event

def va_trace_stop(msg: str) -> None:
    """
    Stop a trace event with the given message.
    This function is used to mark the end of a trace event.
    Should be used in conjunction with `va_trace_start` to mark the start of the trace event.
    """
    if PyVa_TraceLoggingProviderEnabled():
        PyVa_TraceStop(msg)  # ETW event

class VaScopedTrace:
    """
    A scoped trace for tracing enter and exit of a code block.
    Usage:
    with VaScopedTrace("my_function"):
        # code block
    """
    def __init__(self, msg: str) -> None:
        if PyVa_TraceLoggingProviderEnabled():
            self._msg = msg
        else:
            self._msg = None

    def __enter__(self) -> 'VaScopedTrace':
        if PyVa_TraceLoggingProviderEnabled() and self._msg is not None:
            PyVa_TraceStart(f"ENTER {self._msg}")
        return self

    def __exit__(self, exc_type, exc_value, traceback) -> None:
        if PyVa_TraceLoggingProviderEnabled() and self._msg is not None:
            PyVa_TraceStop(f"EXIT {self._msg}")


class Element:
    '''
    Base class for all classes of elements in a Volume.
    '''

    def __init__(self, volume: 'Volume', element: VaElement, element_type: VaElementType, default_async_state: VaElementAsyncStateExt = VA_ELEMENT_ASYNC_STATE_READY):
        with VaScopedTrace(f"Element() volume={volume.handle} element={element} type={element_type}"):
            self.__handle: VaElement = element
            self._volume: Volume = volume
            self._type: VaElementType = element_type
            self._async_state: VaElementAsyncStateExt = default_async_state
            self.on_async_state_changed: Optional[Callable[['Element', VaElementAsyncStateExt], None]] = None
            self._volume._internal_add_element(self)

    @property
    def handle(self) -> VaElement:
        '''
        Get the unique int64 handle of the element.
        '''
        return self.__handle

    def destroy(self) -> None:
        '''
        Destroy the element and disconnect from parents and children.
        '''
        with VaScopedTrace(f"Element.destroy() {self.handle}"):
            PyVa_DestroyElement(self.handle)

    def _internal_update_async_state(self) -> None:
        new_state = PyVa_GetElementPropertyEnum(self.handle, VA_ELEMENT_PROPERTY_ASYNC_STATE)
        # TODO: Disable this check for now for blender plugin and discuss resolution.
        # Model resource begins with READY state; when set_uri, platform never get a chance to set it back to PENDING,
        # when next platform frame arrives, platform had already loaded the model and send back a READY state.
        # which this check guard, the callbacks will never fire.
        # if new_state != self._async_state:
        va_trace_info(f"Element._internal_update_async_state() {self.handle} state changed from {self._async_state} to {new_state}")
        self._async_state = new_state
        if self.on_async_state_changed is not None:
            self.on_async_state_changed(self, self._async_state)

    @property
    def is_ready(self) -> bool:
        '''
        Check if the element is ready to be used.
        '''
        return self._async_state == VA_ELEMENT_ASYNC_STATE_READY

    @property
    def is_pending(self) -> bool:
        '''
        Check if the element has pending async operations.
        '''
        return self._async_state == VA_ELEMENT_ASYNC_STATE_PENDING

    @property
    def has_error(self) -> bool:
        '''
        Check if the element has any aysnc error.
        '''
        return self._async_state == VA_ELEMENT_ASYNC_STATE_ERROR


class VisualElement(Element):
    '''
    A visual element represents an element that can be rendered in the volumetric space.
    Application can control the visibility, position, orientation, and scale of the visual element.
    A visual element itself does not have any visual content by default.
    Application can set a visual resource to the visual element to render, for example a 3D model.

    A VisualElement can be created in one of the following three ways:
    1. If the user provides a visual resource, they cannot provide anything else.
    2. If the user provides a visual reference, they must provide a named node.
    3. If the user provides a visual parent, they cannot provide anything else.
    '''

    def __init__(self, volume: 'Volume',
                 visual_resource: Optional['ModelResource'] = None,
                 visual_reference: Optional['VisualElement'] = None,
                 node_name: Optional[str] = None,
                 visual_parent: Optional['VisualElement'] = None
                 ):
        with VaScopedTrace("VisualElement()"):
            if visual_resource is not None and (visual_reference is not None or node_name is not None or visual_parent is not None):
                raise ValueError("Cannot provide any other parameters with visual_resource.")
            elif visual_parent is not None and (visual_resource is not None or visual_reference is not None or node_name is not None):
                raise ValueError("Cannot provide any other parameters with visual_parent.")
            elif visual_reference is not None and node_name is None:
                raise ValueError("Cannot provide visual_reference without node_name.")
            elif node_name is not None and visual_reference is None:
                raise ValueError("Cannot provide node_name without visual_reference.")

            element_type = VA_ELEMENT_TYPE_VISUAL
            element = PyVa_CreateElement(volume.handle, element_type)
            va_trace_info(f"PyVa_CreateElement element={element}")
            super().__init__(volume, element, element_type)

            if node_name is not None:
                self.set_node_name(node_name)
            if visual_reference is not None:
                self.set_visual_reference(visual_reference)
            if visual_resource is not None:
                self.set_visual_resource(visual_resource)
            if visual_parent is not None:
                self.set_visual_parent(visual_parent)

    @classmethod
    def create_with_visual_resource(cls, volume: 'Volume', visual_resource: 'ModelResource') -> 'VisualElement':
        '''
        Create a visual element for the given visual resource.
        This is a convenience method to create a visual element with a visual resource.
        '''
        with VaScopedTrace(f"VisualElement.create_for_resource() volume={volume.handle} visual_resource={visual_resource.handle}"):
            return cls(volume, visual_resource=visual_resource)

    @classmethod
    def create_named_node(cls, volume: 'Volume', visual_reference: 'VisualElement', node_name: str) -> 'VisualElement':
        '''
        Create a visual element for the given visual reference and node name.
        This is a convenience method to create a visual element with a visual reference and node name.
        '''
        with VaScopedTrace(f"VisualElement.create_named_node() volume={volume.handle} visual_reference={visual_reference.handle} node_name={node_name}"):
            return cls(volume, visual_reference=visual_reference, node_name=node_name)

    @classmethod
    def create_with_parent(cls, volume: 'Volume', visual_parent: 'VisualElement') -> 'VisualElement':
        '''
        Create a visual element with the given visual parent.
        This is a convenience method to create a visual element with a visual parent.
        '''
        with VaScopedTrace(f"VisualElement.create_with_parent() volume={volume.handle} visual_parent={visual_parent.handle}"):
            return cls(volume, visual_parent=visual_parent)

    def set_visible(self, value: bool) -> None:
        '''
        Set the visibility of the visual element.
        '''
        PyVa_SetElementPropertyBool(self.handle, VA_ELEMENT_PROPERTY_VISIBLE, value)

    def set_orientation(self, x: float, y: float, z: float, w: float) -> None:
        '''
        Set the orientation of the visual element in the volumetric space.
        '''
        PyVa_SetElementPropertyQuaternionf(self.handle, VA_ELEMENT_PROPERTY_ORIENTATION, x, y, z, w)

    def set_position(self, x: float, y: float, z: float) -> None:
        '''
        Set the position of the visual element in the volumetric space.
        '''
        PyVa_SetElementPropertyVector3f(self.handle, VA_ELEMENT_PROPERTY_POSITION, x, y, z)

    def set_scale(self, x: float, y: float, z: float) -> None:
        '''
        Set the scale of the visual element in the volumetric space.
        '''
        PyVa_SetElementPropertyVector3f(self.handle, VA_ELEMENT_PROPERTY_SCALE, x, y, z)

    def set_visual_resource(self, model_resource: 'ModelResource') -> None:
        '''
        Set the resource element of the visual element.
        '''
        PyVa_SetElementPropertyHandle(self.handle, VA_ELEMENT_PROPERTY_VISUAL_RESOURCE, model_resource.handle)
        self._async_state = VA_ELEMENT_ASYNC_STATE_PENDING

    def set_visual_reference(self, visual_reference: 'VisualElement') -> None:
        '''
        Set the visual reference property of the visual element.
        '''
        PyVa_SetElementPropertyHandle(self.handle, VA_ELEMENT_PROPERTY_VISUAL_REFERENCE, visual_reference.handle)
        self._async_state = VA_ELEMENT_ASYNC_STATE_PENDING

    def set_visual_parent(self, visual_parent: 'VisualElement') -> None:
        '''
        Set the visual parent property of the visual element.
        This property is used to attach the visual element to a parent visual element.
        '''
        PyVa_SetElementPropertyHandle(self.handle, VA_ELEMENT_PROPERTY_VISUAL_PARENT, visual_parent.handle)
        self._async_state = VA_ELEMENT_ASYNC_STATE_PENDING

    def set_node_name(self, name: str) -> None:
        '''
        Set the node name property of the visual element.
        This property is only effective when used together with a valid visual_reference property.
        '''
        PyVa_SetElementPropertyString(self.handle, VA_ELEMENT_PROPERTY_GLTF2_NODE_NAME_EXT, name)
        self._async_state = VA_ELEMENT_ASYNC_STATE_PENDING


class ModelResource(Element):
    '''
    A model resource represents a 3D model that can be rendered in the volumetric space.
    Application can set the URI of the model to load a GLTF file.
    The loading of a model is an async operation, that can take several frames to complete.
    During the operation, is_pending property is true.
    When loading is complete successfully, is_ready property becomes true.
    If loading fails, has_error property becomes true.
    '''

    def __init__(self, volume: 'Volume', uri: str = None):
        with VaScopedTrace(f"ModelResource() volume={volume.handle} uri={uri}"):
            element_type = VA_ELEMENT_TYPE_MODEL_RESOURCE
            element = PyVa_CreateElement(volume.handle, element_type)
            va_trace_info(f"PyVa_CreateElement element={element}")
            super().__init__(volume, element, element_type)
            if uri is not None:
                self.set_model_uri(uri)

    def set_model_uri(self, uri: str) -> None:
        '''
        Sets the URI of a glTF 2.0 file and asynchronously loads the file into this model resource.
        Use the Element.is_ready() to check when the model has finished loading.
        '''
        PyVa_SetElementPropertyString(self.handle, VA_ELEMENT_PROPERTY_GLTF2_MODEL_URI_EXT, uri)
        self._async_state = VA_ELEMENT_ASYNC_STATE_PENDING
        

class MaterialResource(Element):
    '''
    A material resource represents a material that is used to associated model resource.
    It can be used to reference a named material in a GLTF file that's loaded in a model resource.
    '''

    def __init__(self, volume: 'Volume', model_resource: ModelResource, material_name: str):
        with VaScopedTrace(f"MaterialResource() volume={volume.handle} model_resource={model_resource.handle} material_name={material_name}"):
            element_type = VA_ELEMENT_TYPE_MATERIAL_RESOURCE_EXT
            element = PyVa_CreateElement(volume.handle, element_type)
            va_trace_info(f"PyVa_CreateElement element={element}")
            super().__init__(volume, element, element_type)
            if model_resource is not None:
                self.set_model_reference(model_resource)
            if material_name is not None:
                self.set_material_name(material_name)

    def set_model_reference(self, model_reference: 'ModelResource') -> None:
        '''
        Set the model resource of the material resource.
        '''
        PyVa_SetElementPropertyHandle(self.handle, VA_ELEMENT_PROPERTY_MODEL_REFERENCE, model_reference.handle)

    def set_material_name(self, name: str) -> None:
        '''
        Set the material name of the material resource.
        '''
        PyVa_SetElementPropertyString(self.handle, VA_ELEMENT_PROPERTY_GLTF2_MATERIAL_NAME_EXT, name)

    def set_pbr_base_color_factor(self, r: float, g: float, b: float, a: float) -> None:
        '''
        Set the base color factor of the PBR material. The rgb component should be in linear color space when used as color.
        '''
        PyVa_SetElementPropertyColor4f(self.handle, VA_ELEMENT_PROPERTY_MATERIAL_PBR_BASE_COLOR_FACTOR_EXT, r, g, b, a)

    def set_pbr_roughness_factor(self, value: float) -> None:
        '''
        Set the roughness factor of the PBR material.
        '''
        PyVa_SetElementPropertyFloat(self.handle, VA_ELEMENT_PROPERTY_MATERIAL_PBR_ROUGHNESS_FACTOR_EXT, value)

    def set_pbr_metallic_factor(self, value: float) -> None:
        '''
        Set the metallic factor of the PBR material.
        '''
        PyVa_SetElementPropertyFloat(self.handle, VA_ELEMENT_PROPERTY_MATERIAL_PBR_METALLIC_FACTOR_EXT, value)

    def set_pbr_base_color_texture(self, texture_resource: 'TextureResource') -> None:
        '''
        Set the base color texture of the PBR material.
        '''
        PyVa_SetElementPropertyHandle(self.handle, VA_ELEMENT_PROPERTY_MATERIAL_PBR_BASE_COLOR_TEXTURE_EXT, texture_resource.handle)

    def set_pbr_metallic_roughness_texture(self, texture_resource: 'TextureResource') -> None:
        '''
        Set the metallic roughness texture of the PBR material.
        '''
        PyVa_SetElementPropertyHandle(self.handle, VA_ELEMENT_PROPERTY_MATERIAL_PBR_METALLIC_ROUGHNESS_TEXTURE_EXT, texture_resource.handle)

    def set_normal_texture(self, texture_resource: 'TextureResource') -> None:
        '''
        Set the normal texture of the PBR material.
        '''
        PyVa_SetElementPropertyHandle(self.handle, VA_ELEMENT_PROPERTY_MATERIAL_NORMAL_TEXTURE_EXT, texture_resource.handle)

    def set_occlusion_texture(self, texture_resource: 'TextureResource') -> None:
        '''
        Set the occlusion texture of the PBR material.
        '''
        PyVa_SetElementPropertyHandle(self.handle, VA_ELEMENT_PROPERTY_MATERIAL_OCCLUSION_TEXTURE_EXT, texture_resource.handle)

    def set_emissive_texture(self, texture_resource: 'TextureResource') -> None:
        '''
        Set the emissive texture of the PBR material.
        '''
        PyVa_SetElementPropertyHandle(self.handle, VA_ELEMENT_PROPERTY_MATERIAL_EMISSIVE_TEXTURE_EXT, texture_resource.handle)


class TextureResource(Element):
    '''
    An image resource represents an image that is used to associated model resource.
    It can be used to reference an image in a GLTF file that's loaded in a model resource.
    '''

    def __init__(self, volume: 'Volume', model_resource: ModelResource, image_index: int):
        with VaScopedTrace(f"TextureResource() volume={volume.handle} model_resource={model_resource.handle} image_index={image_index}"):
            element_type = VA_ELEMENT_TYPE_TEXTURE_RESOURCE_EXT
            element = PyVa_CreateElement(volume.handle, element_type)
            va_trace_info(f"TextureResource() PyVa_CreateElement element={element}")
            super().__init__(volume, element, element_type)

    def set_image_uri(self, uri: str) -> None:
        '''
        Set the URI of the image to load.
        '''
        PyVa_SetElementPropertyString(self.handle, VA_ELEMENT_PROPERTY_TEXTURE_IMAGE_URI_EXT, uri)
        self._async_state = VA_ELEMENT_ASYNC_STATE_PENDING

    def set_normal_scale(self, value: float) -> None:
        '''
        Set the normal scale of the texture.  It's ignored if the texture is not used as normal map.
        '''
        PyVa_SetElementPropertyFloat(self.handle, VA_ELEMENT_PROPERTY_TEXTURE_NORMAL_SCALE_EXT, value)

    def set_occlusion_strength(self, value: float) -> None:
        '''
        Set the occlusion strength of the texture.  It's ignored if the texture is not used as occlusion map.
        '''
        PyVa_SetElementPropertyFloat(self.handle, VA_ELEMENT_PROPERTY_TEXTURE_OCCLUSION_STRENGTH_EXT, value)


class MeshBufferWriter:
    '''
    This context manager class manages the mesh buffer access for writing.
    The enter and exit of the context manager is used to acquire and release the mesh buffer.
    Use `with create_mesh_buffer_writer` to create a mesh buffer writer.
    '''

    def __init__(self, mesh_resource: 'MeshResource', buffer_types: list[int], index_count: int = 0, vertex_count: int = 0):
        with VaScopedTrace(f"MeshBufferWriter() mesh_resource={mesh_resource.handle} buffer_types={buffer_types} index_count={index_count} vertex_count={vertex_count}"):
            self._mesh_resource = mesh_resource
            self._buffer_types = buffer_types
            # Here, the buffer_types is list of buffer descriptors:
            # [
            #     {
            #         "bufferFormat": int,
            #         "bufferByteSize": int,
            #         "elementCount": int,
            #         "meshBufferData": opaque pointer to capsule
            #     }
            # ]
            self._runtime_buffer_data = None
            self._index_count = index_count
            self._vertex_count = vertex_count
            self._acquired = False

    def __enter__(self) -> 'MeshBufferWriter':
        with VaScopedTrace(f"MeshBufferWriter.__enter__() mesh_resource={self._mesh_resource.handle}"):
            if not self._mesh_resource.is_ready:
                return self  # cannot acquire mesh before it is ready
            self._runtime_buffer_data = PyVa_AcquireMeshBufferExt(
                self._mesh_resource.handle, self._buffer_types, index_count=self._index_count, vertex_count=self._vertex_count)
            self._acquired = True
            return self

    def __exit__(self, *args) -> None:
        with VaScopedTrace(f"MeshBufferWriter.__exit__() mesh_resource={self._mesh_resource.handle}"):
            if self._acquired:
                PyVa_ReleaseMeshBufferExt(self._mesh_resource.handle)

    def write(self, buffer_types: int, data: bytes, offset: int) -> None:
        '''
        Write the data to the mesh buffer at the given offset.
        '''
        if not self._acquired:
            va_trace_warning("MeshBufferWriter.write() Mesh buffer not acquired. Call write() within the 'with' block after acquiring the mesh buffer.")
            return

        if len(data) + offset > self._runtime_buffer_data[buffer_types]['bufferByteSize']:
            raise ValueError(f"Offset not within bounds [0, {self._runtime_buffer_data[buffer_types]['bufferByteSize'] - 1}]")
        PyVa_WriteMeshBufferBytes(self._runtime_buffer_data[buffer_types]['meshBufferData'], data, offset)


class MeshResource(Element):
    '''
    A mesh resource element reference to a mesh in a model resource.
    It can be used to access a mesh primitive node in a GLTF model.
    It can be created with mesh index and primitive index or with node name and primitive index.
    '''
    class _ElementCreateInfo:
        def __init__(self, node_name: str, mesh_index: int, primitive_index: int, decouple_accessors: bool = False, initialize_data: bool = False):
            with VaScopedTrace(f"MeshResource._ElementCreateInfo() node_name={node_name} mesh_index={mesh_index} primitive_index={primitive_index} decouple_accessors={decouple_accessors} initialize_data={initialize_data}"):
                self.node_name = node_name
                self.mesh_index = mesh_index
                self.primitive_index = primitive_index
                self.decouple_accessors = decouple_accessors
                self.initialize_data = initialize_data

    @classmethod
    def create_with_mesh_index(
        cls, volume: 'Volume',
        model_resource: ModelResource,
        mesh_index: int,
        primitive_index: int,
        buffer_descriptors: dict,
        decouple_accessors: bool = False,
        initialize_data: bool = False,
    ) -> 'MeshResource':
        """
        Creates a mesh element that reference the given mesh index and mesh primitive index in a GLTF model.
        """
        with VaScopedTrace(f"MeshResource.create_with_mesh_index() volume={volume.handle} model_resource={model_resource.handle} mesh_index={mesh_index} primitive_index={primitive_index} buffer_descriptors={buffer_descriptors} decouple_accessors={decouple_accessors} initialize_data={initialize_data}"):
            params = cls._ElementCreateInfo("", mesh_index, primitive_index, decouple_accessors, initialize_data)
            return MeshResource(volume, model_resource, params, buffer_descriptors)

    @classmethod
    def create_with_node_name(
        cls, volume: 'Volume',
        model_resource: ModelResource,
        node_name: str,
        primitive_index: int,
        buffer_descriptors: dict,
        decouple_accessors: bool = False,
        initialize_data: bool = False,
    ) -> 'MeshResource':
        """
        Creates a mesh element that reference the given node name and mesh primitive index in a GLTF model.
        """
        with VaScopedTrace(f"MeshResource.create_with_node_name() volume={volume.handle} model_resource={model_resource.handle} node_name={node_name} primitive_index={primitive_index} buffer_descriptors={buffer_descriptors} decouple_accessors={decouple_accessors} initialize_data={initialize_data}"):
            params = cls._ElementCreateInfo(node_name, 0, primitive_index, decouple_accessors, initialize_data)
            return MeshResource(volume, model_resource, params, buffer_descriptors)

    def __init__(self, volume: 'Volume', model_resource: ModelResource, parameters: _ElementCreateInfo, buffer_descriptors: dict) -> None:
        """
        Internal constructor. Do not use. Use create_with_mesh_index or create_with_node_name instead.
        """
        with VaScopedTrace(
            f"MeshResource() "
            f"volume={volume.handle} "
            f"model_resource={model_resource.handle} "
            f"node_name={parameters.node_name} "
            f"mesh_index={parameters.mesh_index} "
            f"primitive_index={parameters.primitive_index} "
            f"buffer_descriptors={buffer_descriptors} "
            f"decouple_accessors={parameters.decouple_accessors}"
        ):

            self.buffer_descriptors = buffer_descriptors
            element = PyVa_CreateMeshResource(volume.handle,
                                            model_resource.handle,
                                            parameters.node_name,
                                            parameters.mesh_index,
                                            parameters.primitive_index,
                                            parameters.decouple_accessors,
                                            parameters.initialize_data,
                                            buffer_descriptors)

            va_trace_info(f"PyVa_CreateMeshResource element={element}")
            super().__init__(volume, element, VA_ELEMENT_TYPE_MESH_RESOURCE_EXT, VA_ELEMENT_ASYNC_STATE_PENDING)

    def create_mesh_buffer_writer(self, buffer_types: list[int], index_count: int = 0, vertex_count: int = 0) -> MeshBufferWriter:
        '''
        Create a mesh buffer writer to write data to the mesh buffer.
        Use this method with "with" statement to ensure the mesh buffer is released after writing.
        '''
        with VaScopedTrace(f"MeshResource.create_mesh_buffer_writer() mesh_resource={self.handle} buffer_types={buffer_types} index_count={index_count} vertex_count={vertex_count}"):
            if self._volume.app._internal_enabled_extensions.count(VA_EXT_MESH_EDIT_EXTENSION_NAME) == 0:
                raise RuntimeError("The VA_EXT_MESH_EDIT_EXTENSION_NAME extension is not enabled.")
            return MeshBufferWriter(self, buffer_types, index_count, vertex_count)


class AdaptiveCardElement(Element):
    '''
    An adaptive card element represents an element that can render an adaptive card in the volumetric space.
    Application can set the template and data of the adaptive card to render.
    '''

    def __init__(self, volume: 'Volume', on_action) -> None:
        with VaScopedTrace(f"AdaptiveCardElement() volume={volume.handle}"):
            self.handle_adaptive_card_complete: Optional[Callable[['AdaptiveCardElement'], None]] = None
            element_type = VA_ELEMENT_TYPE_ADAPTIVE_CARD_EXT
            element = PyVa_CreateElement(volume.handle, element_type)
            va_trace_info(f"PyVa_CreateElement element={element}")
            self.on_action = on_action
            super().__init__(volume, element, element_type)

    def set_template(self, json_template: str) -> None:
        '''
        Set the template of the adaptive card.
        '''
        PyVa_SetElementPropertyString(self.handle, VA_ELEMENT_PROPERTY_ADAPTIVE_CARD_TEMPLATE_EXT, json_template)

    def set_data(self, json_data: str) -> None:
        '''
        Set the data of the adaptive card.
        '''
        PyVa_SetElementPropertyString(self.handle, VA_ELEMENT_PROPERTY_ADAPTIVE_CARD_DATA_EXT, json_data)

    def _internal_poll_adaptive_card_action_invoke_data(self) -> None:
        with VaScopedTrace(f"AdaptiveCardElement._internal_poll_adaptive_card_action_invoke_data() {self.handle}"):
            verb, data = PyVa_GetNextAdaptiveCardActionInvokedDataExt(self.handle)
            va_trace_info(f"verb={verb} data={data}")
            if self.on_action is not None:
                self.on_action(verb)


class VolumeContent(Element):
    '''
    A VolumeContent represents the content region of the Volume.
    Its properties defines the behavior of the volume content and its relationship with the volume container.
    There can be only one VolumeContent in a Volume.
    '''

    def __init__(self, volume: 'Volume'):
        with VaScopedTrace(f"VolumeContent() volume={volume.handle}"):
            element_type = VA_ELEMENT_TYPE_VOLUME_CONTENT
            element = PyVa_CreateElement(volume.handle, element_type)
            va_trace_info(f"PyVa_CreateElement element={element}")
            super().__init__(volume, element, element_type)

    def set_position(self, x: float, y: float, z: float) -> None:
        '''
        Set the position of the volume content in the volume space.
        '''
        PyVa_SetElementPropertyVector3f(self.handle, VA_ELEMENT_PROPERTY_VOLUME_CONTENT_POSITION, x, y, z)

    def set_orientation(self, x: float, y: float, z: float, w: float) -> None:
        '''
        Set the orientation of the volume content in the volume space.
        '''
        PyVa_SetElementPropertyQuaternionf(self.handle, VA_ELEMENT_PROPERTY_VOLUME_CONTENT_ORIENTATION, x, y, z, w)

    def set_size(self, width: float, height: float, depth: float) -> None:
        '''
        Set the size of the volume content in the volume space.
        The size property is ignored if the size behavior is set to auto size.
        '''
        PyVa_SetElementPropertyExtent3Df(self.handle, VA_ELEMENT_PROPERTY_VOLUME_CONTENT_SIZE, width, height, depth)

    def set_size_behavior(self, size_behavior: int) -> None:
        '''
        Set the size behavior of the volume content relative to the volume container.
        '''
        PyVa_SetElementPropertyEnum(self.handle, VA_ELEMENT_PROPERTY_VOLUME_CONTENT_SIZE_BEHAVIOR, size_behavior)

    @property
    def actual_scale(self) -> float:
        '''
        Get the actual scale of the volume content in the volume container.
        '''
        return PyVa_GetElementPropertyFloat(self.handle, VA_ELEMENT_PROPERTY_VOLUME_CONTENT_ACTUAL_SCALE)


class _EventProperty:
    '''
    An internal descriptor class for event properties.
    This class is used to define the event properties of the element.
    It is write-only and cannot be accessed from API directly.
    '''

    def __init__(self, name: str, param_type: type):
        self.private_name = f"__{name}"
        self.__public_name = name
        self.param_type = param_type

    def __set_name__(self, owner, name):
        self.__public_name = name

    def __get__(self, instance, owner):
        raise AttributeError(f"{self.__public_name} is write-only")

    def __set__(self, instance, callback: Optional[Callable[[any], None]]):
        if callback is not None and not callable(callback):
            raise ValueError("Callback must be callable or None")
        setattr(instance, self.private_name, callback)


class VolumeContainer(Element):
    '''
    A VolumeContainer represents a container element that can contain other elements.
    Its properties defines the behavior of the volume container in mixed reality experience.
    There can be only one VolumeContainer in a Volume.
    '''

    def __init__(self, volume: 'Volume') -> None:
        with VaScopedTrace(f"VolumeContainer() volume={volume.handle}"):
            element_type = VA_ELEMENT_TYPE_VOLUME_CONTAINER
            element = PyVa_CreateElement(volume.handle, element_type)
            va_trace_info(f"PyVa_CreateElement element={element}")
            super().__init__(volume, element, element_type)
            self.__allowed_modes: int = VA_VOLUME_CONTAINER_MODE_DEFAULT_ALLOWED_EXT
            self.__current_modes: int = VA_VOLUME_CONTAINER_MODE_NONE_EXT
            self.__on_interactive_mode_changed: Optional[Callable[[bool], None]] = None
            self.__on_one_to_one_mode_changed: Optional[Callable[[bool], None]] = None
            self.__on_sharing_in_teams_changed: Optional[Callable[[bool], None]] = None
            self.__on_unbounded_mode_changed: Optional[Callable[[bool], None]] = None
            self.__on_subpart_mode_changed: Optional[Callable[[bool], None]] = None
            self.__display_name: str = None
            self.__fallback_display_name: str = self.__get_fallback_display_name()
            self.__set_display_name()

    def set_display_name(self, display_name: str) -> None:
        '''
        Set the display name of the volume container.
        '''
        self.__display_name = display_name
        self.__set_display_name()

    def __set_display_name(self) -> None:
        display_name = self.__display_name if self.__display_name is not None else self.__fallback_display_name
        PyVa_SetElementPropertyString(self.handle, VA_ELEMENT_PROPERTY_VOLUME_CONTAINER_DISPLAY_NAME, display_name)

    def __get_fallback_display_name(self) -> str:
        file_path = __main__.__file__ if hasattr(__main__, '__file__') else None
        if file_path is not None:
            return f"{_os.path.basename(file_path)}"
        return None

    def set_rotation_lock(self, rotation_lock: int) -> None:
        '''
        Set the rotation lock behavior when user rotates the volume container.
        '''
        PyVa_SetElementPropertyFlags(self.handle, VA_ELEMENT_PROPERTY_VOLUME_CONTAINER_ROTATION_LOCK, rotation_lock)

    def set_thumbnail_model_uri(self, thumbnail_model_uri: str) -> None:
        '''
        Set the URI for the thumbnail model of the volume container.
        The thumbnail model is used to represent the volume in system UIs, such as the volume summary view.
        It should be a URI to a Gltf2 model that represents the volume.
        The model should be small and optimized for quick loading.
        If this property is not set, set to null or set to an empty string,
        the platform will take a snapshot of the volume content when needed and used it as default thumbnail model.
        '''
        PyVa_SetElementPropertyString(self.handle, VA_ELEMENT_PROPERTY_VOLUME_CONTAINER_THUMBNAIL_MODEL_URI_EXT, thumbnail_model_uri)

    def set_thumbnail_icon_uri(self, thumbnail_icon_uri: str) -> None:
        '''
        Set the URI for the thumbnail icon of the volume container.
        The thumbnail icon is used to represent the volume in system UIs together with the display name.
        The thumbnail icon must be a square PNG image with size between 32x32 pixels to 256x256 pixels.
        By providing a 256px icon, it ensures the system only ever scales your icon down, never up.
        The image may have transparent pixels.
        If this property is not set, set to null, set to an empty string or failed to load as PNG file,
        the platform will present the volume with a generic thumbnail icon.
        '''
        PyVa_SetElementPropertyString(self.handle, VA_ELEMENT_PROPERTY_VOLUME_CONTAINER_THUMBNAIL_ICON_URI_EXT, thumbnail_icon_uri)

    def allow_interactive_mode(self, allow: bool) -> None:
        '''
        Set whether the volume is allowed to be in the interactive mode.
        By default, it is disallowed and the user cannot switch to the interactive mode.
        The hand inputs will only be delivered to the app when the user enters the interactive mode.
        To properly handle hand inputs, the app must first allow this mode.
        '''
        self.__allowed_modes = self.__allowed_modes | VA_VOLUME_CONTAINER_MODE_INTERACTIVE_MODE_EXT if allow \
            else self.__allowed_modes & ~VA_VOLUME_CONTAINER_MODE_INTERACTIVE_MODE_EXT
        PyVa_SetElementPropertyFlags(self.handle, VA_ELEMENT_PROPERTY_VOLUME_CONTAINER_MODE_CAPABILITIES_EXT, self.__allowed_modes)

    def allow_one_to_one_mode(self, allow: bool) -> None:
        '''
        Set whether the volume is allowed to be in the one-to-one mode.
        By default, it is allowed and the user can switch to the one-to-one mode.
        If disallowed, the user cannot switch this volume to the one-to-one mode.
        '''
        self.__allowed_modes = self.__allowed_modes | VA_VOLUME_CONTAINER_MODE_ONE_TO_ONE_MODE_EXT if allow \
            else self.__allowed_modes & ~VA_VOLUME_CONTAINER_MODE_ONE_TO_ONE_MODE_EXT
        PyVa_SetElementPropertyFlags(self.handle, VA_ELEMENT_PROPERTY_VOLUME_CONTAINER_MODE_CAPABILITIES_EXT, self.__allowed_modes)

    def allow_sharing_in_teams(self, allow: bool) -> None:
        '''
        Set whether the volume is allowed to be shared in teams.
        By default, it is allowed and the user can share this volume in teams.
        If disallowed, the user cannot share this volume in teams.
        '''
        self.__allowed_modes = self.__allowed_modes | VA_VOLUME_CONTAINER_MODE_SHAREABLE_IN_TEAMS_EXT if allow \
            else self.__allowed_modes & ~VA_VOLUME_CONTAINER_MODE_SHAREABLE_IN_TEAMS_EXT
        PyVa_SetElementPropertyFlags(self.handle, VA_ELEMENT_PROPERTY_VOLUME_CONTAINER_MODE_CAPABILITIES_EXT, self.__allowed_modes)

    def allow_unbounded_mode(self, allow: bool) -> None:
        '''
        Set whether the volume is allowed to be in the unbounded mode.
        By default, it is allowed and the user can switch to the unbounded mode.
        If disallowed, the user cannot switch this volume to the unbounded mode.
        '''
        self.__allowed_modes = self.__allowed_modes | VA_VOLUME_CONTAINER_MODE_UNBOUNDED_MODE_EXT if allow \
            else self.__allowed_modes & ~VA_VOLUME_CONTAINER_MODE_UNBOUNDED_MODE_EXT
        PyVa_SetElementPropertyFlags(self.handle, VA_ELEMENT_PROPERTY_VOLUME_CONTAINER_MODE_CAPABILITIES_EXT, self.__allowed_modes)

    def allow_subpart_mode(self, allow: bool) -> None:
        '''
        Set whether the volume is allowed to be in the subpart mode.
        By default, it is allowed and the user can switch to the subpart mode.
        If disallowed, the user cannot switch this volume to the subpart mode.
        '''
        self.__allowed_modes = self.__allowed_modes | VA_VOLUME_CONTAINER_MODE_SUBPART_MODE_EXT if allow \
            else self.__allowed_modes & ~VA_VOLUME_CONTAINER_MODE_SUBPART_MODE_EXT
        PyVa_SetElementPropertyFlags(self.handle, VA_ELEMENT_PROPERTY_VOLUME_CONTAINER_MODE_SUBPART_MODE_EXT, self.__allowed_modes)

    # generated write-only properties for event callbacks
    on_interactive_mode_changed = _EventProperty("on_interactive_mode_changed", bool)
    on_one_to_one_mode_changed = _EventProperty("on_one_to_one_mode_changed", bool)
    on_sharing_in_teams_changed = _EventProperty("on_sharing_in_teams_changed", bool)
    on_unbounded_mode_changed = _EventProperty("on_unbounded_mode_changed", bool)
    on_subpart_mode_changed = _EventProperty("on_subpart_mode_changed", bool)

    def _internal_handle_volume_container_properties_changed(self, event: dict) -> None:
        # NOTE: the data inside event must match BuildEventData() function in PyVaInterop.cpp
        with VaScopedTrace(f"VolumeContainer._internal_handle_volume_container_properties_changed() {self.handle} event={event}"):
            new_modes: int = event["current_modes"]
            changes: int = self.__current_modes ^ new_modes
            if changes != 0:
                self.__current_modes = new_modes

                if changes & VA_VOLUME_CONTAINER_MODE_INTERACTIVE_MODE_EXT:
                    if self.__on_interactive_mode_changed is not None:
                        self.__on_interactive_mode_changed(self.__current_modes & VA_VOLUME_CONTAINER_MODE_INTERACTIVE_MODE_EXT != 0)

                if changes & VA_VOLUME_CONTAINER_MODE_ONE_TO_ONE_MODE_EXT:
                    if self.__on_one_to_one_mode_changed is not None:
                        self.__on_one_to_one_mode_changed(self.__current_modes & VA_VOLUME_CONTAINER_MODE_ONE_TO_ONE_MODE_EXT != 0)

                if changes & VA_VOLUME_CONTAINER_MODE_SHAREABLE_IN_TEAMS_EXT:
                    if self.__on_sharing_in_teams_changed is not None:
                        self.__on_sharing_in_teams_changed(self.__current_modes & VA_VOLUME_CONTAINER_MODE_SHAREABLE_IN_TEAMS_EXT != 0)

                if changes & VA_VOLUME_CONTAINER_MODE_UNBOUNDED_MODE_EXT:
                    if self.__on_unbounded_mode_changed is not None:
                        self.__on_unbounded_mode_changed(self.__current_modes & VA_VOLUME_CONTAINER_MODE_UNBOUNDED_MODE_EXT != 0)

                if changes & VA_VOLUME_CONTAINER_MODE_SUBPART_MODE_EXT:
                    if self.__on_subpart_mode_changed is not None:
                        self.__on_subpart_mode_changed(self.__current_modes & VA_VOLUME_CONTAINER_MODE_SUBPART_MODE_EXT != 0)


class Volume:
    '''
    A Volume defines a 3D space that encapsulates visuals, dynamics, and data for spatial presentation and interaction.
    '''

    def __init__(self, app: 'VolumetricApp', is_restorable: bool = False, restore_id: Optional[uuid.UUID] = None):
        with VaScopedTrace(f"Volume() app={app.handle} is_restorable={is_restorable} restore_id={restore_id}"):
            self.__app = app
            self.__handle: VaVolume = PyVa_CreateVolume(app.handle, app._current_system_id, is_restorable, str(restore_id))
            va_trace_info(f"PyVa_CreateVolume handle={self.handle}")
            self.__elements: list[Element] = []
            self.__tasks: list[Callable] = []
            self.__state: VaVolumeState = 0
            self.__volume_update_mode = VA_VOLUME_UPDATE_MODE_ON_DEMAND
            self.__content: Optional[VolumeContent] = None
            self.__container: Optional[VolumeContainer] = VolumeContainer(self)
            self.__on_ready_dispatched: bool = False
            self.__restore_result_received: bool = False
            self.__on_restore_result_dispatched: bool = False
            self.__restore_result_value: Optional[VaVolumeRestoredResultExt] = None
            app._internal_add_volume(self)

    @property
    def app(self) -> 'VolumetricApp':
        '''
        Get the VolumetricApp instance that owns the volume.
        '''
        return self.__app

    @property
    def handle(self) -> VaVolume:
        '''
        Get the unique int64 handle of the volume.
        '''
        return self.__handle

    def destroy(self) -> None:
        '''
        Destroy the volume and disconnect from all elements.
        '''
        with VaScopedTrace(f"Volume.destroy() volume={self.handle}"):
            # Destroy elements in reverse order of creation to ensure dependent elements
            # (children, references) are destroyed before the elements they depend on.
            for element in reversed(self.__elements):
                element.destroy()
            self.__elements.clear()
            va_trace_info(f"Volume.clear volume={self.handle}")
            PyVa_DestroyVolume(self.handle)

    def remove_element(self, element_to_remove: Element) -> None:
        '''
        Removes an element from the Volume.
        '''
        self.__elements.remove(element_to_remove)

        if element_to_remove is self.__container:
            self.__container = None

        if element_to_remove is self.__content:
            self.__content = None

        element_to_remove.destroy()

    def on_ready(self) -> None:
        '''
        Callback that is invoked once when the volume is ready to be used.
        This is raised for the very first frame after the volume is created.
        The application typically can start creating elements and set properties in this callback.
        This callback is guaranteed to be raised exactly once before any on_update callbacks.
        If subscribed after the volume is already running, the callback will be invoked on the next event processing cycle.
        Can be overridden in subclasses or assigned as a callback.
        '''
        pass

    def on_update(self) -> None:
        '''
        Callback that is invoked when the volume is updated.
        '''
        pass

    def on_pause(self) -> None:
        '''
        Callback that is invoked when the volume is paused.
        The volume can be paused when the user or the system minimizes the volume.
        '''
        pass

    def on_resume(self) -> None:
        '''
        Callback that is invoked when the volume is resumed.
        The volume can be resumed when the user or the system restores the volume.
        '''
        pass

    def on_close(self) -> None:
        '''
        Callback that is invoked when the volume is closed.
        '''
        pass

    def is_running(self) -> bool:
        '''
        Check if the volume is running.
        '''
        return self.__state == VA_VOLUME_STATE_RUNNING

    def is_closed(self) -> bool:
        '''
        Check if the volume is closed.
        '''
        return self.__state == VA_VOLUME_STATE_CLOSED

    def on_restore_result(self, result: VaVolumeRestoredResultExt) -> None:
        '''Callback that is invoked when the volume restore result is available.

        If subscribed after the restore result has already been received, the callback
        will be invoked on the next event processing cycle.
        '''
        pass

    @property
    def content(self) -> VolumeContent:
        '''
        Get the VolumeContent element of the volume.
        There can be only one VolumeContent in a Volume.
        It is created automatically when the first time it is accessed.
        '''
        if self.__content is None:
            self.__content = VolumeContent(self)
        return self.__content

    @property
    def container(self) -> VolumeContainer:
        '''
        Get the VolumeContainer element of the volume.
        There can be only one VolumeContainer in a Volume.
        It is created automatically when the first time it is accessed.
        '''
        if self.__container is None:
            self.__container = VolumeContainer(self)
        return self.__container

    @property
    def restore_id(self) -> uuid.UUID:
        '''
        Get the restore ID of the volume.
        The restore ID is used to restore the volume state after the application is restarted.
        '''
        return uuid.UUID(PyVa_GetVolumeRestoreIdExt(self.handle))

    def _internal_handle_volume_state_changed(self, state: int, action: int) -> None:
        self.__state = state
        if action == VA_VOLUME_STATE_ACTION_ON_READY:
            self._internal_handle_update_frame(1)
        elif action == VA_VOLUME_STATE_ACTION_ON_CLOSE:
            self.__tasks.clear()
            self.on_close()
            self.__app._internal_remove_volume(self)
        elif action == VA_VOLUME_STATE_ACTION_ON_PAUSE:
            self.on_pause()
        elif action == VA_VOLUME_STATE_ACTION_ON_RESUME:
            self.on_resume()

    def _internal_handle_update_frame(self, frame_id: int) -> None:
        PyVa_BeginUpdateVolume(self.handle, frame_id, 0, 0)
        try:
            for task in self.__tasks:
                task(self)
            self.__tasks.clear()

            if frame_id == 1:
                self.__on_ready_dispatched = True
                self.on_ready()
            else:
                self.on_update()
        finally:
            PyVa_EndUpdateVolume(self.handle, frame_id)

    def request_update(self, volume_update_mode: VaVolumeUpdateMode = VA_VOLUME_UPDATE_MODE_ON_DEMAND) -> None:
        '''
        Request the cadence of the volume update callback.
        If the volume_update_mode is ON_DEMAND, the volume will only update once after the request.
        If the volume_update_mode is FULL_FRAMERATE, or alike, the volume will update repeated in requested cadence.
        The platform will schedule the update callback as precise as system resources allow,
        but the actual update rate may vary depending on the system load, volume place to the user, and other factors.
        The application should not rely on the update rate to be exact.
        '''
        self.__volume_update_mode = volume_update_mode
        PyVa_RequestUpdateVolume(self.handle, self.__volume_update_mode, 0)

    def request_update_after(self, delay_ns: int) -> None:
        '''
        Deprecated: use request_update_after_seconds instead.
        Request an on-demand update with given delay.
        '''
        _warnings.warn(
            "request_update_after_ns is deprecated; use request_update_after_seconds instead",
            DeprecationWarning,
            stacklevel=2,  # point to caller
        )
        self.__volume_update_mode = VA_VOLUME_UPDATE_MODE_ON_DEMAND
        seconds : float = delay_ns / 1_000_000_000.0
        PyVa_RequestUpdateVolume(self.handle, self.__volume_update_mode, seconds)

    def request_update_after_seconds(self, delay_seconds: float) -> None:
        '''
        Request an on-demand update with given delay.
        '''
        self.__volume_update_mode = VA_VOLUME_UPDATE_MODE_ON_DEMAND
        PyVa_RequestUpdateVolume(self.handle, self.__volume_update_mode, delay_seconds)

    def _internal_add_element(self, element: Element) -> None:
        self.__elements.append(element)

    def dispatch_to_next_update(self, task: Callable) -> None:
        '''
        Dispatch a task to the next update frame.
        The task will be executed in the next update callback in the correct update scope
        Use this method to avoid "function call out of update scope" error.
        '''
        self.__tasks.append(task)

        # Also request an update if the volume is not running repeated update.
        if self.__volume_update_mode == VA_VOLUME_UPDATE_MODE_ON_DEMAND:
            # va_trace_info(f"Volume.dispatch_to_next_update on_demand")
            # Note: on demand request will coalease multiple requests into one update per platform frame boundary.
            self.request_update(VA_VOLUME_UPDATE_MODE_ON_DEMAND)

    def request_close(self) -> None:
        '''
        Request to close the volume.
        The volume will be closed after any outstanding update and receive the volume.onClose callback.
        '''
        with VaScopedTrace(f"Volume.request_close volume={self.handle}"):
            if not self.is_closed():
                PyVa_RequestCloseVolume(self.handle)

    def __get_element_or_throw(self, handle: VaElement) -> Element:
        for element in self.__elements:
            if handle == element.handle:
                return element
        raise LookupError("Element not found.")

    def _internal_handle_adaptive_card_action_invoked(self, element_handle: VaElement) -> None:
        element = self.__get_element_or_throw(element_handle)
        element._internal_poll_adaptive_card_action_invoke_data()

    def _internal_handle_async_state_changed(self) -> None:
        for element in self.__elements:
            element._internal_update_async_state()

    def _try_dispatch_pending_on_restore_result(self) -> None:
        '''Dispatch on_restore_result if result was received but not yet dispatched.

        Also handles monkey-patching: if a new on_restore_result method has been assigned
        to the instance (detected via presence in instance __dict__), dispatch it and mark
        as dispatched.
        '''
        if not self.__restore_result_received:
            return

        has_instance_on_restore_result = 'on_restore_result' in self.__dict__

        if has_instance_on_restore_result or not self.__on_restore_result_dispatched:
            self.__on_restore_result_dispatched = True
            if has_instance_on_restore_result:
                on_restore_result_func = self.__dict__.pop('on_restore_result')
                on_restore_result_func(self.__restore_result_value)
            else:
                self.on_restore_result(self.__restore_result_value)

    def _internal_handle_restore_result(self, result: VaVolumeRestoredResultExt) -> None:
        '''Handle a restore result event from the runtime.'''
        self.__restore_result_value = result
        self.__restore_result_received = True
        has_instance_on_restore_result = 'on_restore_result' in self.__dict__
        if has_instance_on_restore_result or type(self).on_restore_result is not Volume.on_restore_result:
            self.__on_restore_result_dispatched = True
            self.on_restore_result(result)

    def _try_dispatch_pending_on_ready(self) -> None:
        '''Dispatch on_ready if volume is running but on_ready was not yet dispatched.
        
        Also handles monkey-patching: if a new on_ready method has been assigned to the instance
        (detected via presence in instance __dict__), dispatch it and mark as dispatched.
        '''
        if not self.is_running():
            return
        
        # Detect monkey-patching: Python allows assigning functions to instance attributes.
        # When `volume.on_ready = some_func` is called, the function is stored in `volume.__dict__`.
        # This shadows the base class method. We check `__dict__` directly because:
        # 1. `hasattr()` would return True for the base class method too
        # 2. `getattr()` resolves the MRO chain, hiding whether it's an instance override
        # By checking `'on_ready' in self.__dict__`, we detect only instance-level assignments.
        has_instance_on_ready = 'on_ready' in self.__dict__
        
        if has_instance_on_ready or not self.__on_ready_dispatched:
            self.__on_ready_dispatched = True
            # Clear the instance override after dispatching to prevent repeated calls
            if has_instance_on_ready:
                on_ready_func = self.__dict__.pop('on_ready')
                on_ready_func()
            else:
                self.on_ready()


class VolumetricApp:
    '''
    The VolumetricApp is the top level object to create and manage the volumetric application.
    The successful creation of the VolumetricApp class initializes the volumetric runtime,
    validates API version and available extensions, and enables requested extensions.
    Otherwise, the creation fails and exception is thrown.
    The creation of the VolumetricApp also indicates the intention to connect to the system.
    The on_start callback is called when the system is connected for the first time.
    It is invalid to create any volume before the system is connected.
    '''
    EventHandler = Callable[['VolumetricApp'], None]

    def __init__(
            self,
            application_name: str,
            requested_extensions: list[str],
            volume_restore_behavior: int = VA_VOLUME_RESTORE_BEHAVIOR_NO_RESTORE_EXT,
            wait_for_system_behavior: int = VA_SESSION_WAIT_FOR_SYSTEM_BEHAVIOR_RETRY_WITH_USER_CANCEL):
        with VaScopedTrace(f"VolumetricApp() application_name={application_name} requested_extensions={requested_extensions} volume_restore_behavior={volume_restore_behavior} wait_for_system_behavior={wait_for_system_behavior}"):
            self.__handle: VaSession = 0
            self._lock = _Lock()
            self._volumes: list[Volume] = []
            self._started = False
            self._stopped = False
            self._current_system_id: VaSystemId = 0

            # Event is called when the system is connected for the first time.
            self.__on_start: Optional[VolumetricApp.EventHandler] = None
            self.__on_start_dispatched: bool = False

            # Event is called when the app is stopped.
            self.on_stop: Optional[VolumetricApp.EventHandler] = None

            # Event is called when the app is disconnected from the system.
            self.on_disconnect: Optional[VolumetricApp.EventHandler] = None

            # Event is called when the app is reconnected to the system.
            self.on_reconnect: Optional[VolumetricApp.EventHandler] = None

            # Event is called when the volume collection is changed.
            self.on_volume_collection_changed: Optional[VolumetricApp.EventHandler] = None

            # Event is called by Platform to restore a new volume from the restore ID.
            self.on_volume_restore_request: Optional[Callable[['VolumetricApp', uuid.UUID], None]] = None

            # Event is called when the restore ID is invalidated.
            self.on_volume_restore_id_invalidated: Optional[Callable[['VolumetricApp', uuid.UUID], None]] = None

            # Callback for derived classes to add custom event processing logic.
            # Returns True if the event was handled by the callback, False otherwise.
            self._preprocess_event: Optional[Callable[[dict], bool]] = None

            self._loop = _asyncio.new_event_loop()
            self.on_fatal_error: Optional[Callable[[str], None]] = None
            self.__fatal_error_message: Optional[str] = None
            self._run_thread: Optional[_Thread] = None

            # All requested extensions are considered "required" at the moment.
            # Therefore a successful creation of the session means all requested extensions are enabled.
            self._internal_enabled_extensions = requested_extensions

            PyVa_LoadRuntime()
            self.__handle = PyVa_CreateSession(application_name, wait_for_system_behavior, volume_restore_behavior, requested_extensions)
            va_trace_info(f"VolumetricApp: handle={self.handle}")

    @property
    def on_start(self) -> Optional[EventHandler]:
        '''Get the on_start callback.'''
        return self.__on_start

    @on_start.setter
    def on_start(self, value: Optional[EventHandler]) -> None:
        '''Set the on_start callback.
        
        Called once when the first volumetric system is connected and the app is ready to run.
        In this callback, a valid system Id is already available.
        The application can start creating new volumes and contents inside it.
        This callback is guaranteed to be raised exactly once before any other app events.
        If assigned after the app is already started, the callback will be invoked on the next event processing cycle.
        '''
        self.__on_start = value

    @property
    def handle(self) -> VaRuntime:
        '''
        Get the unique int64 handle of the VolumetricApp.
        '''
        return self.__handle

    def __terminate(self, timeout: float = 5.0) -> None:
        with VaScopedTrace("VolumetricApp.__terminate()"):
            if self._stopped:
                return  # App is already stopped, no need to terminate again

            if self.__fatal_error_message is not None:
                self._stopped = True
                if self.on_fatal_error is not None:
                    self.on_fatal_error(self.__fatal_error_message)
                return  # Fatal error occurred, no need to continue

            with self._lock:
                for volume in self._volumes:
                    volume.destroy()
                self._volumes.clear()
            if self.on_stop is not None:
                self.on_stop(self)
            # There's no turning back after this point
            # The app is stopped and all handles will be destroyed
            self._stopped = True
            PyVa_DestroySession(self.__handle)

            # If run_async() was used, wait for the background thread to finish.
            # Use a timeout to prevent hanging if the thread is unresponsive.
            if self._run_thread is not None and self._run_thread.is_alive():
                self._run_thread.join(timeout=timeout)

    def __should_continue(self) -> bool:
        if self.__fatal_error_message is not None:
            return False    # Fatal error occurred, stop the app
        if self._stopped:
            return False    # App is already stopped and there's no turnning back
        if not self._started:
            # App is not started yet, wait for first system connection.
            return True
        return True

    def poll_events(self) -> bool:
        """
        Process events until the event queue is empty
        and then yield control to the caller.
        """
        try:
            while self.__should_continue():
                event: dict = PyVa_PollEvent(self.handle)
                if event["result"] == VA_EVENT_UNAVAILABLE:
                    break
                self.__process_event(event)
        except Exception as e:
            va_trace_error(f"Fatal error encountered: {e}")
            self.__fatal_error_message = str(e)

        self._process_pending_callbacks()

        if self.__should_continue():
            return True  # Continue running the app
        else:
            self.__terminate()
            return False  # Stop running the app

    def run(self, on_start: Optional[EventHandler] = None) -> None:
        '''
        Start running the app and event loop.
        The on_start callback is invoked when a system is connected.
        This method will block until the session is stopped,
        though the event loop is running in an asyncio loop.
        '''
        with VaScopedTrace("VolumetricApp.run()"):
            if on_start is not None:
                self.on_start = on_start
            self._loop.run_until_complete(self.__run())

    def run_async(self, on_start: Optional[EventHandler] = None) -> _Thread:
        '''
        Start running the app and event loop in a background thread.
        This method returns immediately after starting the thread.
        The returned thread will run until the session is stopped or exited.
        
        Args:
            on_start: Optional callback invoked when the system is connected.
            
        Returns:
            The background thread running the app event loop.
            
        Raises:
            RuntimeError: If the app is already running in a background thread.
        '''
        if self._run_thread is not None and self._run_thread.is_alive():
            raise RuntimeError("run_async called while app is already running")
        
        if on_start is not None:
            self.on_start = on_start
        
        # Use daemon=True so the thread won't block process exit if user forgets request_exit().
        # Cleanup is handled in __terminate() which waits for the thread with a timeout.
        self._run_thread = _Thread(target=self.run, daemon=True)
        self._run_thread.start()
        return self._run_thread

    async def __run(self) -> None:
        with VaScopedTrace("VolumetricApp.__run()"):
            try:
                while self.__should_continue():
                    self.poll_events()
                    await _asyncio.sleep(0.001)
                self.__terminate()
            except Exception as e:
                va_trace_error(f"Fatal error encountered: {e}")
                self.__fatal_error_message = str(e)
                self.__terminate()

    def is_started(self) -> bool:
        '''
        Check if a system has been connected and the app is started.
        '''
        return self._started

    def is_connected(self) -> bool:
        '''
        Check if the app is currently connected to a system.
        '''
        return self._current_system_id != 0

    def is_stopped(self) -> bool:
        '''
        Check if the app has been stopped.
        '''
        return self._stopped

    def request_exit(self) -> bool:
        """Request to stop the session and exit the application.
        If there are any active volumes, they will be first close.
        Each active volume's on_close will be invoked, and at the end, the app.onExit will be invoked.
        """
        with VaScopedTrace("VolumetricApp.request_exit()"):
            PyVa_RequestStopSession(self.handle)

    def _internal_add_volume(self, volume: Volume) -> None:
        with VaScopedTrace(f"VolumetricApp._internal_add_volume volume={volume.handle}"):
            with self._lock:
                self._volumes.append(volume)
            if self.on_volume_collection_changed is not None:
                self.on_volume_collection_changed(self)

    def _internal_remove_volume(self, volume: Volume) -> None:
        with self._lock:
            self._volumes.remove(volume)
            volume.destroy()
        if self.on_volume_collection_changed is not None:
            self.on_volume_collection_changed(self)

    def has_active_volume(self) -> bool:
        """Check if there are any active volumes.
        """
        with self._lock:
            for volume in self._volumes:
                if not volume.is_closed():
                    return True
        return False

    def get_local_asset_uri(self, file_path: str) -> str:
        """ Get the local asset URI for the given file path
        Args:
            file_path (str): A file path to the asset, can be absolute path or relative to the function caller script file.
        """
        if _os.path.isabs(file_path):
            absolute_path = file_path
        else:
            caller_frame = _inspect.stack()[1]
            caller_file = caller_frame.filename
            caller_dir = _os.path.dirname(_os.path.abspath(caller_file))
            absolute_path = _Path(caller_dir) / file_path
        return _Path(absolute_path).resolve().as_uri()

    def remove_restorable_volume(self, restore_id: uuid.UUID) -> None:
        """Remove a restorable volume by its restore ID.
        This method is used to remove a volume that is no longer needed.
        It will not destroy the volume, but will remove it from the list of restorable volumes.
        """
        va_trace_info(f"VolumetricApp.remove_restorable_volume restore_id={restore_id}")
        PyVa_RemoveRestorableVolumeExt(self.handle, str(restore_id))

    def _try_dispatch_pending_on_start(self) -> None:
        '''Dispatch on_start if app is started, callback is set, and on_start was not yet dispatched.'''
        if self._started and self.__on_start is not None and not self.__on_start_dispatched:
            self.__on_start_dispatched = True
            self.__on_start(self)

    def _process_pending_callbacks(self) -> None:
        '''Process any pending callbacks for late subscriptions.'''
        self._try_dispatch_pending_on_start()
        with self._lock:
            for volume in self._volumes:
                volume._try_dispatch_pending_on_restore_result()
                volume._try_dispatch_pending_on_ready()

    def __get_volume_or_throw(self, handle) -> Volume:
        # va_trace_info(f"VolumetricApp._get_volume_or_throw size={len(self._volumes)}")
        with self._lock:
            for volume in self._volumes:
                if handle == volume.handle:
                    return volume
        raise LookupError("Volume not found.")

    def __process_event(self, event: dict) -> None:
        # Allow derived classes to add custom event processing logic
        if self._preprocess_event is not None:
            if self._preprocess_event(event):
                return  # Event was handled by the callback

        event_type = event["type"]
        if event_type == VA_TYPE_EVENT_CONNECTED_SYSTEM_CHANGED:
            self.__handle_connected_system_changed(event["system_id"])
        elif event_type == VA_TYPE_EVENT_SESSION_STOPPED:
            self.__terminate()
        elif event_type == VA_TYPE_EVENT_VOLUME_STATE_CHANGED:
            volume = self.__get_volume_or_throw(event["volume"])
            volume._internal_handle_volume_state_changed(event["state"], event["action"])
        elif event_type == VA_TYPE_EVENT_UPDATE_VOLUME:
            volume = self.__get_volume_or_throw(event["volume"])
            frame_id = event["frame_id"]
            volume._internal_handle_update_frame(frame_id)
        elif event_type == VA_TYPE_EVENT_ADAPTIVE_CARD_ACTION_INVOKED_EXT:
            volume = self.__get_volume_or_throw(event["volume"])
            volume._internal_handle_adaptive_card_action_invoked(event["element"])
        elif event_type == VA_TYPE_EVENT_ELEMENT_ASYNC_STATE_CHANGED:
            volume = self.__get_volume_or_throw(event["volume"])
            volume._internal_handle_async_state_changed()
        elif event_type == VA_TYPE_EVENT_VOLUME_CONTAINER_MODE_CHANGED_EXT:
            volume = self.__get_volume_or_throw(event["volume"])
            volume.container._internal_handle_volume_container_properties_changed(event)
        elif event_type == VA_TYPE_EVENT_VOLUME_RESTORE_RESULT_EXT:
            volume = self.__get_volume_or_throw(event["volume"])
            volume._internal_handle_restore_result(event["volume_restore_result"])
        elif event_type == VA_TYPE_EVENT_VOLUME_RESTORE_REQUEST_EXT:
            restore_id = uuid.UUID(event["volume_restore_id"])

            if self.on_volume_restore_request is not None:
                self.on_volume_restore_request(self, restore_id)
        elif event_type == VA_TYPE_EVENT_VOLUME_RESTORE_ID_INVALIDATED_EXT:
            restore_id = uuid.UUID(event["volume_restore_id"])

            if self.on_volume_restore_id_invalidated is not None:
                self.on_volume_restore_id_invalidated(self, restore_id)
        else:
            # va_trace_info(f"_process_event event_type={event_type} not processed")
            pass

    def __handle_connected_system_changed(self, system_id: VaSystemId) -> None:
        with VaScopedTrace(f"VolumetricApp.__handle_connected_system_changed system_id={system_id}"):
            if self._current_system_id != system_id:
                if system_id != 0:
                    va_trace_info(f"A new system is connected, old={self._current_system_id}, new={system_id}")
                    self._current_system_id = system_id
                    if not self._started:
                        self._started = True
                        if self.__on_start is not None:
                            self.__on_start_dispatched = True
                            self.__on_start(self)
                    elif self.on_reconnect is not None:
                        self.on_reconnect(self)
                else:
                    va_trace_info(f"The current system is disconnected, old={self._current_system_id}, new={system_id}")
                    self._current_system_id = system_id
                    if self.on_disconnect is not None:
                        self.on_disconnect(self)
