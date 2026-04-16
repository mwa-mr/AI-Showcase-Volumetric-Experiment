using Microsoft.MixedReality.Volumetric;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.UI;

#region SharpGLTF
//using SharpGLTF.Geometry;
//using SharpGLTF.Geometry.VertexTypes;
//using SharpGLTF.Materials;
//using SharpGLTF.Schema2;
//using VERTEX_VP = SharpGLTF.Geometry.VertexTypes.VertexPosition;
//using VERTEX_VPN = SharpGLTF.Geometry.VertexTypes.VertexPositionNormal;
#endregion

public static class NativeMethods
{
    [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl)]
    public static extern void memcpy(IntPtr dest, IntPtr src, uint count);
}

namespace VolumetricAudioVisualization
{
    public class VisualizationVolume : Volume
    {
        private readonly VolumetricApp _volumetricApp;
        private readonly GradientCalculator _colorCalculator;

        private VisualElement? _modelVisualElement;
        private VisualElement? _vizVisualElement;
        private ModelResource? _modelResource;
        private MeshResource? _meshResource;
        private MaterialResource? _materialResource;
        private TextureResource? _textureResource;

        private int _columnCount = 10;
        private int _rowCount = 10;
        float _yScale = 1.0f; // Adjust this value to scale the height of the visualization
        float _colorScale = .01f; // Scale for color mapping
        private ConcurrentQueue<float[]> _dataCache;

        private Stopwatch _stopwatch = new Stopwatch();
        private uint[] _transformedIndices;
        private float[] _transformedVertices;
        private float[] _transformedNormals;
        private float[] _transformedColors;
        private float _lastUpdateTime = 0f;
        private float _aveFps = 0f;
        private bool _indexDirty = false;
        //private string _glbPath = Path.GetTempFileName() + ".glb";
        private string _glbPath = Path.Combine(AppContext.BaseDirectory, "scene.glb");

        public VisualizationVolume(VolumetricApp volumetricApp)
             : base(volumetricApp)
        {
            _volumetricApp = volumetricApp;

            //CreateGLB(_glbPath);

            var c = new[] { Color.FromArgb(255, 255, 255, 255), Color.FromArgb(255, 0, 0, 255), Color.FromArgb(255, 0, 255, 255), Color.FromArgb(255, 0, 255, 150), Color.FromArgb(255, 0, 255, 0), Color.FromArgb(255, 155, 255, 0), Color.FromArgb(255, 255, 50, 0) };
            _colorCalculator = new GradientCalculator(c);

            OnReady += HandleOnReady;
            OnUpdate += (_) => HandleOnUpdate();
            //Close += (_) => app.RequestExit();

            _dataCache = new ConcurrentQueue<float[]>();

            lock (_dataCache)
            {
                for (var i = 0; i < _rowCount + 1; i++)
                {
                    _dataCache.Enqueue(new float[_columnCount + 1]);
                }
            }
            _stopwatch.Start();
        }

        internal string GetStats()
        {
            var stats = $"FPS: {_aveFps:0.0}\n" +
                        $"Bands: {_columnCount}\n" +
                        $"History: {_rowCount}\n" +
                        $"Verts: {_rowCount * _columnCount * 4}\n" +
                        $"Triangles: {_rowCount * _columnCount * 2}";

            return stats;
        }

        internal void UpdateTexture(string textureName, string texturePath)
        {
            if (!IsRunning || _materialResource?.IsReady == false || _textureResource?.IsReady == false)
            {
                return;
            }
            if (!File.Exists(texturePath))
            {
                Trace.TraceError($"Texture file not found: {texturePath}");
                return;
            }

            _textureResource.SetImageUri(new Uri(texturePath).AbsolutePath);
            _materialResource.SetPbrBaseColorTexture(_textureResource);
        }

        internal void SetData(double[] data, int columnCount, int rowCount)
        {
            if (data == null)
            {
                return;
            }
            lock (_dataCache)
            {
                if (columnCount != _columnCount)
                    _dataCache.Clear();

                _columnCount = columnCount;
                _rowCount = rowCount;

                while (_dataCache.Count < _rowCount + 1)
                {
                    _dataCache.Enqueue(new float[_columnCount + 1]);
                }

                var step = (data.Length) / (_columnCount + 1);

                var newData = new float[data.Length / step];
                for (var i = 0; i < newData.Length; i++)
                {
                    double sum = 0;
                    for (var s = 0; s < step; s++)
                    {
                        sum += (float)data[i * step + s];
                    }
                    newData[i] = (float)(sum / step);
                }

                _dataCache.Enqueue(newData);
                while (_dataCache.Count > _rowCount + 1)
                    _dataCache.TryDequeue(out _);
            }
        }

        /// <summary>
        /// Simple test pattern.
        /// </summary>
        public static void FillWaveData(List<float[]> dataCache, int width, int height, float timeOffset)
        {
            float waveSpeed = 0.05f; // Controls animation speed
            float frequency = 0.02f; // Adjust wave density
            float amplitude = 75.0f; // Peak wave height
            float baseHeight = 75.0f; // Keeps values in [0, 150]

            // Ensure list is initialized
            if (dataCache.Count == 0)
            {
                for (int x = 0; x < width; x++)
                    dataCache.Add(new float[height]);
            }

            Parallel.For(0, width, x =>
            {
                for (int y = 0; y < height; y++)
                {
                    float waveValue = amplitude * MathF.Sin(x * frequency + timeOffset * waveSpeed) * MathF.Cos(y * frequency + timeOffset * waveSpeed);
                    dataCache[x][y] = Math.Max(0, Math.Min(150, baseHeight + waveValue));
                }
            });
        }

        private void HandleOnReady(Volume volume)
        {
            Container.SetDisplayName("Volumetric Music");
            Content.SetSizeBehavior(VaVolumeSizeBehavior.Fixed);
            Content.SetSize(new VaExtent3Df() { width = 1.01f, height = 1.01f, depth = 2.01f });
            Content.SetPosition(new VaVector3f() { x = 0, y = -.5f, z = 0 });
            var uri = new Uri(_glbPath, UriKind.Absolute);

            if (_modelResource == null)
                _modelResource = new ModelResource(volume, uri.AbsoluteUri);
            else
                _modelResource.SetModelUri(uri.AbsoluteUri);

            if (_modelVisualElement == null)
                _modelVisualElement = new VisualElement(volume, _modelResource);

            if (_vizVisualElement == null)
                _vizVisualElement = new VisualElement(volume, _modelVisualElement, "vizMesh");

            if (_materialResource == null)
                _materialResource = new MaterialResource(_modelResource, "backMaterial");

            if (_textureResource == null)
                _textureResource = new TextureResource(volume);

            if (_meshResource == null)
            {
                var bufferDescriptors = new VaMeshBufferDescriptorExt[]
                {
                    new VaMeshBufferDescriptorExt { bufferType = VaMeshBufferTypeExt.Index, bufferFormat = VaMeshBufferFormatExt.Uint32 },
                    new VaMeshBufferDescriptorExt { bufferType = VaMeshBufferTypeExt.VertexPosition, bufferFormat = VaMeshBufferFormatExt.Float3 },
                    new VaMeshBufferDescriptorExt { bufferType = VaMeshBufferTypeExt.VertexNormal, bufferFormat = VaMeshBufferFormatExt.Float3 },
                    new VaMeshBufferDescriptorExt { bufferType = VaMeshBufferTypeExt.VertexColor, bufferFormat = VaMeshBufferFormatExt.Float4 }
                };
                _meshResource = new MeshResource(_modelResource, 0, 0, bufferDescriptors, false /*decoupleAccessors*/, false /*initializeData*/);
            }

            volume.RequestUpdate(VaVolumeUpdateMode.FullFramerate);
        }

        private void HandleOnUpdate()
        {
            //FillWaveData(_dataCache, _rowCount, _columnCount, _stopwatch.ElapsedMilliseconds * .1f);

            var indexCount = _rowCount * _columnCount * 2 * 3;  // 2 triangles per quad, 3 indices per triangle
            var vertexCount = _rowCount * _columnCount * 4;     // 2 triangles per quad, 4 vertices per quad

            if (_transformedVertices?.Length != vertexCount * 3)
            {
                _transformedVertices = new float[vertexCount * 3];
                _transformedNormals = new float[vertexCount * 3];
                _transformedColors = new float[vertexCount * 4];
                _transformedIndices = new uint[indexCount];

                //HandleOnReady(this);
                _indexDirty = true;
                return;
            }
            if (_meshResource?.IsReady == true)
            {
                float[][] data;
                int rowCount, columnCount;
                lock (_dataCache)
                {
                    data = _dataCache.ToArray();
                    rowCount = _rowCount;
                    columnCount = _columnCount;
                }

                // Parallelize the quad processing
                int quadCount = rowCount * columnCount;
                Parallel.For(0, quadCount, q =>
                {
                    int z = q / columnCount;
                    int x = q % columnCount;

                    int b = q * 4;
                    int vertPos = b * 3;
                    int colorPos = b * 4;
                    int indexPos = q * 6;
                    if (_indexDirty)
                    {
                        _transformedIndices[indexPos++] = (uint)(b + 0);
                        _transformedIndices[indexPos++] = (uint)(b + 1);
                        _transformedIndices[indexPos++] = (uint)(b + 2);

                        _transformedIndices[indexPos++] = (uint)(b + 0);
                        _transformedIndices[indexPos++] = (uint)(b + 2);
                        _transformedIndices[indexPos++] = (uint)(b + 3);
                    }

                    var newPosY0 = data[rowCount - z][x];
                    var newPosY1 = data[rowCount - z - 1][x];
                    var newPosY2 = data[rowCount - z - 1][x + 1];
                    var newPosY3 = data[rowCount - z][x + 1];

                    var newVert0 = new Vector3(x, newPosY0 * _yScale, z);
                    var newVert1 = new Vector3(x, newPosY1 * _yScale, z + 1);
                    var newVert2 = new Vector3(x + 1, newPosY2 * _yScale, z + 1);
                    var newVert3 = new Vector3(x + 1, newPosY3 * _yScale, z);

                    var normal0 = ComputeNormal(newVert0, newVert1, newVert2);
                    var normal1 = ComputeNormal(newVert0, newVert2, newVert3);
                    var blendNormal = Vector3.Normalize(normal0 + normal1);

                    // Fit the vertices to the volume size.  Y is scaled to a known good height, X and Z are scaled to fit the width and depth of the volume.
                    float xScale = 0.93f / _columnCount;
                    float yScale = 1.0f / 500;
                    float zScale = 1.0f / _rowCount * 2;

                    newVert0.X *= xScale; newVert1.X *= xScale; newVert2.X *= xScale; newVert3.X *= xScale;
                    newVert0.Y *= yScale; newVert1.Y *= yScale; newVert2.Y *= yScale; newVert3.Y *= yScale;
                    newVert0.Z *= zScale; newVert1.Z *= zScale; newVert2.Z *= zScale; newVert3.Z *= zScale;

                    newVert0.CopyTo(_transformedVertices, vertPos);
                    blendNormal.CopyTo(_transformedNormals, vertPos);
                    vertPos += 3;
                    newVert1.CopyTo(_transformedVertices, vertPos);
                    normal0.CopyTo(_transformedNormals, vertPos);
                    vertPos += 3;
                    newVert2.CopyTo(_transformedVertices, vertPos);
                    blendNormal.CopyTo(_transformedNormals, vertPos);
                    vertPos += 3;
                    newVert3.CopyTo(_transformedVertices, vertPos);
                    normal1.CopyTo(_transformedNormals, vertPos);
                    vertPos += 3;

                    var c = _colorCalculator.GetColor((float)(newPosY0) * _colorScale);
                    var color = new Vector4(c.R / 255.0f, c.G / 255.0f, c.B / 255.0f, c.A / 255.0f);
                    color.CopyTo(_transformedColors, colorPos);
                    colorPos += 4;

                    c = _colorCalculator.GetColor((float)(newPosY1) * _colorScale);
                    color = new Vector4(c.R / 255.0f, c.G / 255.0f, c.B / 255.0f, c.A / 255.0f);
                    color.CopyTo(_transformedColors, colorPos);
                    colorPos += 4;

                    c = _colorCalculator.GetColor((float)(newPosY2) * _colorScale);
                    color = new Vector4(c.R / 255.0f, c.G / 255.0f, c.B / 255.0f, c.A / 255.0f);
                    color.CopyTo(_transformedColors, colorPos);
                    colorPos += 4;

                    c = _colorCalculator.GetColor((float)(newPosY3) * _colorScale);
                    color = new Vector4(c.R / 255.0f, c.G / 255.0f, c.B / 255.0f, c.A / 255.0f);
                    color.CopyTo(_transformedColors, colorPos);
                    colorPos += 4;
                });

                IReadOnlyList<VaMeshBufferTypeExt> buffers;
                if (_indexDirty) buffers = [VaMeshBufferTypeExt.VertexPosition, VaMeshBufferTypeExt.VertexNormal, VaMeshBufferTypeExt.VertexColor, VaMeshBufferTypeExt.Index];
                else buffers = [VaMeshBufferTypeExt.VertexPosition, VaMeshBufferTypeExt.VertexNormal, VaMeshBufferTypeExt.VertexColor];

                _meshResource.WriteMeshBuffers(buffers, (uint)indexCount, (uint)vertexCount, (IReadOnlyList<MeshBufferData> meshBuffers) =>
                {
                    unsafe
                    {
                        fixed (float* sourcePtr = _transformedVertices)
                        {
                            NativeMethods.memcpy(meshBuffers[0].Buffer, (IntPtr)sourcePtr, (uint)(_transformedVertices.Length * sizeof(float)));
                        }
                        fixed (float* sourcePtr = _transformedNormals)
                        {
                            NativeMethods.memcpy(meshBuffers[1].Buffer, (IntPtr)sourcePtr, (uint)(_transformedNormals.Length * sizeof(float)));
                        }
                        fixed (float* sourcePtr = _transformedColors)
                        {
                            NativeMethods.memcpy(meshBuffers[2].Buffer, (IntPtr)sourcePtr, (uint)(_transformedColors.Length * sizeof(float)));
                        }
                        if (_indexDirty)
                        {
                            fixed (uint* sourcePtr = _transformedIndices)
                            {
                                NativeMethods.memcpy(meshBuffers[3].Buffer, (IntPtr)sourcePtr, (uint)(_transformedIndices.Length * sizeof(uint)));
                            }
                            _indexDirty = false;
                        }
                    }

                    //Marshal.Copy(_transformedVertices, 0, meshBuffers[0].Buffer, _transformedVertices.Length);
                    //Marshal.Copy(_transformedNormals, 0, meshBuffers[1].Buffer, _transformedNormals.Length);
                    //Marshal.Copy(_transformedColors, 0, meshBuffers[2].Buffer, _transformedColors.Length);
                    //if (_indexDirty)
                    //    Marshal.Copy(_transformedIndices, 0, meshBuffers[3].Buffer, _transformedIndices.Length);

                    float elapsedTime = (float)_stopwatch.Elapsed.TotalMilliseconds - _lastUpdateTime;
                    _lastUpdateTime = (float)_stopwatch.Elapsed.TotalMilliseconds;
                    var fps = 1000f / elapsedTime;

                    _aveFps = Lerp(_aveFps, fps, 0.05f);

                    //Trace.TraceInformation($"Volume FPS: {_aveFps:0.0}  ({fps:0.0})  {elapsedTime:0.0} ms  vertexCount: {vertexCount}");
                });
            }
            else
            {
                Trace.TraceError("ModelResource is not ready, skipping update.");
            }
        }

        static float Lerp(float firstFloat, float secondFloat, float by)
        {
            return firstFloat * (1 - by) + secondFloat * by;
        }

        public static Vector3 ComputeNormal(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            var normal = Vector3.Cross(p2 - p1, p3 - p1);
            return Vector3.Normalize(normal);
        }

        #region SharpGLTF Code For creating original GLB

        //internal void CreateGLB(string filename)
        //{
        //    var model = ModelRoot.CreateModel();
        //    var scene = model.UseScene("default");

        //    var vizroot = scene.CreateNode("vizroot");

        //    var vizMaterial = new MaterialBuilder("vizMaterial")
        //        .WithMetallicRoughness(.15f, .3f)
        //        .WithDoubleSide(true)
        //        .WithChannelParam(KnownChannel.BaseColor, KnownProperty.RGBA, Vector4.One);

        //    var vizMesh = model.CreateMesh(CreatePlane("vizMesh", _columnCount, _rowCount, vizMaterial, new Vector3(.93f / _columnCount, 1.0f / 500, -1.0f / _rowCount * 2)));
        //    vizroot.CreateNode("vizMesh").WithMesh(vizMesh)
        //        // Bake the scale into the vertices instead of setting the scale on the VisualElement
        //        //.WithLocalScale(new Vector3(.93f / _columnCount, 1.0f / 500, -1.0f / _rowCount * 2))
        //        .WithLocalScale(new Vector3(1, 1, 1))
        //        .WithLocalTranslation(new Vector3(-.465f, .0475f, -1));

        //    var backMaterial = new MaterialBuilder("backMaterial")
        //        .WithMetallicRoughness(0, .5f)
        //        .WithDoubleSide(true)
        //        .WithAlpha(SharpGLTF.Materials.AlphaMode.BLEND)
        //        .WithChannelParam(KnownChannel.BaseColor, KnownProperty.RGBA, Vector4.One)
        //        .WithChannelImage(KnownChannel.BaseColor, @"back.png");

        //    var backMesh = model.CreateMesh(CreatePlaneWithUvs("backMesh", 1, 1, backMaterial));
        //    vizroot.CreateNode("backMesh").WithMesh(backMesh)
        //        .WithLocalScale(new Vector3(1, 1, .75f))
        //        .WithLocalRotation(Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)Math.PI / 2))
        //        .WithLocalTranslation(new Vector3(-.5f, .75f, -1));

        //    model.SaveGLB(filename);
        //}

        //internal MeshBuilder<VERTEX_VPN, VertexColor1> CreatePlane(string name, int xCount, int zCount, MaterialBuilder material, Vector3 bakedScale)
        //{
        //    var mesh = new MeshBuilder<VERTEX_VPN, VertexColor1>($"{name}");

        //    var prim = mesh.UsePrimitive(material);

        //    VERTEX_VPN[,] planeVertices = new VERTEX_VPN[(xCount + 1), (zCount + 1)];
        //    for (int z = 0; z < zCount + 1; z++)
        //    {
        //        for (int x = 0; x < xCount + 1; x++)
        //        {
        //            planeVertices[x, z] = new VERTEX_VPN(x * bakedScale.X, 0, z * bakedScale.Z, 0, 1, 0);
        //        }
        //    }
        //    Random r = new Random();

        //    for (int z = 0; z < zCount; z++)
        //    {
        //        for (int x = 0; x < xCount; x++)
        //        {
        //            var indices = prim.AddQuadrangle(
        //                (planeVertices[x, z], new VertexColor1(Vector4.One - .01f * new Vector4((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble(), 1))),
        //                (planeVertices[x, z + 1], new VertexColor1(Vector4.One - .01f * new Vector4((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble(), 1))),
        //                (planeVertices[x + 1, z + 1], new VertexColor1(Vector4.One - .01f * new Vector4((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble(), 1))),
        //                (planeVertices[x + 1, z], new VertexColor1(Vector4.One - .01f * new Vector4((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble(), 1)))
        //                );
        //        }
        //    }

        //    return mesh;
        //}

        //internal MeshBuilder<VERTEX_VPN, VertexColor1Texture1> CreatePlaneWithUvs(string name, int xCount, int zCount, MaterialBuilder material)
        //{
        //    var mesh = new MeshBuilder<VERTEX_VPN, VertexColor1Texture1>($"{name}");
        //    var prim = mesh.UsePrimitive(material);
        //    VERTEX_VPN[,] planeVertices = new VERTEX_VPN[(xCount + 1), (zCount + 1)];
        //    VertexColor1Texture1[,] planeUVs = new VertexColor1Texture1[(xCount + 1), (zCount + 1)];
        //    Random r = new Random();
        //    for (int z = 0; z < zCount + 1; z++)
        //    {
        //        for (int x = 0; x < xCount + 1; x++)
        //        {
        //            planeVertices[x, z] = new VERTEX_VPN(x, 0, z, 0, 1, 0);
        //            planeUVs[x, z] = new VertexColor1Texture1(Vector4.One, new Vector2((float)x / zCount, (float)z / xCount));
        //        }
        //    }
        //    for (int z = 0; z < zCount; z++)
        //    {
        //        for (int x = 0; x < xCount; x++)
        //        {
        //            var indices = prim.AddQuadrangle(
        //                (planeVertices[x, z], planeUVs[x, z]),
        //                (planeVertices[x, z + 1], planeUVs[x, z + 1]),
        //                (planeVertices[x + 1, z + 1], planeUVs[x + 1, z + 1]),
        //                (planeVertices[x + 1, z], planeUVs[x + 1, z])
        //                );
        //        }
        //    }
        //    return mesh;
        //}

        //internal MeshBuilder<VERTEX_VP, VertexColor1> CreateCube(string name)
        //{
        //    var white = new Vector4(1, 1, 1, 1);
        //    var green = new Vector4(.2f, 1, .2f, 1);
        //    var material = new MaterialBuilder()
        //        .WithMetallicRoughness(0, .5f)
        //        //.WithDoubleSide(true)
        //        .WithChannelParam(KnownChannel.BaseColor, KnownProperty.RGBA, white);

        //    var mesh = new MeshBuilder<VERTEX_VP, VertexColor1>($"{name}");
        //    var prim = mesh.UsePrimitive(material);
        //    VERTEX_VP v0 = new VERTEX_VP(-.5f, .5f, .5f);
        //    VERTEX_VP v1 = new VERTEX_VP(-.5f, -.5f, .5f);
        //    VERTEX_VP v2 = new VERTEX_VP(.5f, -.5f, .5f);
        //    VERTEX_VP v3 = new VERTEX_VP(.5f, .5f, .5f);
        //    VERTEX_VP v4 = new VERTEX_VP(.5f, .5f, -.5f);
        //    VERTEX_VP v5 = new VERTEX_VP(.5f, -.5f, -.5f);
        //    VERTEX_VP v6 = new VERTEX_VP(-.5f, -.5f, -.5f);
        //    VERTEX_VP v7 = new VERTEX_VP(-.5f, .5f, -.5f);


        //    //front
        //    prim.AddQuadrangle(
        //            (new VERTEX_VP(v0), new VertexColor1(white)),
        //            (new VERTEX_VP(v1), new VertexColor1(white)),
        //            (new VERTEX_VP(v2), new VertexColor1(white)),
        //            (new VERTEX_VP(v3), new VertexColor1(white)));

        //    //back
        //    prim.AddQuadrangle(
        //            (new VERTEX_VP(v4), new VertexColor1(white)),
        //            (new VERTEX_VP(v5), new VertexColor1(white)),
        //            (new VERTEX_VP(v6), new VertexColor1(white)),
        //            (new VERTEX_VP(v7), new VertexColor1(white)));

        //    //top
        //    prim.AddQuadrangle(
        //            (new VERTEX_VP(v3), new VertexColor1(white)),
        //            (new VERTEX_VP(v4), new VertexColor1(white)),
        //            (new VERTEX_VP(v7), new VertexColor1(white)),
        //            (new VERTEX_VP(v0), new VertexColor1(white)));

        //    //bottom
        //    prim.AddQuadrangle(
        //            (new VERTEX_VP(v1), new VertexColor1(white)),
        //            (new VERTEX_VP(v6), new VertexColor1(white)),
        //            (new VERTEX_VP(v5), new VertexColor1(white)),
        //            (new VERTEX_VP(v2), new VertexColor1(white)));

        //    //left
        //    prim.AddQuadrangle(
        //            (new VERTEX_VP(v7), new VertexColor1(white)),
        //            (new VERTEX_VP(v6), new VertexColor1(white)),
        //            (new VERTEX_VP(v1), new VertexColor1(white)),
        //            (new VERTEX_VP(v0), new VertexColor1(white)));

        //    //right
        //    prim.AddQuadrangle(
        //            (new VERTEX_VP(v3), new VertexColor1(white)),
        //            (new VERTEX_VP(v2), new VertexColor1(white)),
        //            (new VERTEX_VP(v5), new VertexColor1(white)),
        //            (new VERTEX_VP(v4), new VertexColor1(white)));


        //    return mesh;
        //}

        #endregion
    }
}
