import multiprocessing
from logic.volumetric_experience import run_volumetric_experience

# Global reference to the volumetric process to prevent duplicate launches
volumetric_process = None

def launch_3d_process(state):
    # Launches the 3D volumetric experience in a separate process
    # Prevents launching multiple instances by checking if the process is already running
    global volumetric_process
    if volumetric_process is not None and volumetric_process.is_alive():
        print("Volumetric experience is already running.")
        return
    # Only launch from the main process
    if multiprocessing.current_process().name == "MainProcess":
        volumetric_process = multiprocessing.Process(
            target=run_volumetric_experience,
            args=(state,),
            name="VolumetricExperience"
        )
        volumetric_process.start()