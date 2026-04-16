import dearpygui.dearpygui as dpg
import os
import logic.ui_controller as ui_controller
from utils.paths import get_assets_path
# List of accessory image trios: (default, selected, preview)
accessory_image_trios = [
    (f"{get_assets_path()}/acc1.png", f"{get_assets_path()}/acc1sel.png", f"{get_assets_path()}/headset3.png"),
    (f"{get_assets_path()}/acc2.png", f"{get_assets_path()}/acc2sel.png", f"{get_assets_path()}/headset2.png"),
    (f"{get_assets_path()}/acc3.png", f"{get_assets_path()}/acc3sel.png", f"{get_assets_path()}/headset4.png"),
    ]

def draw_accessories_panel(state):
    # Draws the accessories selection panel, loads accessory images and manages selection state

    def load_images(accessories_trios):
        # Loads accessory images (normal, selected, preview) into the DearPyGui texture registry
        with dpg.texture_registry(show=False):
            for i, (normal_path, selected_path, preview_path) in enumerate(accessories_trios):
                # Normal
                if os.path.exists(normal_path):
                    result = dpg.load_image(normal_path)
                    if result:
                        width, height, channels, data = result
                        dpg.add_static_texture(width, height, data, tag=f"img{i}_normal_accesory")
                    else:
                        print(f"Could not load image: {normal_path}")
                else:
                    print(f"File not found: {normal_path}")

                # Selected
                if os.path.exists(selected_path):
                    result = dpg.load_image(selected_path)
                    if result:
                        width, height, channels, data = result
                        dpg.add_static_texture(width, height, data, tag=f"img{i}_selected_accesory")
                    else:
                        print(f"Could not load image: {selected_path}")
                else:
                    print(f"File not found: {selected_path}")

                # Preview 
                if os.path.exists(preview_path):
                    result = dpg.load_image(preview_path)
                    if result:
                        width, height, channels, data = result
                        dpg.add_static_texture(width, height, data, tag=f"img{i}_preview_accesory")
                    else:
                        print(f"Could not load image: {preview_path}")
                else:
                    print(f"File not found: {preview_path}")

    def toggle_accessory(idx):
        # Toggles the selection state of an accessory by index and updates the 'Active_accessories' list in the shared state
        active = state.get("Active_accessories", [])
        if idx in active:
            active.remove(idx)
        else:
            active.append(idx)
        state["Active_accessories"] = active

    def draw_buttons():
        # Redraws the accessory image buttons to reflect the current selection state
        dpg.delete_item("accessory_selector_buttons", children_only=True)
        with dpg.group(horizontal=True, parent="accessory_selector_buttons"):
            for i, tex in enumerate(images):
                def make_callback(idx=i):
                    # Callback toggles accessory, redraws buttons and updates the UI
                    return lambda: [
                        toggle_accessory(idx),
                        draw_buttons(),
                        ui_controller.update_accessories(state)
                        ]
                is_selected = i in state.get("Active_accessories")
                dpg.add_image_button(
                    tex["selected"] if is_selected else tex["normal"],
                    width=113,
                    height=180,
                    callback=make_callback()
                )

    # Load accessory images
    load_images(accessory_image_trios)
    
    # Define texture tags for each accessory button
    images = [
        {"normal": "img0_normal_accesory", "selected": "img0_selected_accesory"},
        {"normal": "img1_normal_accesory", "selected": "img1_selected_accesory"},
        {"normal": "img2_normal_accesory", "selected": "img2_selected_accesory"},
    ]
    
    # Panel title
    with dpg.child_window(width=-1, height=55, border=False):
        dpg.add_text("Accessories", tag="accessories_title", wrap=0, bullet=False)
    
    # Panel button
    with dpg.child_window(width=-1, height=210, border=False):
        with dpg.group(tag="accessory_selector_buttons"):
            pass
    
    draw_buttons()