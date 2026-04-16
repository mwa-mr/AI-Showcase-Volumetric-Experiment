import dearpygui.dearpygui as dpg
from ui.themes import create_disabled_button_theme
from logic.state_mutations import DeployButtonState

def update_color(state, sender):
    # Updates the color picker and preview panels based on ui interaction or state changes
    # Handles both element selection and color changes
    selected = dpg.get_value("element_selector")
    if sender == "element_selector":
        # User changed the selected element (Headband or Speakers)
        color = state.get(selected)
        dpg.set_value("color_picker_tag", color)
    elif sender == "color_picker_tag":
        # User changed the color in the color picker
        color = dpg.get_value("color_picker_tag")
        state[selected] = color
        if selected == "Headband":
            dpg.configure_item("preview_panel_headband", color=color)
        elif selected == "Speakers":
            dpg.configure_item("preview_panel_speakers", color=color)
    else:
        # If called from shuffle or external state change: update both previews
        dpg.configure_item("preview_panel_headband", color=state["Headband"])
        dpg.configure_item("preview_panel_speakers", color=state["Speakers"])

def update_textures(state):
    # Updates the texture preview panel based on the selected texture index in the state
    idx = state.get("Texture_idx", 0)
    texture_tag = f"img{idx}_preview"
    dpg.configure_item("preview_panel_texture", texture_tag=texture_tag, show=True)

def update_accessories(state):
        # Updates the accessory preview panels to show or hide each accessory based on the state
        active = state.get("Active_accessories", [])
        for idx in range(3):
            accesory_tag = f"img{idx}_preview_accesory"
            img_tag = f"preview_panel_accesory_{idx}"
            if idx in active:
                dpg.configure_item(img_tag, texture_tag=accesory_tag, show=True)
            else:
                dpg.configure_item(img_tag, texture_tag=accesory_tag, show=False)

def update_deploy_button(state):
    # Updates the deploy button's state, label and theme based on the current state
    btn_state = state.get("deploy_btn", DeployButtonState.LOADING)
    if btn_state == DeployButtonState.ENABLED.value:
        dpg.configure_item("deploy_button", enabled=True, label="Deploy")
        dpg.bind_item_theme("deploy_button", 0)
    elif btn_state == DeployButtonState.DISABLED.value:
        dpg.configure_item("deploy_button", enabled=False, label="Deploy")
        dpg.bind_item_theme("deploy_button", create_disabled_button_theme())
    elif btn_state == DeployButtonState.LOADING.value:
        dpg.configure_item("deploy_button", enabled=False, label="Loading...")
        dpg.bind_item_theme("deploy_button", create_disabled_button_theme())

def poll_and_update_ui(sender, app_data, user_data):
    # Main UI polling loop. Checks if the UI needs to be updated (ui_dirty flag). If so, updates all UI panels and the deploy button state
    state = user_data
    if state.get("ui_dirty"):
        update_color(state, None)
        update_textures(state)
        update_accessories(state)
        update_deploy_button(state)
        state["ui_dirty"] = False

    # Schedule the next poll after a fixed number of frames
    POLLING_INTERVAL_FRAMES = 10
    dpg.set_frame_callback(dpg.get_frame_count() + POLLING_INTERVAL_FRAMES, poll_and_update_ui, user_data=state)