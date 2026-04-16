# utils/paths.py
import os


def get_project_root() -> str:
    """Returns the absolute path to the root of the project."""
    return os.path.dirname(os.path.dirname(os.path.abspath(__file__)))


def get_assets_path() -> str:
    """Returns the path to the assets directory."""
    return os.path.join(get_project_root(), 'assets')


def get_models_path() -> str:
    """Returns the path to the models directory."""
    return os.path.join(get_assets_path(), 'models')