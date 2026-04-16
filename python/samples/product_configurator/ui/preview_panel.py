import dearpygui.dearpygui as dpg
import os
from utils.paths import get_assets_path

texture_image_paths = [
        f"{get_assets_path()}/headphone.png",
        f"{get_assets_path()}/headBand.png",
        f"{get_assets_path()}/speaker.png"
    ]

def draw_preview_panel():
    def load_images(paths):
        # Loads preview images into the DearPyGui texture registry. Tags are assigned as img1, img2, img3, etc
        with dpg.texture_registry(show=False):
            for i, path in enumerate(paths):
                if not os.path.exists(path):
                    print(f"File not found: {path}")
                    continue
                result = dpg.load_image(path)
                if result is None:
                    print(f"Could not load image: {path}")
                    continue
                width, height, channels, data = result
                dpg.add_static_texture(width, height, data, tag=f"img{i+1}")

    load_images(texture_image_paths)

    color = dpg.get_value("color_picker_tag") or [1, 1, 1, 1]

    # Panel title
    with dpg.child_window(width=-1, height=55, border=False):
        dpg.add_text("Preview", tag="preview_title", wrap=0, bullet=False)

    image_x = 70
    image_y = 10
    offset = 200
    
    # Drawlist with preview images
    with dpg.child_window(width=-1, height=210, border=False):
        with dpg.drawlist(width=350, height=180):
            dpg.draw_image("img1", (image_x, image_y), (image_x + offset, image_y + offset), color=color, tag="preview_panel_headphones")
            dpg.draw_image("img2", (image_x, image_y), (image_x + offset, image_y + offset), color=color, tag="preview_panel_headband")
            dpg.draw_image("img3", (image_x, image_y), (image_x + offset, image_y + offset), color=color, tag="preview_panel_speakers")
            dpg.draw_image("img1", (image_x, image_y), (image_x + offset, image_y + offset), color=color, tag="preview_panel_texture", show=False)
            dpg.draw_image("img1", (image_x, image_y), (image_x + offset, image_y + offset), color=color, tag="preview_panel_accesory_0", show=False)
            dpg.draw_image("img1", (image_x, image_y), (image_x + offset, image_y + offset), color=color, tag="preview_panel_accesory_1", show=False)
            dpg.draw_image("img1", (image_x, image_y), (image_x + offset, image_y + offset), color=color, tag="preview_panel_accesory_2", show=False)