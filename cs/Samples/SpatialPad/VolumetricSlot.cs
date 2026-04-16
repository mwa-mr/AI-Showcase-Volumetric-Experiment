using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.MixedReality.Volumetric;
using System.Runtime.InteropServices;

namespace Volumetric.Samples.SpatialPad
{
    public class VolumetricSlot
    {
        // 3D Slot model properties
        public ModelResource? Model;
        public VisualElement? Visual;
        public ModelResource? DisabledModel;
        public VisualElement? DisabledVisual;
        private MeshResource? _mesh;
        private MaterialResource? _material;
        public VaVector3f PositionInSpace
        {
            get { return getGridPosition(); }
        }

        private Slot _slot;
        public bool Initialized = false;
        public bool ModelHasChanged = true;
        public DateTime ModificationTime;

        public bool Enabled
        {
            get
            {
                if (_slot.Shortcut.App == ShortcutApp.powertoys)
                {
                    return ShortcutsManager.IsPowerToysRunning;
                }
                else
                {
                    return ShortcutsManager.ForegroundApp.ToLower().Contains(_slot.Shortcut.App.ToString()) || _slot.Shortcut.App.ToString().Equals("windows");
                }
            }
        }

        // Press autton animation properties
        public bool AnimationIn = false;
        public bool AnimationOut = false;
        private float pressAnimationSpeed = 5f;
        private float _lightenColor = 1.3f;
        private float blend = 0f;
        private DateTime _lastAnimationTime;
        private bool _buttonHasMorph
        {
            get
            {
                var data = Data.ButtonsData[_slot.Shortcut.ButtonTypeData];

                return
                    data.BlendedVertices != null &&
                    data.BlendedNormals != null &&
                    data.BlendedTangents != null;
            }
        }

        public VolumetricSlot(Volume volume, Slot slot)
        {
            _slot = slot;
            initializeSlot(volume);
        }

        // Set slot 
        private void initializeSlot(Volume volume)
        {
            // Get assets URI
            var _uri = VolumetricApp.GetAssetUri(Data.ButtonsData[_slot.Shortcut.ButtonTypeData].ModelUri);
            var _disabledUri = VolumetricApp.GetAssetUri(Data.ButtonsData[_slot.Shortcut.ButtonTypeData].DisabledModelUri);

            // Load button GLB model
            Model = new ModelResource(volume, _uri);
            DisabledModel = new ModelResource(volume, _disabledUri);
            // Check model state changes
            Model.OnAsyncStateChanged += onAsyncStateChanged;
            DisabledModel.OnAsyncStateChanged += onAsyncStateChanged;
            // Create Visual Element with an associated model
            Visual = new VisualElement(volume, Model);
            DisabledVisual = new VisualElement(volume, DisabledModel);
            // Hide Visual element
            Visual.SetVisible(false);
            DisabledVisual.SetVisible(false);
            // Reference model materials for later editing
            _material = new MaterialResource(Model, "mat");
            ModelHasChanged = true;

            // Create mesh buffers for mesh editing
            var bufferDescriptors = new VaMeshBufferDescriptorExt[]
            {
                new VaMeshBufferDescriptorExt { bufferType = VaMeshBufferTypeExt.Index, bufferFormat = VaMeshBufferFormatExt.Uint32 },
                new VaMeshBufferDescriptorExt { bufferType = VaMeshBufferTypeExt.VertexPosition, bufferFormat = VaMeshBufferFormatExt.Float3 },
                new VaMeshBufferDescriptorExt { bufferType = VaMeshBufferTypeExt.VertexNormal, bufferFormat = VaMeshBufferFormatExt.Float3 },
                new VaMeshBufferDescriptorExt { bufferType = VaMeshBufferTypeExt.VertexTangent, bufferFormat = VaMeshBufferFormatExt.Float4 },
            };
            _mesh = new MeshResource(Model, 0, 0, bufferDescriptors, false /*decoupleAccessors*/, false /*initializeData*/);
        }

        // Listen to model state changes 
        private void onAsyncStateChanged(VaElementAsyncState newState)
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

        // Perform actions when button is pressed
        public void OnButtonPressed()
        {
            if (_buttonHasMorph)
            {
                _lastAnimationTime = DateTime.Now;
                AnimationIn = true;
                lighten(true);
            }
            // Trigger shortcut
            _slot.Shortcut.Action();
        }

        // Calculate slot position according to grid
        private VaVector3f getGridPosition()
        {
            int nCols = 3;
            int nRows = 3;
            float spacing = 1.72f;

            int col = _slot.Index % nCols;
            int row = _slot.Index / nCols;

            float totalWidth = (nCols - 1) * spacing;
            float totalHeight = (nRows - 1) * spacing;

            float startX = -totalWidth / 2f;
            float startZ = -(totalHeight / 2f);

            VaVector3f position;

            position.x = startX + col * spacing;
            position.y = 0f;
            position.z = startZ + row * spacing;

            return position;
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

        // Edit material color when button is pressed
        private void lighten(bool state)
        {
            VaColor4f col = new VaColor4f();
            col.r = col.g = col.b = state ? _lightenColor : 1f;
            if (_material != null) _material.SetBaseColorFactor(sRGBToLinear(col));
        }

        // Set button state properties (enabled/disabled)
        public void Enable()
        {
            if (Model?.IsReady != true || Visual?.IsReady != true || DisabledModel?.IsReady != true || DisabledVisual?.IsReady != true) return;
            Visual.SetVisible(Enabled && _slot.HasShortcut);
            DisabledVisual.SetVisible(!Enabled && _slot.HasShortcut);
        }

        // Update model Uri if button type or color has changed
        public void UpdateModel()
        {
            if (Model == null) return;

            Visual?.SetVisible(false);
            DisabledVisual?.SetVisible(false);
            ModificationTime = DateTime.Now;
            Model.SetModelUri(VolumetricApp.GetAssetUri(Data.ButtonsData[_slot.Shortcut.ButtonTypeData].ModelUri));
            DisabledModel?.SetModelUri(VolumetricApp.GetAssetUri(Data.ButtonsData[_slot.Shortcut.ButtonTypeData].DisabledModelUri));
            ModelHasChanged = true;
        }

        // Edit Mesh Buffers to animate a mesh between its default position and a morph (animate button when pressed)
        public void PressButtonAnimation()
        {
            if ((AnimationIn || AnimationOut) && _mesh != null && _mesh?.IsReady == true && _buttonHasMorph)
            {
                // Smooth animation between 0 and 1 and return
                DateTime now = DateTime.Now;
                double deltaTime = (now - _lastAnimationTime).TotalSeconds;
                _lastAnimationTime = now;

                if (AnimationIn)
                {
                    blend += (float)(deltaTime * pressAnimationSpeed);
                    if (blend >= 1)
                    {
                        blend = 1;
                        AnimationIn = false;
                        AnimationOut = true;
                    }
                }
                else if (AnimationOut)
                {
                    blend -= (float)(deltaTime * pressAnimationSpeed);
                    if (blend <= 0)
                    {
                        blend = 0;
                        AnimationOut = false;
                        lighten(false);
                    }
                }

                _mesh!.WriteMeshBuffers(
                    [VaMeshBufferTypeExt.VertexPosition, VaMeshBufferTypeExt.VertexNormal, VaMeshBufferTypeExt.VertexTangent],
                    (IReadOnlyList<MeshBufferData> meshBuffers) =>
                    {
                        var buttonData = Data.ButtonsData[_slot.Shortcut.ButtonTypeData];

                        for (int i = 0; i < buttonData.VertexCount; i++)
                        {
                            // Positions
                            var basePos = buttonData.MeshVertexPositions[i];
                            var deltaPos = buttonData.MorphVertexPositions[i];
                            var finalPos = basePos + deltaPos * blend;

                            // Normal
                            var baseNorm = buttonData.MeshVertexNormals[i];
                            var deltaNorm = buttonData.MorphVertexNormals[i];
                            var finalNorm = baseNorm + deltaNorm * blend;

                            //Tangent
                            var baseTang = buttonData.MeshVertexTangents[i];
                            var deltaTang = buttonData.MorphVertexTangents[i];
                            var finalTang = baseTang + deltaTang * blend;

                            //Write position
                            buttonData.BlendedVertices[i * 3 + 0] = finalPos.X;
                            buttonData.BlendedVertices[i * 3 + 1] = finalPos.Y;
                            buttonData.BlendedVertices[i * 3 + 2] = finalPos.Z;

                            //Write normal
                            buttonData.BlendedNormals[i * 3 + 0] = finalNorm.X;
                            buttonData.BlendedNormals[i * 3 + 1] = finalNorm.Y;
                            buttonData.BlendedNormals[i * 3 + 2] = finalNorm.Z;

                            //Write tangent
                            buttonData.BlendedTangents[i * 4 + 0] = finalTang.X;
                            buttonData.BlendedTangents[i * 4 + 1] = finalTang.Y;
                            buttonData.BlendedTangents[i * 4 + 2] = finalTang.Z;
                            buttonData.BlendedTangents[i * 4 + 3] = finalTang.W;
                        }

                        Marshal.Copy(buttonData.BlendedVertices, 0, meshBuffers[0].Buffer, buttonData.BlendedVertices.Length);
                        Marshal.Copy(buttonData.BlendedNormals, 0, meshBuffers[1].Buffer, buttonData.BlendedNormals.Length);
                        Marshal.Copy(buttonData.BlendedTangents, 0, meshBuffers[2].Buffer, buttonData.BlendedTangents.Length);
                    }
                );
            }
        }
    }
}
