using System;
using System.Diagnostics;
using Microsoft.MixedReality.Volumetric;

namespace Volumetric.Samples.ProductConfigurator
{
    // The Volumetric Experience class controls the Volumetric part of the app.
    public class VolumetricExperience
    {
        public HeadphonesVolume? Volume;
        public ConfigPage ConfigPage;
        private VolumetricApp _app;

        public VolumetricExperience(string appName, ConfigPage page)
        {
            ConfigPage = page;

            // Create Volumetric App with the needed extensions
            _app = new VolumetricApp(appName,
                requiredExtensions: new string[] {
                    Extensions.VA_EXT_gltf2_model_resource,
                    Extensions.VA_EXT_material_resource,
                    Extensions.VA_EXT_adaptive_card_element,
                    Extensions.VA_EXT_mesh_edit,
                });
            _app.OnStart += __ =>
            {
                // Create a Volume once the Volumetric App has started
                CreateVolume();
            };
            // Run the Volumetric App asynchronously
            _app.RunAsync();
        }

        public void CreateVolume()
        {
            if (Volume == null)
            {
                try
                {
                    Volume = new HeadphonesVolume(_app, this);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception: " + ex.Message);
                }
            }
        }
    }
}
