using Microsoft.MixedReality.Volumetric;
using System;
using System.ComponentModel;
using System.Text;
using System.Collections.Generic;

namespace CsMaterialExplorer
{
    public enum TextureLoadStatus
    {
        None,
        Loading,
        Success,
        Error
    }

    public enum TextureType
    {
        BaseColor,
        Normal,
        Occlusion,
        MetallicRoughness,
        Emissive,
        Unknown
    }

    public class TextureStatusEventArgs : EventArgs
    {
        public TextureType TextureType { get; }
        public TextureLoadStatus Status { get; }
        public string? ErrorMessage { get; }

        public TextureStatusEventArgs(TextureType textureType, TextureLoadStatus status, string? errorMessage = null)
        {
            TextureType = textureType;
            Status = status;
            ErrorMessage = errorMessage;
        }
    }

    public class VolumetricModel
    {
        private VolumetricApp _app;
        private Volume? _volume;
        private Elements _elements;
        struct Elements
        {
            public VisualElement? Visual;
            public ModelResource? Model;
            public MaterialResource? Material;
            public TextureResource? BaseColorTexture;
            public TextureResource? MetallicRoughnessTexture;
            public TextureResource? NormalTexture;
            public TextureResource? OcclusionTexture;
            public TextureResource? EmissiveTexture;
        }

        readonly private object _materialLock = new object();
        private MaterialData? _materialData;
        private HashSet<string> _changedProperties = new HashSet<string>(); // Track changed properties

        public event EventHandler<TextureStatusEventArgs>? TextureStatusChanged;

        public VolumetricModel(string appName)
        {
            _app = new VolumetricApp(appName,
                requiredExtensions: new string[] {
                    Extensions.VA_EXT_gltf2_model_resource,
                    Extensions.VA_EXT_material_resource,
                    Extensions.VA_EXT_texture_resource},
                waitForSystemBehavior: VaSessionWaitForSystemBehavior.RetrySilently);
            _app.RunAsync();
        }

        public void OpenVolume(string? modelUri = null)
        {
            _volume = new Volume(_app);
            _volume.OnReady += volume =>
            {
                _elements = new Elements();
                _elements.Model = new ModelResource(volume, modelUri);
                _elements.Visual = new VisualElement(volume, _elements.Model);
                _volume.RequestUpdate();
            };
            _volume.OnUpdate += _ => OnVolumeUpdate();
            _volume.OnClose += _ =>
            {
                _volume = null;
                _elements = new Elements();
            };
        }

        public void CloseVolume()
        {
            if (_volume is not null)
            {
                _volume.RequestClose();
            }
        }

        public void LoadModel(string uri)
        {
            if (_volume is null)
            {
                OpenVolume(uri);   // Lazy create volume, in case it was closed earlier.
            }
            else if (_elements.Model != null)
            {
                _elements.Model.SetModelUri(uri);
                _volume.RequestUpdate();
            }
        }
        public void SelectMaterial(MaterialData material)
        {
            lock (_materialLock)
            {
                if (_materialData is not null)
                {
                    _materialData.PropertyChanged -= Material_PropertyChanged;
                }

                _materialData = material;
                _materialData.PropertyChanged += Material_PropertyChanged;

                // When selecting a new material, clear all dirty properties
                _changedProperties.Clear();

                // Destroy previous material node and reset to null, so that
                // a new one will be lazily created and connected with new material data.
                _elements.Material?.Destroy();
                _elements.Material = null;
            }
        }

        private void Material_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            lock (_materialLock)
            {
                // Add the changed property name to our tracking set
                _changedProperties.Add(e.PropertyName ?? string.Empty);
            }
            _volume?.RequestUpdate();    // Request an update to ensure the material change is applied.
        }

        private void OnTextureResourceAsyncStateChanged(TextureResource sourceTexture, TextureType textureType, VaElementAsyncState asyncState)
        {
            if (asyncState == VaElementAsyncState.Error)
            {
                StringBuilder errorsForDisplay = new();
                sourceTexture.GetAsyncErrors((error, errorMsg) =>
                {
                    errorsForDisplay.AppendLine(FormattableString.Invariant($"Error: {error} - {errorMsg}"));
                });

                string errorMessage = errorsForDisplay.ToString();
                TextureStatusChanged?.Invoke(this, new TextureStatusEventArgs(textureType, TextureLoadStatus.Error, errorMessage));
            }
            else if (asyncState == VaElementAsyncState.Ready)
            {
                TextureStatusChanged?.Invoke(this, new TextureStatusEventArgs(textureType, TextureLoadStatus.Success));
            }
            // Potentially handle other states like None or Loading if needed
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

        private void OnVolumeUpdate()
        {
            lock (_materialLock)
            {
                if (_materialData is not null && _changedProperties.Count > 0)
                {
                    if (_elements.Material is null)
                    {
                        _elements.Material = new MaterialResource(_elements.Model!, _materialData.MaterialName!);
                    }

                    if (_changedProperties.Contains(nameof(MaterialData.BaseColorFactorR)) ||
                        _changedProperties.Contains(nameof(MaterialData.BaseColorFactorG)) ||
                        _changedProperties.Contains(nameof(MaterialData.BaseColorFactorB)) ||
                        _changedProperties.Contains(nameof(MaterialData.BaseColorFactorA)))
                    {
                        _elements.Material.SetBaseColorFactor(sRGBToLinear(new VaColor4f()
                        {
                            r = _materialData.BaseColorFactorR,
                            g = _materialData.BaseColorFactorG,
                            b = _materialData.BaseColorFactorB,
                            a = _materialData.BaseColorFactorA,
                        }));
                    }

                    if (_changedProperties.Contains(nameof(MaterialData.MetallicFactor)))
                    {
                        _elements.Material.SetMetallicFactor(_materialData.MetallicFactor);
                    }

                    if (_changedProperties.Contains(nameof(MaterialData.RoughnessFactor)))
                    {
                        _elements.Material.SetRoughnessFactor(_materialData.RoughnessFactor);
                    }

                    if (_changedProperties.Contains(nameof(MaterialData.MetallicRoughnessTextureUri)))
                    {
                        if (!string.IsNullOrEmpty(_materialData.MetallicRoughnessTextureUri))
                        {
                            if (_elements.MetallicRoughnessTexture is null)
                            {
                                _elements.MetallicRoughnessTexture = new TextureResource(_volume!);
                                _elements.MetallicRoughnessTexture.OnAsyncStateChanged = (VaElementAsyncState asyncState) =>
                                {
                                    OnTextureResourceAsyncStateChanged(_elements.MetallicRoughnessTexture!, TextureType.MetallicRoughness, asyncState);
                                };
                                _elements.Material.SetPbrMetallicRoughnessTexture(_elements.MetallicRoughnessTexture);
                            }
                            _elements.MetallicRoughnessTexture.SetImageUri(_materialData.MetallicRoughnessTextureUri);
                        }
                        else
                        {
                            _elements.MetallicRoughnessTexture?.Destroy();
                            _elements.MetallicRoughnessTexture = null;
                        }
                    }

                    if (_changedProperties.Contains(nameof(MaterialData.BaseColorTextureUri)))
                    {
                        if (!string.IsNullOrEmpty(_materialData.BaseColorTextureUri))
                        {
                            if (_elements.BaseColorTexture is null)
                            {
                                _elements.BaseColorTexture = new TextureResource(_volume!);
                                _elements.BaseColorTexture.OnAsyncStateChanged = (VaElementAsyncState asyncState) =>
                                {
                                    OnTextureResourceAsyncStateChanged(_elements.BaseColorTexture!, TextureType.BaseColor, asyncState);
                                };
                                _elements.Material.SetPbrBaseColorTexture(_elements.BaseColorTexture);
                            }
                            _elements.BaseColorTexture.SetImageUri(_materialData.BaseColorTextureUri);
                        }
                        else
                        {
                            _elements.BaseColorTexture?.Destroy();
                            _elements.BaseColorTexture = null;
                        }
                    }
                    if (_changedProperties.Contains(nameof(MaterialData.NormalScale)) &&
                        _elements.NormalTexture != null)
                    {
                        _elements.NormalTexture.SetNormalScale(_materialData.NormalScale);
                    }

                    if (_changedProperties.Contains(nameof(MaterialData.NormalTextureUri)))
                    {
                        if (!string.IsNullOrEmpty(_materialData.NormalTextureUri))
                        {
                            if (_elements.NormalTexture is null)
                            {
                                _elements.NormalTexture = new TextureResource(_volume!);
                                _elements.NormalTexture.OnAsyncStateChanged = (VaElementAsyncState asyncState) =>
                                {
                                    OnTextureResourceAsyncStateChanged(_elements.NormalTexture!, TextureType.Normal, asyncState);
                                };
                                _elements.Material.SetNormalTexture(_elements.NormalTexture);
                            }

                            _elements.NormalTexture.SetImageUri(_materialData.NormalTextureUri);
                            _elements.NormalTexture.SetNormalScale(_materialData.NormalScale);
                        }
                        else
                        {
                            _elements.NormalTexture?.Destroy();
                            _elements.NormalTexture = null;
                        }
                    }

                    if (_changedProperties.Contains(nameof(MaterialData.OcclusionStrength)) &&
                        _elements.OcclusionTexture != null)
                    {
                        _elements.OcclusionTexture.SetOcclusionStrength(_materialData.OcclusionStrength);
                    }

                    if (_changedProperties.Contains(nameof(MaterialData.OcclusionTextureUri)))
                    {
                        if (!string.IsNullOrEmpty(_materialData.OcclusionTextureUri))
                        {
                            if (_elements.OcclusionTexture is null)
                            {
                                _elements.OcclusionTexture = new TextureResource(_volume!);
                                _elements.OcclusionTexture.OnAsyncStateChanged = (VaElementAsyncState asyncState) =>
                                {
                                    OnTextureResourceAsyncStateChanged(_elements.OcclusionTexture!, TextureType.Occlusion, asyncState);
                                };
                                _elements.Material.SetOcclusionTexture(_elements.OcclusionTexture);
                            }

                            _elements.OcclusionTexture.SetImageUri(_materialData.OcclusionTextureUri);
                            _elements.OcclusionTexture.SetOcclusionStrength(_materialData.OcclusionStrength);
                        }
                        else
                        {
                            _elements.OcclusionTexture?.Destroy();
                            _elements.OcclusionTexture = null;
                        }
                    }

                    if (_changedProperties.Contains(nameof(MaterialData.EmissiveTextureUri)))
                    {
                        if (!string.IsNullOrEmpty(_materialData.EmissiveTextureUri))
                        {
                            if (_elements.EmissiveTexture is null)
                            {
                                _elements.EmissiveTexture = new TextureResource(_volume!);
                                _elements.EmissiveTexture.OnAsyncStateChanged = (VaElementAsyncState asyncState) =>
                                {
                                    OnTextureResourceAsyncStateChanged(_elements.EmissiveTexture!, TextureType.Emissive, asyncState);
                                };
                                _elements.Material.SetEmissiveTexture(_elements.EmissiveTexture);
                            }
                            _elements.EmissiveTexture.SetImageUri(_materialData.EmissiveTextureUri);
                        }
                        else
                        {
                            _elements.EmissiveTexture?.Destroy();
                            _elements.EmissiveTexture = null;
                        }
                    }

                    // Clear the changed properties after processing
                    _changedProperties.Clear();
                }
            }
        }
    }
}
