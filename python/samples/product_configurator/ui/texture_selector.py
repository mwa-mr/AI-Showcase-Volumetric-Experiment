import dearpygui.dearpygui as dpg
import os
import logic.ui_controller as ui_controller
import sys
from utils.paths import get_assets_path
# List of texture image trios (default, selected, preview)
texture_image_trios = [
    (f"{get_assets_path()}/EarCup1.png", f"{get_assets_path()}/EarCup1_selected.png", f"{get_assets_path()}/EarCup1_preview.png"),
    (f"{get_assets_path()}/EarCup2.png", f"{get_assets_path()}/EarCup2_selected.png", f"{get_assets_path()}/EarCup2_preview.png"),
    (f"{get_assets_path()}/EarCup3.png", f"{get_assets_path()}/EarCup3_selected.png", f"{get_assets_path()}/EarCup3_preview.png"),
    (f"{get_assets_path()}/EarCup4.png", f"{get_assets_path()}/EarCup4_selected.png", f"{get_assets_path()}/EarCup4_preview.png"),
    ]


def draw_texture_selector(state):
    # Draws the textures selection panel, loads texture images and manages selection state

    def load_texture_pairs(texture_trios):
        # Loads texture images (normal, selected, preview) into the DearPyGui texture registry
        with dpg.texture_registry(show=False):
            for i, (normal_path, selected_path, preview_path) in enumerate(texture_trios):
                # Normal
                if os.path.exists(normal_path):
                    result = dpg.load_image(normal_path)
                    if result:
                        width, height, channels, data = result
                        dpg.add_static_texture(width, height, data, tag=f"img{i}_normal")
                    else:
                        print(f"Could not load image: {normal_path}")
                else:
                    print(f"File not found: {normal_path}")

                # Selected
                if os.path.exists(selected_path):
                    result = dpg.load_image(selected_path)
                    if result:
                        width, height, channels, data = result
                        dpg.add_static_texture(width, height, data, tag=f"img{i}_selected")
                    else:
                        print(f"Could not load image: {selected_path}")
                else:
                    print(f"File not found: {selected_path}")

                # Preview
                if os.path.exists(preview_path):
                    result = dpg.load_image(preview_path)
                    if result:
                        width, height, channels, data = result
                        dpg.add_static_texture(width, height, data, tag=f"img{i}_preview")
                    else:
                        print(f"Could not load image: {preview_path}")
                else:
                    print(f"File not found: {preview_path}")
    
    load_texture_pairs(texture_image_trios)

    images = [
        {"normal": "img0_normal", "selected": "img0_selected"},
        {"normal": "img1_normal", "selected": "img1_selected"},
        {"normal": "img2_normal", "selected": "img2_selected"},
        {"normal": "img3_normal", "selected": "img3_selected"},
    ]
    selected_idx = {"value": 0}

    def select_texture(idx):
        if selected_idx["value"] != idx:
            selected_idx["value"] = idx
            state["Texture_idx"] = idx

    def draw_buttons():
        # Redraws texture buttons to reflect the current selection state
        dpg.delete_item("texture_selector_buttons", children_only=True)
        with dpg.group(horizontal=True, parent="texture_selector_buttons"):
            for i, tex in enumerate(images):
                def make_callback(idx=i):
                    return lambda: [
                        select_texture(idx), 
                        draw_buttons(),
                        ui_controller.update_textures(state)
                        ]
                dpg.add_image_button(
                    tex["selected"] if selected_idx["value"] == i else tex["normal"],
                    width=150,
                    height=150,
                    callback=make_callback()
                )

    # Panel title
    with dpg.child_window(width=-1, height=55, border=False):
        dpg.add_text("Textures", tag="textures_title", wrap=0, bullet=False)
    
    # Panel button
    with dpg.child_window(width=-1, height=180, border=False, horizontal_scrollbar=True):
        with dpg.group(tag="texture_selector_buttons"):
            pass
    
    draw_buttons()