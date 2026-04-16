# Entry point for the Product Configurator app.
# Sets up shared state, initializes DearPyGui and starts the UI event loop.

import sys
import dearpygui.dearpygui as dpg
from ui.layout import draw_main_layout
from multiprocessing import Manager
from logic.ui_controller import poll_and_update_ui
from logic.state_mutations import DeployButtonState
from logic.volumetric_experience import run_volumetric_experience

if __name__ == "__main__":

    deploy = False

    # The --deploy command line will simply deploy the volumetric process for the configurator
    # without spawing a subprocess to host it. 
    if len(sys.argv) > 1 and sys.argv[1].lower() == "--deploy":
        deploy = True

    # Create a shared state dictionary for inter-process communication
    manager = Manager()
    state = manager.dict({
            "Headband": [255, 255, 255, 255],
            "Speakers": [255, 255, 255, 255],
            "Texture_idx": 0,
            "Active_accessories": [],
            "ui_dirty": False,
            "deploy_btn": DeployButtonState.ENABLED.value,
    })

    if deploy:
        # Deploy the volume directly without creating the subprocess or showing any UI
        run_volumetric_experience(state)
    
    else:
        # Initialize DearPyGui context
        dpg.create_context()

        # Create and configure the main application window
        dpg.create_viewport(title='Product Configurator', width=820, height=680)

        # Define layouts here to ensures everything is registered before setup and rendering
        draw_main_layout(state)

        # Starts loop to check for state changes coming from the 3D process and update the UI
        poll_and_update_ui(None, None, state)

        # Prepares all layout for display
        dpg.setup_dearpygui()
        # Shows the main window to the user
        dpg.show_viewport()
        # Starts main loop and blocks adding widgets and any code execution until it ends. From this point on, you can only interact with the UI
        dpg.start_dearpygui()

        # Cleans up the context and all resources used by dearpygui
        dpg.destroy_context()