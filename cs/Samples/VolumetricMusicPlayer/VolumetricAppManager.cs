using Microsoft.MixedReality.Volumetric;
using Microsoft.UI.Xaml;
using System.Diagnostics;

namespace VolumetricAudioVisualization
{
    public class VolumetricAppManager
    {
        private VolumetricApp _volumetricApp;
        private VisualizationVolume _volume;
        public VisualizationVolume Volume
        {
            get { return _volume; }
        }

        public VolumetricAppManager()
        {
            _volumetricApp = new VolumetricApp(
                appName: "Viz",
                requiredExtensions: new string[]
                {
                    Extensions.VA_EXT_gltf2_model_resource,
                    Extensions.VA_EXT_mesh_edit,
                    Extensions.VA_EXT_locate_joints,
                    Extensions.VA_EXT_locate_spaces,
                    Extensions.VA_EXT_volume_container_modes,
                    Extensions.VA_EXT_material_resource,
                    Extensions.VA_EXT_texture_resource
                });
            _volumetricApp.OnStart += AppConnected;
            _volumetricApp.OnReconnect += AppConnected;
            _volumetricApp.OnDisconnect += AppDisconnected;
            _volumetricApp.OnStop += AppStopped;

            _volumetricApp.RunAsync();
        }

        ~VolumetricAppManager()
        {
            Debug.WriteLine("VolumetricAppManager finalized");
            if (_volumetricApp != null)
            {
                _volumetricApp.OnStart -= AppConnected;
                _volumetricApp.OnReconnect -= AppConnected;
                _volumetricApp.OnDisconnect -= AppDisconnected;
                _volumetricApp.OnStop -= AppStopped;

                foreach (var volume in _volumetricApp.Volumes)
                {
                    volume.RequestClose();
                }
                _volumetricApp.RequestExit();
            }
        }

        private void AppStopped(VolumetricApp app)
        {
            Debug.WriteLine("VolumetricAppManager.AppStopped()");
            Application.Current?.Exit();
        }

        private void AppConnected(VolumetricApp app)
        {
            Debug.WriteLine("VolumetricAppManager.AppConnected()");
            CreateVizVolume();
        }

        private void AppDisconnected(VolumetricApp app)
        {
            Debug.WriteLine("VolumetricAppManager.AppDisconnected()");
        }

        private bool CreateVizVolume()
        {
            if (_volumetricApp.IsConnected)
            {
                if (_volume == null && _volumetricApp.IsConnected)
                {
                    _volume = new VisualizationVolume(_volumetricApp);
                }
                return true;
            }
            return false;
        }
    }
}
