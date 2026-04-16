using Microsoft.MixedReality.Volumetric;
using System.Runtime.InteropServices;

namespace CsSpinningCube;

public class SpinningVolume : Volume
{
    private VisualElement? _visualElement;
    private ModelResource? _modelResource;
    private MeshResource? _meshResource;

    public SpinningVolume(VolumetricApp app) : base(app)
    {
        OnReady += HandleOnReady;
        OnUpdate += (_) => HandleOnUpdate();
        OnClose += (_) => app.RequestExit();
    }

    private void HandleOnReady(Volume volume)
    {
        var uri = VolumetricApp.GetAssetUri("BoxTextured.glb");
        _modelResource = new ModelResource(volume, uri);
        _visualElement = new VisualElement(volume, _modelResource);

        var bufferDescriptors = new VaMeshBufferDescriptorExt[]
        {
            new VaMeshBufferDescriptorExt { bufferType = VaMeshBufferTypeExt.Index,  bufferFormat = VaMeshBufferFormatExt.Uint32 },
            new VaMeshBufferDescriptorExt { bufferType = VaMeshBufferTypeExt.VertexPosition, bufferFormat = VaMeshBufferFormatExt.Float3 },
            new VaMeshBufferDescriptorExt { bufferType = VaMeshBufferTypeExt.VertexNormal, bufferFormat = VaMeshBufferFormatExt.Float3 },
        };
        _meshResource = new MeshResource(_modelResource, 0, 0, bufferDescriptors, false /*decoupleAccessors*/, true /*initializeData*/);

        // Request a repeated OnUpdate event for rotating the model.
        volume.RequestUpdate(VaVolumeUpdateMode.FullFramerate);
    }

    private void HandleOnUpdate()
    {
        if (_modelResource?.IsReady == true)
        {
            float angle = (Environment.TickCount64 / 1000.0f) * MathF.PI / 2;
            VaQuaternionf quaternion = new VaQuaternionf() { x = 0, y = MathF.Sin(angle / 2), z = 0, w = MathF.Cos(angle / 2) };
            _visualElement!.SetOrientation(in quaternion);

            float seconds = (float)FrameState.frameTime * 1e-9f;
            _visualElement!.SetScale((MathF.Cos(seconds) + 3) / 4);

            _meshResource!.WriteMeshBuffers([VaMeshBufferTypeExt.VertexPosition], (IReadOnlyList<MeshBufferData> meshBuffers) =>
            {
                var t = -3 * DateTime.Now.Subtract(DateTime.MinValue).TotalSeconds;
                var ct = Math.Cos(t);
                var st = Math.Sin(t);
                var r = 0.5f; // 1m cube, so the radius is 0.5

                // Modify the top 12 vertices while keeping the bottom 24 vertices intact.
#pragma warning disable format
                // x' = x cos(t) - y sin(t)
                // y' = y cos(t) + x sin(t)
                float[] vertices =
                   [(float)(-r * ct - -r * st), (float)(-r * ct + -r * st), r,      // v0 -> (-r, -r)
                    (float)( r * ct - -r * st), (float)(-r * ct +  r * st), r,      // v1 -> ( r, -r)
                    (float)(-r * ct -  r * st), (float)( r * ct + -r * st), r,      // v2 -> (-r,  r)
                    (float)( r * ct -  r * st), (float)( r * ct +  r * st), r];     // v3 -> ( r,  r)
#pragma warning restore format
                Marshal.Copy(vertices, 0, meshBuffers[0].Buffer, vertices.Length);
            });
        }
    }
}

internal sealed class Program
{
    static int Main()
    {
        var app = new VolumetricApp("SpinningVolumetricApp",
            requiredExtensions: new[] {
                Extensions.VA_EXT_gltf2_model_resource,
                Extensions.VA_EXT_mesh_edit,
            });
        return app.Run(onStart: app => _ = new SpinningVolume(app));
    }
}
