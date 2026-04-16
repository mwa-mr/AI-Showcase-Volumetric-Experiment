'''
This sample demonstrates adaptive card UX to browse all GLTF files in a folder using Volumetric SDK.
'''
import json
import os
import sys
import volumetric as va


class ModelViewer(va.Volume):
    '''
    This class demonstrates a child class of Volume that loads GLTF models in a volume.
    It also registers an AdaptiveCard and handles button clicks to load the next or previous model.
    '''

    def __init__(self, app: va.VolumetricApp, models: list = None, restore_id: str = None, restore_glb_uri: str = None):
        super().__init__(app, is_restorable=True, restore_id=restore_id)
        self.adaptive_card_element: va.AdaptiveCardElement = None
        self.visual: va.VisualElement = None
        self.model: va.ModelResource = None
        self.models: list = models  # List of tuples (uri, name)
        self.model_index_loaded: int = -1
        self.model_index_to_load: int = -1
        self.adaptive_card_template: str = """
            {
                "type": "AdaptiveCard",
                "body": [
                    {
                        "id": "modelInfo",
                        "type": "TextBlock",
                        "text": "${currentModel}",
                        "horizontalAlignment": "center"
                    }
                ],
                "actions": [
                    {
                        "type": "Action.Execute",
                        "title": "Prev Model",
                        "verb": "dec"
                    },
                    {
                        "type": "Action.Execute",
                        "title": "Next Model",
                        "verb": "inc"
                    }
                ],
                "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
                "version": "1.4"
            }
            """
        self.state = {
            "restore_id": restore_id,
            "glb_uri": restore_glb_uri,
        }

    def _format_adaptive_card_data(self, model_name: str) -> str:
        return f'{{"currentModel":"{model_name}"}}'

    def _start_loading_model(self, index: int) -> None:
        print(f"ModelViewer._start_loading_model index={index}")
        if not 0 <= index < len(self.models):
            print(f"ModelViewer._start_loading_model invalid model index={index} models_len={len(self.models)}")
            return
        if self.model_index_loaded == index:
            print(f"ModelViewer._start_loading_model already loaded index={index}")
            return
        model_uri, model_name = self.models[index]
        self.model_index_to_load = index
        self.model.set_model_uri(model_uri)

        self.state["glb_uri"] = self.models[self.model_index_to_load][0]
        self._save_state()

        adaptive_card_data = self._format_adaptive_card_data(f"Loading {model_name} ...")
        self.adaptive_card_element.set_template(self.adaptive_card_template)
        self.adaptive_card_element.set_data(adaptive_card_data)
        print(f"ModelViewer._start_loading_model load model started, name={model_name} index={index}")

    def on_ready(self) -> None:
        self.model = va.ModelResource(self)
        self.model.on_async_state_changed = self._on_model_async_state_changed
        self.visual = va.VisualElement.create_with_visual_resource(self, visual_resource=self.model)
        self.adaptive_card_element = va.AdaptiveCardElement(self, self._on_adaptive_card_action)
        self.state["restore_id"] = str(self.restore_id)

        index_to_load = 0

        if self.state["glb_uri"]:
            for index, (uri, _) in enumerate(self.models):
                if uri == self.state["glb_uri"]:
                    index_to_load = index
                    break

        self._start_loading_model(index_to_load)

    def on_update(self) -> None:
        # Load the next model if requested
        if self.model_index_to_load != -1:
            print(f"ModelViewer.on_update indx_loaded={self.model_index_loaded} indx_2_load={self.model_index_to_load}")
            self._start_loading_model(self.model_index_to_load)
            # Here the new index is applied even if the actual model is not yet loaded.
            self.model_index_loaded = self.model_index_to_load
            self.model_index_to_load = -1

            # Apply the new model name to the volume title
            _, model_name = self.models[self.model_index_loaded]
            self.container.set_display_name(f"Model Viewer - {model_name}")


    def _on_adaptive_card_action(self, verb: str):
        print(f"ModelViewer._on_adaptive_card_action verb={verb}")
        if verb == "inc":
            self.model_index_to_load = (
                self.model_index_loaded + 1) % len(self.models)
        elif verb == "dec":
            self.model_index_to_load = (
                self.model_index_loaded - 1) % len(self.models)
        self.request_update()  # Request update to load the new model

    def _update_adaptive_card(self):
        _, model_name = self.models[self.model_index_loaded]
        adaptive_card_data = self._format_adaptive_card_data(f"Model name {model_name}")
        self.adaptive_card_element.set_data(adaptive_card_data)

    def _on_model_async_state_changed(self, element: va.VisualElement, state: int) -> None:
        _, model_name = self.models[self.model_index_loaded]
        self.container.set_display_name(f"Model Viewer - {model_name}")
        if state == va.VA_ELEMENT_ASYNC_STATE_READY:
            adaptive_card_data = self._format_adaptive_card_data(f"{model_name}")
            self.adaptive_card_element.set_data(adaptive_card_data)
        elif state == va.VA_ELEMENT_ASYNC_STATE_PENDING:
            adaptive_card_data = self._format_adaptive_card_data(f"Loading {model_name}...")
            self.adaptive_card_element.set_data(adaptive_card_data)
        elif state == va.VA_ELEMENT_ASYNC_STATE_ERROR:
            adaptive_card_data = self._format_adaptive_card_data(f"Model load failed. {model_name}")
            self.adaptive_card_element.set_data(adaptive_card_data)

    def on_close(self) -> None:
        self.visual = None
        self.model = None
        self.adaptive_card_element = None
        # This sample has only one volume. Hence, the app exits when the user closes the volume.
        self.app.request_exit()

    def on_restore_result(self, result: va.VaVolumeRestoredResultExt) -> None:
        if result != va.VA_VOLUME_RESTORED_RESULT_SUCCESS_EXT:
            print(f"ModelViewer.on_restore_result failed with result={result}")
            _delete_state_file()
            # Now reset the state so we load in the default way, not the restored way.
            self.state = {
                "restore_id": None,
                "glb_uri": None,
            }

    def _save_state(self) -> None:
        local_app_data = os.environ.get("LOCALAPPDATA")
        if not local_app_data:
            return

        state_file_path = os.path.join(local_app_data, "py_model_viewer_state.json")

        with open(state_file_path, "w", encoding="utf-8") as state_file:
            json.dump(self.state, state_file, indent=4)


def _main() -> None:
    app = va.VolumetricApp("Python Model Viewer", [va.VA_EXT_GLTF2_MODEL_RESOURCE_EXTENSION_NAME,
                                                   va.VA_EXT_ADAPTIVE_CARD_ELEMENT_EXTENSION_NAME,
                                                   va.VA_EXT_VOLUME_RESTORE_EXTENSION_NAME],
                           volume_restore_behavior=va.VA_VOLUME_RESTORE_BEHAVIOR_BY_APP_EXT)
    app.on_volume_restore_id_invalidated = _on_volume_restore_id_invalidated
    app.run(_on_start)
    print("Done.")


def _load_state() -> dict:
    local_app_data = os.environ.get("LOCALAPPDATA")
    if not local_app_data:
        print("LOCALAPPDATA environment variable is not set.")
        return None

    state_file_path = os.path.join(local_app_data, "py_model_viewer_state.json")
    if not os.path.exists(state_file_path):
        return None

    try:
        with open(state_file_path, "r", encoding="utf-8") as state_file:
            state = json.load(state_file)
            if not isinstance(state, dict):
                raise ValueError("Invalid state data format")
            return state
    except (json.JSONDecodeError, ValueError, OSError):
        os.remove(state_file_path)
        return None


def _delete_state_file() -> None:
    print("_delete_state_file")
    local_app_data = os.environ.get("LOCALAPPDATA")
    if not local_app_data:
        return

    state_file_path = os.path.join(local_app_data, "py_model_viewer_state.json")
    if os.path.exists(state_file_path):
        try:
            os.remove(state_file_path)
        except OSError as e:
            print(f"Error deleting state file: {e}")


def _on_volume_restore_id_invalidated(app: va.VolumetricApp, restore_id: str) -> None:
    print(f"_on_volume_restore_id_invalidated restore_id={restore_id}")
    # Just assume this is the cached restore ID we have.
    _delete_state_file()


def _prepare_models_in_folder(folders: list) -> list:
    models = []

    for folder in folders:
        if not os.path.exists(folder):
            print(f"Folder not found: {folder}")
            continue
        for entry in os.scandir(folder):
            if entry.name.endswith(".glb") and entry.is_file():
                uri = "file:///" + \
                    os.path.abspath(os.path.join(
                        folder, entry.name)).replace("\\", "/")
                name = os.path.splitext(entry.name)[0]
                models.append((uri, name))

    if models:
        print(f"Found {len(models)} glb files:")
        for i, model in enumerate(models):
            print(f"Model[{i}]: {model[0]}")

    return models

def _prepare_models_from_file(file_path: str) -> str:
    models = []
    if os.path.isfile(file_path) and file_path.endswith(".glb"):
        uri= "file:///" + os.path.abspath(file_path).replace("\\", "/")
        name = os.path.splitext(os.path.basename(file_path))[0]
        models.append((uri, name))
    return models

def _get_default_models_folders() -> list:
    script_directory = os.path.dirname(os.path.abspath(__file__))
    models_directory = os.path.join(script_directory, "../../../assets")
    print(f"models_directory: {models_directory}")

    user_profile = os.environ.get("USERPROFILE")
    user_profile_3d_objects_directory = os.path.join(
        user_profile, "3D Objects")

    return [models_directory, user_profile_3d_objects_directory]

def _on_start(app: va.VolumetricApp) -> None:
    print("_on_start")
    if len(sys.argv) > 1:
        path = sys.argv[1]
        print(f"Using path argument: {path}")
        if os.path.isfile(path):
            ModelViewer(app, _prepare_models_from_file(path))
        elif os.path.isdir(path):
            ModelViewer(app, _prepare_models_in_folder([path]))
        else:
            print(f"Invalid path argument: {path}")
    else:   # No path argument provided, use default folders
        folders = _get_default_models_folders()
        saved_state = _load_state()

        if saved_state is not None:
            ModelViewer(app, _prepare_models_in_folder(folders), restore_id=saved_state["restore_id"],
                        restore_glb_uri=saved_state["glb_uri"])
        else:
            ModelViewer(app, _prepare_models_in_folder(folders))

if __name__ == '__main__':
    _main()
