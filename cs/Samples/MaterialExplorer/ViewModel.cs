using Microsoft.MixedReality.Volumetric;
using SharpGLTF.Schema2;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CsMaterialExplorer
{
    public class MaterialData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private void SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
        {
            if (!object.Equals(storage, value))
            {
                storage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string? _materialName = "Material Name";
        public string? MaterialName
        {
            get => _materialName;
            set => SetProperty(ref _materialName, value);
        }

        private string? _materialType = "Material Type";
        public string? MaterialType
        {
            get => _materialType;
            set => SetProperty(ref _materialType, value);
        }

        private float _baseColorFactorR = 1f;
        public float BaseColorFactorR
        {
            get => _baseColorFactorR;
            set => SetProperty(ref _baseColorFactorR, value);
        }


        private float _baseColorFactorG = 1f;
        public float BaseColorFactorG
        {
            get => _baseColorFactorG;
            set => SetProperty(ref _baseColorFactorG, value);
        }

        private float _baseColorFactorB = 1f;
        public float BaseColorFactorB
        {
            get => _baseColorFactorB;
            set => SetProperty(ref _baseColorFactorB, value);
        }

        private float _baseColorFactorA = 1f;
        public float BaseColorFactorA
        {
            get => _baseColorFactorA;
            set => SetProperty(ref _baseColorFactorA, value);
        }

        private float _metallicFactor = 1f;
        public float MetallicFactor
        {
            get => _metallicFactor;
            set => SetProperty(ref _metallicFactor, value);
        }

        private float _roughnessFactor = 1f;
        public float RoughnessFactor
        {
            get => _roughnessFactor;
            set => SetProperty(ref _roughnessFactor, value);
        }

        private string? _baseColorTextureUri;
        public string? BaseColorTextureUri
        {
            get => _baseColorTextureUri;
            set => SetProperty(ref _baseColorTextureUri, value);
        }

        private string? _metallicRoughnessTextureUri;
        public string? MetallicRoughnessTextureUri
        {
            get => _metallicRoughnessTextureUri;
            set => SetProperty(ref _metallicRoughnessTextureUri, value);
        }

        private string? _normalTextureUri;
        public string? NormalTextureUri
        {
            get => _normalTextureUri;
            set => SetProperty(ref _normalTextureUri, value);
        }

        private string? _occlusionTextureUri;
        public string? OcclusionTextureUri
        {
            get => _occlusionTextureUri;
            set => SetProperty(ref _occlusionTextureUri, value);
        }

        private string? _emissiveTextureUri;
        public string? EmissiveTextureUri
        {
            get => _emissiveTextureUri;
            set => SetProperty(ref _emissiveTextureUri, value);
        }

        private TextureLoadStatus _baseColorTextureStatus = TextureLoadStatus.None;
        public TextureLoadStatus BaseColorTextureStatus
        {
            get => _baseColorTextureStatus;
            set => SetProperty(ref _baseColorTextureStatus, value);
        }

        private string? _baseColorTextureError;
        public string? BaseColorTextureError
        {
            get => _baseColorTextureError;
            set => SetProperty(ref _baseColorTextureError, value);
        }

        private TextureLoadStatus _normalTextureStatus = TextureLoadStatus.None;
        public TextureLoadStatus NormalTextureStatus
        {
            get => _normalTextureStatus;
            set => SetProperty(ref _normalTextureStatus, value);
        }

        private string? _normalTextureError;
        public string? NormalTextureError
        {
            get => _normalTextureError;
            set => SetProperty(ref _normalTextureError, value);
        }

        private TextureLoadStatus _metallicRoughnessTextureStatus = TextureLoadStatus.None;
        public TextureLoadStatus MetallicRoughnessTextureStatus
        {
            get => _metallicRoughnessTextureStatus;
            set => SetProperty(ref _metallicRoughnessTextureStatus, value);
        }

        private string? _metallicRoughnessTextureError;
        public string? MetallicRoughnessTextureError
        {
            get => _metallicRoughnessTextureError;
            set => SetProperty(ref _metallicRoughnessTextureError, value);
        }

        private TextureLoadStatus _occlusionTextureStatus = TextureLoadStatus.None;
        public TextureLoadStatus OcclusionTextureStatus
        {
            get => _occlusionTextureStatus;
            set => SetProperty(ref _occlusionTextureStatus, value);
        }

        private string? _occlusionTextureError;
        public string? OcclusionTextureError
        {
            get => _occlusionTextureError;
            set => SetProperty(ref _occlusionTextureError, value);
        }

        private TextureLoadStatus _emissiveTextureStatus = TextureLoadStatus.None;
        public TextureLoadStatus EmissiveTextureStatus
        {
            get => _emissiveTextureStatus;
            set => SetProperty(ref _emissiveTextureStatus, value);
        }

        private string? _emissiveTextureError;
        public string? EmissiveTextureError
        {
            get => _emissiveTextureError;
            set => SetProperty(ref _emissiveTextureError, value);
        }

        private float _normalScale = 1f;
        public float NormalScale
        {
            get => _normalScale;
            set => SetProperty(ref _normalScale, value);
        }

        private float _occlusionStrength = 1f;
        public float OcclusionStrength
        {
            get => _occlusionStrength;
            set => SetProperty(ref _occlusionStrength, value);
        }
    }

    public class ViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<MaterialData> Materials { get; } = new ObservableCollection<MaterialData>();

        private string fileUri = string.Empty;
        public string FileUri
        {
            get => fileUri;
            set => SetProperty(ref fileUri, value);
        }

        private MaterialData? _selectedValue;
        public MaterialData? SelectedValue
        {
            get => _selectedValue;
            set => SetProperty(ref _selectedValue, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
        {
            if (!object.Equals(storage, value))
            {
                storage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        // Use SharpGLTF to load the glTF file and extract material information
        internal async Task LoadGltfAsync(string path)
        {
            Materials.Clear();
            FileUri = new Uri(path).AbsoluteUri;
            ModelRoot model = await Task.Run(() => ModelRoot.Load(path));

            // Load materials using SharpGLTF
            foreach (Material material in model.LogicalMaterials)
            {
                var data = new MaterialData()
                {
                    MaterialName = material.Name,
                    MaterialType = VaMaterialTypeExt.Pbr.ToString(),
                };

                var channel = material.FindChannel("BaseColor");
                if (channel != null && channel.HasValue)
                {
                    data.BaseColorFactorR = channel.Value.Color.X;
                    data.BaseColorFactorG = channel.Value.Color.Y;
                    data.BaseColorFactorB = channel.Value.Color.Z;
                    data.BaseColorFactorA = channel.Value.Color.W;
                }

                channel = material.FindChannel("MetallicRoughness");
                if (channel != null && channel.HasValue)
                {
                    var metallicFactor = channel.Value.Parameters.FirstOrDefault(m => m.Name == "MetallicFactor");
                    data.MetallicFactor = metallicFactor == null ? 0.5f : (float)metallicFactor.Value;
                    var roughnessFactor = channel.Value.Parameters.FirstOrDefault(m => m.Name == "RoughnessFactor");
                    data.RoughnessFactor = roughnessFactor == null ? 0.5f : (float)roughnessFactor.Value;
                }

                channel = material.FindChannel("Normal");
                if (channel != null && channel.HasValue)
                {
                    var normalScale = channel.Value.Parameters.FirstOrDefault(m => m.Name == "NormalScale");
                    data.NormalScale = normalScale == null ? 1f : (float)normalScale.Value;
                }

                channel = material.FindChannel("Occlusion");
                if (channel != null && channel.HasValue)
                {
                    var occlusionStrength = channel.Value.Parameters.FirstOrDefault(m => m.Name == "OcclusionStrength");
                    data.OcclusionStrength = occlusionStrength == null ? 1f : (float)occlusionStrength.Value;
                }

                Materials.Add(data);
            }

            SelectedValue = Materials.FirstOrDefault();
        }
    }
}
