import random
from enum import Enum

class DeployButtonState(Enum):
    ENABLED = "enabled"
    DISABLED = "disabled"
    LOADING = "loading"

def random_color():
    return [random.randint(0, 255) for _ in range(4)]

def shuffle(state):
    # Randomize the product configuration state and marks the UI as dirty to trigger a refresh
    state["Headband"] = random_color()
    state["Speakers"] = random_color()
    state["Texture_idx"] = random.randint(0, 3)
    state["Active_accessories"] = [i for i in range(3) if random.choice([True, False])]
    state["ui_dirty"] = True

def update_deploy_button_state(state_value: DeployButtonState, sender=None, app_data=None, user_data=None):
    state = user_data
    state["deploy_btn"] = state_value
    state["ui_dirty"] = True