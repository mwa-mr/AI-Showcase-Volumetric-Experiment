
using Microsoft.MixedReality.Volumetric;
using System;
using System.Diagnostics;

namespace Volumetric.Samples.SpatialPad
{
    // The Volumetric Experience class controls the Volumetric part of the app.
    public class VolumetricExperience
    {
        // Create multiple volumes
        public SpatialPadVolume?[] PadVolumes = new SpatialPadVolume?[5];
        public DesignPage DesignPage;
        private VolumetricApp _app;

        public VolumetricExperience(string appName, DesignPage page)
        {
            DesignPage = page;

            // Create Volumetric App with the needed extensions
            _app = new VolumetricApp(appName,
                requiredExtensions: new string[] {
                    Extensions.VA_EXT_gltf2_model_resource,
                    Extensions.VA_EXT_material_resource,
                    Extensions.VA_EXT_mesh_edit,
                    Extensions.VA_EXT_locate_spaces,
                    Extensions.VA_EXT_locate_joints,
                    Extensions.VA_EXT_adaptive_card_element,
                    Extensions.VA_EXT_volume_container_modes,
                });
            _app.OnStart += __ =>
            {
                // Create a Volume once the Volumetric App has started
                CreateSpatialPad(App.GetCurrentKeypad());
            };
            // Run the Volumetric App asynchronously
            _app.RunAsync();
        }

        public void CreateSpatialPad(KeypadData keypadData)
        {
            if (PadVolumes[keypadData.Index] == null)
            {
                try
                {
                    // Create volume
                    PadVolumes[keypadData.Index] = new SpatialPadVolume(_app, keypadData, this);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception: " + ex.Message);
                }
            }
        }

        public bool IsVolumeOpen(int index)
        {
            if (index < 0 || index >= PadVolumes.Length)
                return false;

            var volume = PadVolumes[index];
            if (volume == null)
                return false;

            return !volume.IsClosed;
        }
    }
}
