import dearpygui.dearpygui as dpg
import logic.launcher as launcher
import logic.state_mutations as state_mutations
from ui.color_picker import draw_color_picker
from ui.accessories_panel import draw_accessories_panel
from ui.texture_selector import draw_texture_selector
from ui.preview_panel import draw_preview_panel
from ui.themes import create_white_bg_theme, create_section_theme

def resize_callback(sender, win_id, user_data):
    # Get current main window size
    main_width = dpg.get_item_width(win_id)
    main_height = dpg.get_item_height(win_id)
    
    # Calculate proportional sizes based on original ratios
    # Original: 390/810 = 0.48 for width, 290/660 = 0.44 for height
    child_width = int(main_width * 0.48)
    child_height = int((main_height - 40) * 0.44)  # Subtract footer height
    footer_width = int(main_width * 0.975)  # 790/810 = 0.975
    
    # Update all child window sizes
    dpg.configure_item("color_picker_section", width=child_width, height=child_height)
    dpg.configure_item("texture_selector_section", width=child_width, height=child_height)
    dpg.configure_item("accessories_panel_section", width=child_width, height=child_height)
    dpg.configure_item("preview_panel_section", width=child_width, height=child_height)
    dpg.configure_item("footer_section", width=footer_width)
    
    
def draw_main_layout(state):

    white_bg_theme = create_white_bg_theme()
    section_theme = create_section_theme()
    dpg.bind_theme(white_bg_theme)

    with dpg.window(label="Main window", no_title_bar=True, width=810, height=660, no_resize=False) as main_win_id:
        with dpg.item_handler_registry(tag="window_handler"):
            dpg.add_item_resize_handler(callback=resize_callback)
        dpg.bind_item_handler_registry(main_win_id, "window_handler")
        
        with dpg.group(horizontal=True):
            # Left column
            with dpg.group():
                with dpg.child_window(tag="color_picker_section", width=390, height=290):  # Top left
                    dpg.bind_item_theme("color_picker_section", section_theme)
                    draw_color_picker(state)
                with dpg.child_window(tag="texture_selector_section", width=390, height=290):  # Bottom left
                    dpg.bind_item_theme("texture_selector_section", section_theme)
                    draw_texture_selector(state)
            # Right column
            with dpg.group():
                with dpg.child_window(tag="accessories_panel_section", width=390, height=290):  # Top right
                    dpg.bind_item_theme("accessories_panel_section", section_theme)
                    draw_accessories_panel(state)
                with dpg.child_window(tag="preview_panel_section", width=390, height=290):  # Bottom right
                    dpg.bind_item_theme("preview_panel_section", section_theme)
                    draw_preview_panel()
        # Footer
        with dpg.child_window(tag="footer_section", width=790, height=40):
            dpg.bind_item_theme("footer_section", section_theme)
            with dpg.group(horizontal=True):
                dpg.add_text("Only Available in Meta Quest")
                dpg.add_spacer(width=345)
                dpg.add_button(
                    label="Deploy",
                    tag="deploy_button",
                    callback=lambda: (
                        state_mutations.update_deploy_button_state(state_mutations.DeployButtonState.LOADING.value, None, None, state),
                        launcher.launch_3d_process(state)
                    )
                )
                
        dpg.set_primary_window(main_win_id, True)
