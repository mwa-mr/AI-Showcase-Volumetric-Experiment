import dearpygui.dearpygui as dpg
import logic.ui_controller as ui_controller

def draw_color_picker(state):
    # Panel title
    with dpg.child_window(width=-1, height=55, border=False):
        dpg.add_text("Colors", tag="colors_title", wrap=0, bullet=False)
    
    # Color picker and element selector
    with dpg.child_window(width=-1, height=210, border=False):
        with dpg.group(horizontal=True):
            # Color picker widget
            dpg.add_color_picker(
                (255, 255, 255, 255),
                width=180,
                alpha_bar=True,
                tag="color_picker_tag",
                callback=lambda sender: ui_controller.update_color(state, sender)
            )
            # Radio button to select which element to color
            dpg.add_radio_button(
                items=["Headband", "Speakers"],
                tag="element_selector",
                default_value="Headband",
                callback=lambda sender: ui_controller.update_color(state, sender,)
            )
