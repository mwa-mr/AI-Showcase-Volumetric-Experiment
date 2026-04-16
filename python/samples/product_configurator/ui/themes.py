import dearpygui.dearpygui as dpg

background_white = (246, 246, 246, 255)
btn_color = (233, 233, 233, 255)
border_visible = (200, 200, 200, 255)

def create_white_bg_theme():
    with dpg.theme() as white_bg_theme:
        with dpg.theme_component(dpg.mvAll):
            dpg.add_theme_color(dpg.mvThemeCol_WindowBg, background_white)
            dpg.add_theme_color(dpg.mvThemeCol_ChildBg, background_white)
            dpg.add_theme_color(dpg.mvThemeCol_Text, (0, 0, 0, 255))
            dpg.add_theme_color(dpg.mvThemeCol_FrameBg, background_white)
            dpg.add_theme_color(dpg.mvThemeCol_ScrollbarBg, background_white)
            dpg.add_theme_color(dpg.mvThemeCol_ScrollbarGrab, (200, 200, 200, 255))
            dpg.add_theme_color(dpg.mvThemeCol_ScrollbarGrabHovered, (180, 180, 180, 255))
            dpg.add_theme_color(dpg.mvThemeCol_ScrollbarGrabActive, (160, 160, 160, 255))
            dpg.add_theme_color(dpg.mvThemeCol_Button, btn_color)
            dpg.add_theme_color(dpg.mvThemeCol_Border, (0, 0, 0, 0))
    return white_bg_theme

def create_section_theme():
    with dpg.theme() as section_theme:
        with dpg.theme_component(dpg.mvChildWindow):
            dpg.add_theme_color(dpg.mvThemeCol_Border, border_visible)
            dpg.add_theme_style(dpg.mvStyleVar_ChildRounding, 6)
    return section_theme

def create_disabled_button_theme():
    with dpg.theme() as disabled_button_theme:
        with dpg.theme_component(dpg.mvButton, enabled_state=False):
            dpg.add_theme_color(dpg.mvThemeCol_Button, (180, 180, 180, 255))
            dpg.add_theme_color(dpg.mvThemeCol_ButtonHovered, (180, 180, 180, 255))
            dpg.add_theme_color(dpg.mvThemeCol_ButtonActive, (180, 180, 180, 255))
    return disabled_button_theme