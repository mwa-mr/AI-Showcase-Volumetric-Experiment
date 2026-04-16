using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Globalization.NumberFormatting;
using Windows.Graphics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CsMaterialExplorer
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private ViewModel _viewModel = new ViewModel();
        private VolumetricModel _volumetricModel = new VolumetricModel("CsMaterialExplorer");
        private INumberFormatter2 _numberFormatter;

        public MainWindow()
        {
            this.InitializeComponent();

            this.AppWindow.Title = "Material Explorer 03";
            this.AppWindow.Resize(new SizeInt32(800, 600));
            _numberFormatter = CreateNumberFormatter();

            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            _volumetricModel.TextureStatusChanged += VolumetricModel_TextureStatusChanged;
        }

        private void VolumetricModel_TextureStatusChanged(object? sender, TextureStatusEventArgs e)
        {
            if (_viewModel.SelectedValue != null)
            {
                // Dispatch to UI thread since OnTextureResourceAsyncStateChanged fires on volume update thread
                DispatcherQueue.TryEnqueue(() =>
                {
                    switch (e.TextureType)
                    {
                        case TextureType.BaseColor:
                            _viewModel.SelectedValue.BaseColorTextureStatus = e.Status;
                            _viewModel.SelectedValue.BaseColorTextureError = e.ErrorMessage;
                            break;
                        case TextureType.MetallicRoughness:
                            _viewModel.SelectedValue.MetallicRoughnessTextureStatus = e.Status;
                            _viewModel.SelectedValue.MetallicRoughnessTextureError = e.ErrorMessage;
                            break;
                        case TextureType.Normal:
                            _viewModel.SelectedValue.NormalTextureStatus = e.Status;
                            _viewModel.SelectedValue.NormalTextureError = e.ErrorMessage;
                            break;
                        case TextureType.Occlusion:
                            _viewModel.SelectedValue.OcclusionTextureStatus = e.Status;
                            _viewModel.SelectedValue.OcclusionTextureError = e.ErrorMessage;
                            break;
                        case TextureType.Emissive:
                            _viewModel.SelectedValue.EmissiveTextureStatus = e.Status;
                            _viewModel.SelectedValue.EmissiveTextureError = e.ErrorMessage;
                            break;
                    }
                });
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedValue")
            {
                var material = _viewModel.SelectedValue;
                if (material != null)
                {
                    _volumetricModel.SelectMaterial(material);
                }
            }
        }

        private static DecimalFormatter CreateNumberFormatter()
        {
            IncrementNumberRounder rounder = new IncrementNumberRounder();
            rounder.Increment = 0.01;
            rounder.RoundingAlgorithm = RoundingAlgorithm.RoundHalfToEven;

            DecimalFormatter formatter = new DecimalFormatter();
            formatter.IntegerDigits = 1;
            formatter.FractionDigits = 2;
            formatter.NumberRounder = rounder;
            return formatter;
        }


        public class CursorHelper
        {
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            private static extern IntPtr SetCursor(IntPtr hCursor);

            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            internal static extern IntPtr GetCursor();

            private const int IDC_WAIT = 32514; // Standard wait cursor

            public static void SetBusyCursor()
            {
                // Load and set the wait cursor
                IntPtr waitCursor = LoadCursor(IntPtr.Zero, IDC_WAIT);
                SetCursor(waitCursor);
            }

            public static void RestoreCursor(IntPtr originalCursor)
            {
                // Restore the original cursor
                SetCursor(originalCursor);
            }
        }

        private async void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Objects3D;
            picker.FileTypeFilter.Add(".glb");
            picker.FileTypeFilter.Add(".gltf");

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                var uri = new Uri(file.Path).AbsoluteUri;

                var _originalCursor = CursorHelper.GetCursor();
                CursorHelper.SetBusyCursor();
                try
                {
                    await _viewModel.LoadGltfAsync(file.Path);
                    _volumetricModel.LoadModel(uri);

                    if (_viewModel.Materials.Count > 0)
                    {
                        _viewModel.SelectedValue = _viewModel.Materials[0];
                    }
                }
                finally
                {
                    CursorHelper.RestoreCursor(_originalCursor);
                }
            }
            else
            {
                await ShowMessageAsync("Error", "Must choose a file to continue");
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            if (_volumetricModel is not null)
            {
                _volumetricModel.CloseVolume();
            }

            if (_viewModel is not null)
            {
                _viewModel.FileUri = string.Empty;
                _viewModel.Materials.Clear();
            }
        }

        private ContentDialog? _messageBox;
        private async Task ShowMessageAsync(string title, string message)
        {
            if (_messageBox != null)
            {
                _messageBox.Hide();
            }

            _messageBox = new ContentDialog
            {
                Title = title,
                XamlRoot = this.Content.XamlRoot,
                Content = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 10,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = message,
                            VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center
                        }
                    }
                },
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close
            };

            await _messageBox.ShowAsync();
        }

        private async void SelectTexture_Click(object sender, RoutedEventArgs e, TextureType textureProperty)
        {
            if (_viewModel.SelectedValue is null)
            {
                await ShowMessageAsync("Error", "Please select a material first.");
                return;
            }

            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                var uri = new Uri(file.Path).AbsoluteUri;
                switch (textureProperty)
                {
                    case TextureType.BaseColor:
                        _viewModel.SelectedValue.BaseColorTextureUri = uri;
                        _viewModel.SelectedValue.BaseColorTextureStatus = TextureLoadStatus.Loading;
                        break;
                    case TextureType.Normal:
                        _viewModel.SelectedValue.NormalTextureUri = uri;
                        _viewModel.SelectedValue.NormalTextureStatus = TextureLoadStatus.Loading;
                        break;
                    case TextureType.MetallicRoughness:
                        _viewModel.SelectedValue.MetallicRoughnessTextureUri = uri;
                        _viewModel.SelectedValue.MetallicRoughnessTextureStatus = TextureLoadStatus.Loading;
                        break;
                    case TextureType.Occlusion:
                        _viewModel.SelectedValue.OcclusionTextureUri = uri;
                        _viewModel.SelectedValue.OcclusionTextureStatus = TextureLoadStatus.Loading;
                        break;
                    case TextureType.Emissive:
                        _viewModel.SelectedValue.EmissiveTextureUri = uri;
                        _viewModel.SelectedValue.EmissiveTextureStatus = TextureLoadStatus.Loading;
                        break;
                }
            }
        }

        private async void ResetTextureResource_Click(object sender, RoutedEventArgs e, TextureType textureProperty)
        {
            if (_viewModel.SelectedValue is null)
            {
                await ShowMessageAsync("Error", "Please select a material first.");
                return;
            }

            switch (textureProperty)
            {
                case TextureType.BaseColor:
                    _viewModel.SelectedValue.BaseColorTextureUri = null;
                    _viewModel.SelectedValue.BaseColorTextureStatus = TextureLoadStatus.None;
                    break;
                case TextureType.MetallicRoughness:
                    _viewModel.SelectedValue.MetallicRoughnessTextureUri = null;
                    _viewModel.SelectedValue.MetallicRoughnessTextureStatus = TextureLoadStatus.None;
                    break;
                case TextureType.Normal:
                    _viewModel.SelectedValue.NormalTextureUri = null;
                    _viewModel.SelectedValue.NormalTextureStatus = TextureLoadStatus.None;
                    break;
                case TextureType.Occlusion:
                    _viewModel.SelectedValue.OcclusionTextureUri = null;
                    _viewModel.SelectedValue.OcclusionTextureStatus = TextureLoadStatus.None;
                    break;
                case TextureType.Emissive:
                    _viewModel.SelectedValue.EmissiveTextureUri = null;
                    _viewModel.SelectedValue.EmissiveTextureStatus = TextureLoadStatus.None;
                    break;
            }
        }

        private void SelectBaseColorTexture_Click(object sender, RoutedEventArgs e)
        {
            SelectTexture_Click(sender, e, TextureType.BaseColor);
        }

        private void SelectMetallicRoughnessTexture_Click(object sender, RoutedEventArgs e)
        {
            SelectTexture_Click(sender, e, TextureType.MetallicRoughness);
        }

        private void SelectNormalTexture_Click(object sender, RoutedEventArgs e)
        {
            SelectTexture_Click(sender, e, TextureType.Normal);
        }

        private void SelectOcclusionTexture_Click(object sender, RoutedEventArgs e)
        {
            SelectTexture_Click(sender, e, TextureType.Occlusion);
        }

        private void SelectEmissiveTexture_Click(object sender, RoutedEventArgs e)
        {
            SelectTexture_Click(sender, e, TextureType.Emissive);
        }

        private void ResetBaseColorTexture_Click(object sender, RoutedEventArgs e)
        {
            ResetTextureResource_Click(sender, e, TextureType.BaseColor);
        }

        private void ResetMetallicRoughnessTexture_Click(object sender, RoutedEventArgs e)
        {
            ResetTextureResource_Click(sender, e, TextureType.MetallicRoughness);
        }

        private void ResetNormalTexture_Click(object sender, RoutedEventArgs e)
        {
            ResetTextureResource_Click(sender, e, TextureType.Normal);
        }

        private void ResetOcclusionTexture_Click(object sender, RoutedEventArgs e)
        {
            ResetTextureResource_Click(sender, e, TextureType.Occlusion);
        }

        private void ResetEmissiveTexture_Click(object sender, RoutedEventArgs e)
        {
            ResetTextureResource_Click(sender, e, TextureType.Emissive);
        }
    }
}
