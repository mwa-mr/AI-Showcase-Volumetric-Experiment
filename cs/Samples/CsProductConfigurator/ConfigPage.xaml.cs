using System;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.Graphics.Canvas.Effects;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.UI;
using System.Linq;

namespace Volumetric.Samples.ProductConfigurator
{
    public sealed partial class ConfigPage : Page
    {
        private VolumetricExperience _volumetricExperience;

        private DateTime _lastTintTimeHeadband;
        private DateTime _lastTintTimeSpeaker;
        private float _tintRegulatorTime = 0.05f;

        private bool _tintingSpeaker = false;
        private bool _tintingHeadband = false;

        private enum SelectedComponent
        {
            Headband,
            Speakers
        }

        private SelectedComponent currentComponent = SelectedComponent.Headband;

        public ConfigPage()
        {
            this.InitializeComponent();

            GenerateTextureButtons();
            GenerateAccessoriesButtons();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            HeadsetHeadband.ImageOpened += (s, e) =>
            {
                ApplyTintEffect(HeadsetHeadband, Data.HeadbandSelectedColor, ref _tintingHeadband, ref _lastTintTimeHeadband);
            };
            HeadsetSpeaker.ImageOpened += (s, e) =>
            {
                ApplyTintEffect(HeadsetSpeaker, Data.SpeakersSelectedColor, ref _tintingSpeaker, ref _lastTintTimeSpeaker);
            };

            ElementColorPicker.ColorChanged += OnColorPickerColorChanged;
        }

        private void OnColorPickerColorChanged(ColorPicker sender, ColorChangedEventArgs e)
        {
            ApplySelectedColor(e.NewColor);
        }

        private void ApplySelectedColor(Color newColor, Color? optionalSecondColor = null)
        {

            if (optionalSecondColor.HasValue)
            {
                if (Data.HeadbandSelectedColor != newColor)
                {
                    Data.HeadbandSelectedColor = newColor;
                    ApplyTintEffect(HeadsetHeadband, newColor, ref _tintingHeadband, ref _lastTintTimeHeadband);
                    _volumetricExperience?.Volume?.ChangeHeadbandColor(newColor);
                }

                if (Data.SpeakersSelectedColor != optionalSecondColor.Value)
                {
                    Data.SpeakersSelectedColor = optionalSecondColor.Value;

                    ApplyTintEffect(HeadsetSpeaker, optionalSecondColor.Value, ref _tintingSpeaker, ref _lastTintTimeSpeaker);
                    _volumetricExperience?.Volume?.ChangeSpeakersColor(optionalSecondColor.Value);
                }

                ElementColorPicker.Color = currentComponent == SelectedComponent.Headband
                    ? newColor
                    : optionalSecondColor.Value;
            }
            else
            {
                if (currentComponent == SelectedComponent.Headband)
                {
                    if (Data.HeadbandSelectedColor == newColor) return;

                    Data.HeadbandSelectedColor = newColor;
                    ApplyTintEffect(HeadsetHeadband, newColor, ref _tintingHeadband, ref _lastTintTimeHeadband);
                    _volumetricExperience?.Volume?.ChangeHeadbandColor(newColor);
                }
                else
                {
                    if (Data.SpeakersSelectedColor == newColor) return;

                    Data.SpeakersSelectedColor = newColor;
                    ApplyTintEffect(HeadsetSpeaker, newColor, ref _tintingSpeaker, ref _lastTintTimeSpeaker);
                    _volumetricExperience?.Volume?.ChangeSpeakersColor(newColor);
                }
            }
        }

        private void ApplyRandomTexture()
        {
            var toggleButtons = ImageTogglePanel.Children
                .OfType<ToggleButton>()
                .ToList();

            if (toggleButtons.Count == 0) return;

            var random = new Random();
            int index = random.Next(toggleButtons.Count);
            var selectedToggle = toggleButtons[index];
            foreach (var toggle in toggleButtons)
            {
                toggle.IsChecked = toggle == selectedToggle;
            }
        }

        private void ApplyRandomAccessories()
        {
            var random = new Random();
            foreach (var child in AccessoriesImageTogglePanel.Children)
            {
                if (child is ToggleButton toggle)
                {
                    bool shouldSelect = random.NextDouble() < 0.5;
                    toggle.IsChecked = shouldSelect;
                }
            }
        }
        public void ShuffleHeadphonesOptions()
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                ApplySelectedColor(GenerateRandomColor(), GenerateRandomColor());
                ApplyRandomTexture();
                ApplyRandomAccessories();
            });
        }

        public static Color GenerateRandomColor()
        {
            var random = new Random();
            return Microsoft.UI.ColorHelper.FromArgb(
                255,
                (byte)random.Next(0, 256),
                (byte)random.Next(0, 256),
                (byte)random.Next(0, 256)
            );
        }

        private void HeadbandToggle_Click(object sender, RoutedEventArgs e)
        {
            UpdateToggleState(HeadbandToggle);
        }

        private void SpeakersToggle_Click(object sender, RoutedEventArgs e)
        {
            UpdateToggleState(SpeakersToggle);
        }
        private void UpdateToggleState(ToggleButton selectedToggle)
        {
            HeadbandToggle.IsChecked = selectedToggle == HeadbandToggle;
            SpeakersToggle.IsChecked = selectedToggle == SpeakersToggle;

            if (selectedToggle == HeadbandToggle)
            {
                currentComponent = SelectedComponent.Headband;
                ElementColorPicker.Color = Data.HeadbandSelectedColor;
            }
            else if (selectedToggle == SpeakersToggle)
            {
                currentComponent = SelectedComponent.Speakers;
                ElementColorPicker.Color = Data.SpeakersSelectedColor;
            }
        }

        private void deployButton_Pressed(object sender, RoutedEventArgs e)
        {
            if (_volumetricExperience == null)
            {
                _volumetricExperience = new VolumetricExperience("Product Configurator", this);
            }
            else
            {
                _volumetricExperience.CreateVolume();
            }
            DeployButtonState("loading");
        }

        public void DeployButtonState(string state)
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                switch (state)
                {
                    case "enabled":
                        deployButton.IsEnabled = true;
                        break;
                    case "disabled":
                        deployText.Text = "Deploy";
                        deployButton.IsHitTestVisible = true;
                        deployButton.IsEnabled = false;
                        break;
                    case "loading":
                        deployText.Text = "Loading...";
                        VisualStateManager.GoToState(deployButton, "Loading", true);
                        deployButton.IsHitTestVisible = false;
                        break;
                }
            });
        }

        private void GenerateTextureButtons()
        {
            foreach (var slot in Data.ImageSlots)
            {
                var toggle = new ToggleButton
                {
                    Width = 100,
                    Height = 100,
                    Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
                    Padding = new Thickness(0),
                    Margin = new Thickness(0),
                    BorderThickness = new Thickness(0),
                    BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
                    Tag = slot,
                    Style = (Style)Application.Current.Resources["CustomToggleButtonStyle"]
                };

                var image = new Image
                {
                    Source = new BitmapImage(new Uri($"ms-appx:///{slot.OriginalImage}")),
                    Stretch = Stretch.UniformToFill
                };

                toggle.Content = image;
                toggle.Checked += ToggleButton_Checked;
                toggle.Unchecked += ToggleButton_Unchecked;

                ImageTogglePanel.Children.Add(toggle);
            }
        }

        private void GenerateAccessoriesButtons()
        {
            foreach (var slot in Data.AccessoriesSlot)
            {
                var toggle = new ToggleButton
                {
                    Width = 148,
                    Height = 236,
                    Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
                    Padding = new Thickness(0),
                    Margin = new Thickness(0),
                    BorderThickness = new Thickness(0),
                    CornerRadius = new CornerRadius(10),
                    BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
                    Tag = slot,
                    Style = (Style)Application.Current.Resources["CustomToggleButtonStyle"]
                };

                var image = new Image
                {
                    Source = new BitmapImage(new Uri($"ms-appx:///{slot.OriginalImage}")),
                    Stretch = Stretch.UniformToFill
                };

                toggle.Content = image;
                toggle.Checked += Accesory_Checked;
                toggle.Unchecked += Accesory_Unchecked;

                AccessoriesImageTogglePanel.Children.Add(toggle);
            }
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            var clickedToggle = sender as ToggleButton;

            foreach (var child in ImageTogglePanel.Children)
            {
                if (child is ToggleButton toggle)
                {
                    var image = toggle.Content as Image;
                    var slot = toggle.Tag as ImageSlot;

                    if (toggle == clickedToggle)
                    {
                        var fadeOutAnimation = new DoubleAnimation
                        {
                            From = 1.0,
                            To = 0.0,
                            Duration = new Duration(TimeSpan.FromMilliseconds(300))
                        };

                        var fadeInAnimation = new DoubleAnimation
                        {
                            From = 0.0,
                            To = 1.0,
                            Duration = new Duration(TimeSpan.FromMilliseconds(300))
                        };

                        var fadeOutStoryboard = new Storyboard();
                        fadeOutStoryboard.Children.Add(fadeOutAnimation);
                        Storyboard.SetTarget(fadeOutAnimation, image);
                        Storyboard.SetTargetProperty(fadeOutAnimation, "Opacity");

                        fadeOutStoryboard.Completed += (s, args) =>
                        {
                            image.Source = new BitmapImage(new Uri($"ms-appx:///{slot.SelectedImage}"));
                            HeadsetTexture.Source = new BitmapImage(new Uri($"ms-appx:///{slot.PreviewImage}"));

                            var fadeInStoryboard = new Storyboard();
                            fadeInStoryboard.Children.Add(fadeInAnimation);
                            Storyboard.SetTarget(fadeInAnimation, image);
                            Storyboard.SetTargetProperty(fadeInAnimation, "Opacity");

                            fadeInStoryboard.Begin();
                        };

                        fadeOutStoryboard.Begin();

                        Data.SelectedTexture = slot;
                        if (_volumetricExperience != null && _volumetricExperience.Volume != null) _volumetricExperience.Volume.SetActiveEarcup(Data.SelectedTexture.Index);
                    }
                    else
                    {
                        toggle.IsChecked = false;
                    }
                }
            }
        }

        private void Accesory_Checked(object sender, RoutedEventArgs e)
        {
            var clickedToggle = sender as ToggleButton;

            foreach (var child in AccessoriesImageTogglePanel.Children)
            {
                if (child is ToggleButton toggle)
                {
                    var image = toggle.Content as Image;
                    var slot = toggle.Tag as AccessorySlot;

                    if (toggle == clickedToggle)
                    {
                        var fadeOutAnimation = new DoubleAnimation
                        {
                            From = 1.0,
                            To = 0.0,
                            Duration = new Duration(TimeSpan.FromMilliseconds(300))
                        };

                        var fadeInAnimation = new DoubleAnimation
                        {
                            From = 0.0,
                            To = 1.0,
                            Duration = new Duration(TimeSpan.FromMilliseconds(300))
                        };

                        var fadeOutStoryboard = new Storyboard();
                        fadeOutStoryboard.Children.Add(fadeOutAnimation);
                        Storyboard.SetTarget(fadeOutAnimation, image);
                        Storyboard.SetTargetProperty(fadeOutAnimation, "Opacity");

                        fadeOutStoryboard.Completed += (s, args) =>
                        {
                            image.Source = new BitmapImage(new Uri($"ms-appx:///{slot.SelectedImage}"));

                            var fadeInStoryboard = new Storyboard();
                            fadeInStoryboard.Children.Add(fadeInAnimation);
                            Storyboard.SetTarget(fadeInAnimation, image);
                            Storyboard.SetTargetProperty(fadeInAnimation, "Opacity");

                            fadeInStoryboard.Begin();
                        };

                        fadeOutStoryboard.Begin();

                        if (slot.Id == 1)
                        {
                            Data.SelectedAccesory = slot;
                            Accessory1Image.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                        }
                        if (slot.Id == 2)
                        {
                            Data.SelectedAccesory2 = slot;
                            Accessory2Image.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                        }
                        if (slot.Id == 3)
                        {
                            Data.SelectedAccesory3 = slot;
                            Accessory3Image.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                        }
                        if (_volumetricExperience != null && _volumetricExperience.Volume != null) _volumetricExperience.Volume.SetActiveAccesory(slot.Id - 1, true);
                    }
                }
            }
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            var toggle = sender as ToggleButton;
            var image = toggle.Content as Image;
            var slot = toggle.Tag as ImageSlot;

            if (Data.SelectedTexture == slot)
            {
                Data.SelectedTexture = null;
                HeadsetTexture.Source = null;
            }

            if (image != null && slot != null)
            {
                var fadeOutAnimation = new DoubleAnimation
                {
                    From = 1.0,
                    To = 0.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(300))
                };

                var fadeInAnimation = new DoubleAnimation
                {
                    From = 0.0,
                    To = 1.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(300))
                };

                var fadeOutStoryboard = new Storyboard();
                fadeOutStoryboard.Children.Add(fadeOutAnimation);
                Storyboard.SetTarget(fadeOutAnimation, image);
                Storyboard.SetTargetProperty(fadeOutAnimation, "Opacity");

                fadeOutStoryboard.Completed += (s, args) =>
                {
                    image.Source = new BitmapImage(new Uri("ms-appx:///" + slot.OriginalImage));

                    var fadeInStoryboard = new Storyboard();
                    fadeInStoryboard.Children.Add(fadeInAnimation);
                    Storyboard.SetTarget(fadeInAnimation, image);
                    Storyboard.SetTargetProperty(fadeInAnimation, "Opacity");

                    fadeInStoryboard.Begin();
                };

                fadeOutStoryboard.Begin();
            }
        }

        private void Accesory_Unchecked(object sender, RoutedEventArgs e)
        {
            var toggle = sender as ToggleButton;

            var image = toggle.Content as Image;
            var slot = toggle.Tag as AccessorySlot;

            if (slot.Id == 1)
            {
                Accessory1Image.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            }
            if (slot.Id == 2)
            {
                Accessory2Image.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            }
            if (slot.Id == 3)
            {
                Accessory3Image.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            }

            if (_volumetricExperience != null && _volumetricExperience.Volume != null) _volumetricExperience.Volume.SetActiveAccesory(slot.Id - 1, false);

            if (Data.SelectedAccesory == slot)
            {
                Data.SelectedAccesory = null;
            }
            if (Data.SelectedAccesory2 == slot)
            {
                Data.SelectedAccesory2 = null;
            }
            if (Data.SelectedAccesory3 == slot)
            {
                Data.SelectedAccesory3 = null;
            }

            if (image != null && slot != null)
            {
                var fadeOutAnimation = new DoubleAnimation
                {
                    From = 1.0,
                    To = 0.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(300))
                };

                var fadeInAnimation = new DoubleAnimation
                {
                    From = 0.0,
                    To = 1.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(300))
                };

                // Fade in animation
                var fadeOutStoryboard = new Storyboard();
                fadeOutStoryboard.Children.Add(fadeOutAnimation);
                Storyboard.SetTarget(fadeOutAnimation, image);
                Storyboard.SetTargetProperty(fadeOutAnimation, "Opacity");

                fadeOutStoryboard.Completed += (s, args) =>
                {
                    image.Source = new BitmapImage(new Uri("ms-appx:///" + slot.OriginalImage));

                    var fadeInStoryboard = new Storyboard();
                    fadeInStoryboard.Children.Add(fadeInAnimation);
                    Storyboard.SetTarget(fadeInAnimation, image);
                    Storyboard.SetTargetProperty(fadeInAnimation, "Opacity");

                    fadeInStoryboard.Begin();
                };

                fadeOutStoryboard.Begin();
            }
        }

        private void ApplyTintEffect(Image imageElement, Color tintColor, ref bool isTinting, ref DateTime lastTitnTime)
        {
            TimeSpan timeSinceLastColorPick = DateTime.Now - lastTitnTime;
            if (isTinting || timeSinceLastColorPick.TotalSeconds < _tintRegulatorTime) return;
            isTinting = true;
            lastTitnTime = DateTime.Now;

            var compositor = ElementCompositionPreview.GetElementVisual(imageElement).Compositor;

            if (imageElement.Source is BitmapImage bitmapImage && bitmapImage.UriSource is not null)
            {
                var uri = bitmapImage.UriSource;
                var surface = LoadedImageSurface.StartLoadFromUri(uri);

                surface.LoadCompleted += (s, e) =>
                {
                    var surfaceBrush = compositor.CreateSurfaceBrush(surface);
                    surfaceBrush.Stretch = Microsoft.UI.Composition.CompositionStretch.Uniform;

                    var effect = new CompositeEffect
                    {
                        Mode = CanvasComposite.DestinationIn,
                        Sources =
                        {
                            new BlendEffect
                            {
                                Mode = BlendEffectMode.Multiply,
                                Background = new Microsoft.UI.Composition.CompositionEffectSourceParameter("ImageSource"),
                                Foreground = new ColorSourceEffect
                                {
                                    Name = "Tint",
                                    Color = tintColor
                                }
                            },
                            new Microsoft.UI.Composition.CompositionEffectSourceParameter("ImageSource")
                        }
                    };

                    var effectFactory = compositor.CreateEffectFactory(effect, new[] { "Tint.Color" });
                    var effectBrush = effectFactory.CreateBrush();
                    effectBrush.SetSourceParameter("ImageSource", surfaceBrush);

                    var spriteVisual = compositor.CreateSpriteVisual();
                    spriteVisual.Brush = effectBrush;

                    ElementCompositionPreview.SetElementChildVisual(imageElement, spriteVisual);
                    spriteVisual.Size = new System.Numerics.Vector2(
                        (float)imageElement.ActualWidth,
                        (float)imageElement.ActualHeight);
                };
                isTinting = false;
            }
            else
            {
                Debug.WriteLine("Image URI not valid");
                isTinting = false;
            }
        }
    }
}
