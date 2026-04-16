using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.MixedReality.Volumetric;
using SharpGLTF.Schema2;

namespace Volumetric.Samples.ProductConfigurator
{
    public class HeadphonesVolume : Volume
    {
        private VolumetricExperience _volumetricExperience;

        // Headphones model properties
        private string _modelUri = "Assets/Models/Headphones.glb";
        private ModelResource? _model;
        private VisualElement? _visual;
        private List<VisualElement?> _earcups = new List<VisualElement?>();
        private MaterialResource? _headbandMaterial, _speakersMaterial;
        private Windows.UI.Color _lastheadbandColor, _lastSpeakersColor;

        // Accessories properties
        private string _wingsUri = "Assets/Models/Wings.glb";
        private string _wingsMorphUri = "Assets/Models/WingsMorph.glb";
        private ModelResource? _wingsModel;
        private VisualElement? _wingsVisual;
        private string _earsUri = "Assets/Models/Ears.glb";
        private ModelResource? _earsModel;
        private VisualElement? _earsVisual;
        private string _cromoUri = "Assets/Models/Cromo.glb";
        private ModelResource? _cromoModel;
        private VisualElement? _cromoVisual;
        private List<VisualElement?> _accessories = new List<VisualElement?>();

        // Mesh properties
        public MeshResource? _wingsMesh;
        bool _wingsActive = false;
        private List<Vector3> _meshVertexPositions;
        private List<Vector3> _meshVertexNormals;
        private List<Vector4> _meshVertexTangents;
        private List<Vector3> _morphVertexPositions;
        private List<Vector3> _morphVertexNormals;
        private List<Vector4> _morphVertexTangents;
        private int _vertexCount;
        private float[] _blendedVertices;
        private float[] _blendedNormals;
        private float[] _blendedTangents;
        private bool _wingsHasMorph
        {
            get
            {
                return
                    _blendedVertices != null &&
                    _blendedNormals != null &&
                    _blendedTangents != null;
            }
        }

        // Adaptive Card template
        private AdaptiveCard? _adaptiveCard;
        private readonly string _adaptiveCardTemplate = """
        {
            "type": "AdaptiveCard",
            "body": [],
            "actions": [
                {
                    "type": "Action.Execute",
                    "iconUrl": "${icon}",
                    "verb": "shuffle"
                }
            ],
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "version": "1.4"
        }
        """;

        private bool _modelInstantiated;

        // Manage volume events
        public HeadphonesVolume(VolumetricApp app, VolumetricExperience volumetricExperience) : base(app)
        {
            _volumetricExperience = volumetricExperience;
            OnReady += onReady;
            OnUpdate += (_) => onUpdate();
            OnClose += (_) => onClose();
        }

        // Perform actions when volume is ready
        private void onReady(Volume volume)
        {
            // Set Volume display name
            Container.SetDisplayName(this.App.AppName);

            // Set Volume properties and behaviour
            Content.SetSizeBehavior(VaVolumeSizeBehavior.Fixed);
            VaExtent3Df _volumeSize = new VaExtent3Df();
            _volumeSize.width = _volumeSize.height = _volumeSize.depth = 3.4f;
            Content.SetSize(_volumeSize);

            // Load headphones GLB model
            _model = new ModelResource(volume, VolumetricApp.GetAssetUri(_modelUri));
            _model.OnAsyncStateChanged += OnAsyncStateChanged;
            // Create Visual Element with an associated model
            _visual = new VisualElement(volume, _model);

            // Reference model materials for later editing
            _headbandMaterial = new MaterialResource(_model, "Mat_HeadBand");
            _speakersMaterial = new MaterialResource(_model, "Mat_speakers");

            // Load headphones accessories (ears, cromo, wings)
            _wingsModel = new ModelResource(volume, VolumetricApp.GetAssetUri(_wingsUri));
            _wingsModel.OnAsyncStateChanged += OnAsyncStateChanged;
            _wingsVisual = new VisualElement(volume, _wingsModel);
            _accessories.Add(_wingsVisual);

            _earsModel = new ModelResource(volume, VolumetricApp.GetAssetUri(_earsUri));
            _earsModel.OnAsyncStateChanged += OnAsyncStateChanged;
            _earsVisual = new VisualElement(volume, _earsModel);
            _accessories.Add(_earsVisual);

            _cromoModel = new ModelResource(volume, VolumetricApp.GetAssetUri(_cromoUri));
            _cromoModel.OnAsyncStateChanged += OnAsyncStateChanged;
            _cromoVisual = new VisualElement(volume, _cromoModel);
            _accessories.Add(_cromoVisual);

            // Control model sub nodes by creating new Visual Elements
            for (int i = 1; i <= 4; i++)
            {
                _earcups.Add(new VisualElement(volume, _visual, $"EarCupsv{i}"));
                if (i != 1) _earcups[i - 1].SetVisible(false);
            }

            // Hide visuals until GLB models are loaded and ready
            _visual.SetVisible(false);
            foreach (var item in _accessories)
            {
                item.SetVisible(false);
            }

            // Create Adaptive Card
            _adaptiveCard = new AdaptiveCard(volume, _adaptiveCardTemplate, FormatAdaptiveCardData());
            // Add listener for Adaptative Card actions
            _adaptiveCard.ActionInvoked += OnAdaptiveCardActionInvoked;

            // Fire and forget. Don't do long running tasks during onReady or onUpdate.
            // This method will schedule regular updates when it has completed setting up the volume's elements and resources.
            InitializeMeshBuffers();
        }

        // Listen to models state changes
        private void OnAsyncStateChanged(VaElementAsyncState newState)
        {
            switch (newState)
            {
                case VaElementAsyncState.Pending:
                    // Debug.WriteLine("Loading model...");
                    break;
                case VaElementAsyncState.Ready:
                    // Debug.WriteLine("Model loaded");
                    break;
                case VaElementAsyncState.Error:
                    Debug.WriteLine("Error loading model");
                    break;
                default:
                    break;
            }
        }

        // Perform actions when Volume is updated
        private void onUpdate()
        {
            // Perform actions just one time after the model is ready
            if (!_modelInstantiated)
            {
                if (_model?.IsReady == true && _visual.IsReady == true &&
                    _wingsMesh != null && _wingsMesh.IsReady &&
                    _accessories[0].IsReady == true && _accessories[1].IsReady == true && _accessories[2].IsReady == true)
                {
                    _modelInstantiated = true;

                    // Initialize headphones. Set default state.
                    ChangeHeadbandColor(Data.HeadbandSelectedColor);
                    ChangeSpeakersColor(Data.SpeakersSelectedColor);
                    if (Data.SelectedTexture != null) SetActiveEarcup(Data.SelectedTexture.Index);
                    SetActiveAccesory(0, Data.SelectedAccesory != null);
                    SetActiveAccesory(1, Data.SelectedAccesory2 != null);
                    SetActiveAccesory(2, Data.SelectedAccesory3 != null);

                    _visual.SetVisible(true);
                    _volumetricExperience.ConfigPage.DeployButtonState("disabled");
                }
            }
            // Edit Mesh Buffers each loop
            wingsAnimation();
        }

        // Perform actions when Volume is closed
        private void onClose()
        {
            _volumetricExperience.ConfigPage.DeployButtonState("enabled");
            _volumetricExperience.Volume = null;
        }

        // Format Adaptive Card
        private string FormatAdaptiveCardData()
        {
            string iconUrl = VolumetricApp.GetAssetUri("Assets/Images/Shuffle.png");
            return $$"""
            {
                "icon": "{{iconUrl}}",
            }
            """;
        }

        // Respond to Adaptive Card actions
        private void OnAdaptiveCardActionInvoked(object? sender, AdaptiveCard.ActionEventArgs args)
        {
            if (args.Verb == "shuffle")
            {
                ShuffleProperties();
            }
        }

        private VaColor4f sRGBToLinear(VaColor4f colorInSrgb)
        {
            static float ConvertChannel(float channel)
            {
                float clamped = Math.Clamp(channel, 0f, 1f);
                return clamped <= 0.04045f
                    ? clamped / 12.92f
                    : (float)Math.Pow((clamped + 0.055f) / 1.055f, 2.4f);
            }

            return new VaColor4f
            {
                r = ConvertChannel(colorInSrgb.r),
                g = ConvertChannel(colorInSrgb.g),
                b = ConvertChannel(colorInSrgb.b),
                a = colorInSrgb.a
            };
        }

        // Edit headphones materials
        public void ChangeHeadbandColor(Windows.UI.Color color)
        {
            if (color == _lastheadbandColor)
            {
                return;
            }

            VaColor4f col = new VaColor4f();
            col.r = color.R / 255f;
            col.g = color.G / 255f;
            col.b = color.B / 255f;

            _headbandMaterial.SetBaseColorFactor(sRGBToLinear(col));
            _lastheadbandColor = color;
        }

        public void ChangeSpeakersColor(Windows.UI.Color color)
        {
            if (color == _lastSpeakersColor)
            {
                return;
            }

            VaColor4f col = new VaColor4f();
            col.r = color.R / 255f;
            col.g = color.G / 255f;
            col.b = color.B / 255f;

            _speakersMaterial.SetBaseColorFactor(sRGBToLinear(col));
            _lastSpeakersColor = color;
        }

        // Control visibility of GLB models
        public void SetActiveAccesory(int indexAccesory, bool state)
        {
            if (indexAccesory > _accessories.Count - 1)
            {
                return;
            }

            _accessories[indexAccesory].SetVisible(state);

            if (indexAccesory == 0)
            {
                _wingsActive = state;
            }
        }

        // Control visibility of model subparts
        public void SetActiveEarcup(int earcupIndex)
        {
            if (earcupIndex > _earcups.Count - 1)
            {
                return;
            }

            foreach (var earcup in _earcups)
            {
                earcup.SetVisible(false);
            }

            _earcups[earcupIndex].SetVisible(true);
        }

        // Randomizer for headphones colors and accessories.
        public void ShuffleProperties()
        {
            _volumetricExperience.ConfigPage.ShuffleHeadphonesOptions();
        }

        private async void InitializeMeshBuffers()
        {
            // Create mesh buffers for mesh editing
            await GetMorphInfo(_wingsMorphUri);

            DispatchToNextUpdate(() =>
            {
                var bufferDescriptors = new VaMeshBufferDescriptorExt[]
                {
                    new VaMeshBufferDescriptorExt { bufferType = VaMeshBufferTypeExt.Index,  bufferFormat = VaMeshBufferFormatExt.Uint32 },
                    new VaMeshBufferDescriptorExt { bufferType = VaMeshBufferTypeExt.VertexPosition, bufferFormat = VaMeshBufferFormatExt.Float3 },
                    new VaMeshBufferDescriptorExt { bufferType = VaMeshBufferTypeExt.VertexNormal, bufferFormat = VaMeshBufferFormatExt.Float3 },
                    new VaMeshBufferDescriptorExt { bufferType = VaMeshBufferTypeExt.VertexTangent, bufferFormat = VaMeshBufferFormatExt.Float4 },
                };

                // Create Mesh for later editing
                _wingsMesh = new MeshResource(_wingsModel, 0, 0, bufferDescriptors, false /*decoupleAccessors*/, false /*initializeData*/);

                // Now that all the elements and resources have been created, we are ready for repeated frame updates.
                RequestUpdate(VaVolumeUpdateMode.FullFramerate);
            });
        }

        // Get morph information of a model to edit Mesh Buffers (SharpGLTF dependency)
        private async Task GetMorphInfo(string uri)
        {
            string assetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, uri).Replace("\\", "/");
            ModelRoot model = await Task.Run(() => ModelRoot.Load(assetPath));

            foreach (var node in model.LogicalNodes)
            {
                // Access the mesh of the model
                if (node.Mesh != null)
                {
                    // Loop through the primitives of the mesh
                    foreach (var primitive in node.Mesh.Primitives)
                    {
                        // Access vertex data
                        var positions = primitive.GetVertexAccessor("POSITION");
                        if (positions != null)
                        {
                            _meshVertexPositions = new List<Vector3>(positions.AsVector3Array());
                        }

                        var normals = primitive.GetVertexAccessor("NORMAL");
                        if (normals != null)
                        {
                            _meshVertexNormals = new List<Vector3>(normals.AsVector3Array());
                        }

                        var tangents = primitive.GetVertexAccessor("TANGENT");
                        if (tangents != null)
                        {
                            _meshVertexTangents = new List<Vector4>(tangents.AsVector4Array());
                        }

                        // Check if the primitive has any morph targets (blendshapes)
                        if (primitive.MorphTargetsCount > 0)
                        {
                            Debug.WriteLine(node.Name + " mesh has blendshapes.");

                            // Iterate through each morph target (blendshape)
                            for (int i = 0; i < primitive.MorphTargetsCount; i++)
                            {
                                var morphTarget = primitive.GetMorphTargetAccessors(i);

                                // Access morph data
                                if (morphTarget.TryGetValue("POSITION", out var positionAccessor))
                                {
                                    var morphPositions = positionAccessor.AsVector3Array();
                                    _morphVertexPositions = new List<Vector3>(morphPositions);
                                }

                                if (morphTarget.TryGetValue("NORMAL", out var normalAccessor))
                                {
                                    var morphNormals = normalAccessor.AsVector3Array();
                                    _morphVertexNormals = new List<Vector3>(morphNormals);
                                }

                                if (morphTarget.TryGetValue("TANGENT", out var tangentAccessor))
                                {
                                    var morphTangents = tangentAccessor.AsColorArray(0);
                                    _morphVertexTangents = new List<Vector4>(morphTangents);
                                }
                            }
                            _vertexCount = Math.Min(_meshVertexPositions.Count, _morphVertexPositions.Count);
                            _blendedVertices = new float[_vertexCount * 3];
                            _blendedNormals = new float[_vertexCount * 3];
                            _blendedTangents = new float[_vertexCount * 4];
                        }
                        else
                        {
                            Debug.WriteLine("No blendshapes found in " + node.Name);
                        }
                    }
                }
            }
        }

        // Edit Mesh Buffers to animate a mesh between its default position and a morph
        private void wingsAnimation()
        {
            if (_wingsModel?.IsReady == true && _wingsMesh != null && _wingsActive && _wingsHasMorph)
            {
                // Smooth ping-pong between 0 and 1
                float velocity = 5f;
                float blend = (float)((Math.Sin(DateTime.Now.TimeOfDay.TotalSeconds * velocity) * 0.5 + 0.5));

                _wingsMesh!.WriteMeshBuffers(
                    [VaMeshBufferTypeExt.VertexPosition, VaMeshBufferTypeExt.VertexNormal, VaMeshBufferTypeExt.VertexTangent],
                    (IReadOnlyList<MeshBufferData> meshBuffers) =>
                    {
                        for (int i = 0; i < _vertexCount; i++)
                        {
                            // Positions
                            var basePos = _meshVertexPositions[i];
                            var deltaPos = _morphVertexPositions[i];
                            var finalPos = basePos + deltaPos * blend;

                            // Normals
                            var baseNorm = _meshVertexNormals[i];
                            var deltaNorm = _morphVertexNormals[i];
                            var finalNorm = baseNorm + deltaNorm * blend;

                            // Tangents
                            var baseTangent = _meshVertexTangents[i];
                            var deltaTangent = _morphVertexTangents[i];
                            var finalTangent = baseTangent + deltaTangent * blend;

                            // Write position
                            _blendedVertices[i * 3 + 0] = finalPos.X;
                            _blendedVertices[i * 3 + 1] = finalPos.Y;
                            _blendedVertices[i * 3 + 2] = finalPos.Z;

                            // Write normal
                            _blendedNormals[i * 3 + 0] = finalNorm.X;
                            _blendedNormals[i * 3 + 1] = finalNorm.Y;
                            _blendedNormals[i * 3 + 2] = finalNorm.Z;

                            // Write tangent
                            _blendedTangents[i * 4 + 0] = finalTangent.X;
                            _blendedTangents[i * 4 + 1] = finalTangent.Y;
                            _blendedTangents[i * 4 + 2] = finalTangent.Z;
                            _blendedTangents[i * 4 + 3] = finalTangent.W;
                        }

                        Marshal.Copy(_blendedVertices, 0, meshBuffers[0].Buffer, _blendedVertices.Length);
                        Marshal.Copy(_blendedNormals, 0, meshBuffers[1].Buffer, _blendedNormals.Length);
                        Marshal.Copy(_blendedTangents, 0, meshBuffers[2].Buffer, _blendedTangents.Length);
                    }
                );
            }
        }
    }
}
