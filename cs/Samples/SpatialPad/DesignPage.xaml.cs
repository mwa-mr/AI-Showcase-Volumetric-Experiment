using Microsoft.MixedReality.Volumetric;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WinRT.Interop;
using Point = Windows.Foundation.Point;

namespace Volumetric.Samples.SpatialPad
{
    public sealed partial class DesignPage : Page
    {
        private VolumetricExperience? _volumetricExperience;
        private KeypadData _keypadData;
        private ObservableCollection<KeypadData> _keypads = new();
        private bool _hasAutoDeployed = false;

        public double PopupWidth { get; set; }
        public double PopupHeight { get; set; }

        private bool _popupIsOpen = false;
        double popupWidth = 200;
        double popupHeight = 200;

        double colorPopupWidth = 280;
        double colorPopupHeight = 44;

        double shapePopupWidth = 340;
        double shapePopupHeight = 340;

        double slotWidth = 150;
        double slotHeight = 150;
        public double SlotWidth
        {
            get { return slotWidth; }
            set { slotWidth = value; }
        }

        public DesignPage()
        {
            this.InitializeComponent();
            Loaded += OnLoaded;

            _keypadData = App.GetCurrentKeypad();
            _keypads = new ObservableCollection<KeypadData>(App.GetKeypads());

            PopupBorder.Width = popupWidth;
            PopupBorder.Height = popupHeight;

            ColorPopupBorder.Width = colorPopupWidth;
            ColorPopupBorder.Height = colorPopupHeight;
            ColorPopupBorder.CornerRadius = new CornerRadius(colorPopupHeight / 2, colorPopupHeight / 2, colorPopupHeight / 2, colorPopupHeight / 2);

            ShapePopupBorder.Width = shapePopupWidth;
            ShapePopupBorder.Height = shapePopupHeight;
            ShapePopupBorder.CornerRadius = new CornerRadius(16, 16, 16, 16);
            ShapePopupBorder.Margin = new Thickness(0, 20, 0, 0);

            var selectionColors = new List<SolidColorBrush>();

            SelectionGrid.ItemsSource = selectionColors;

            ShortcutSelectionGrid.ItemsSource = ShortcutsManager.ShortcutAppVisuals;

            LoadShapeChoices();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            TryAutoDeploy();
        }

        private void TryAutoDeploy()
        {
            if (_hasAutoDeployed)
            {
                return;
            }

            if (!App.AutoDeployRequested)
            {
                return;
            }

            _hasAutoDeployed = true;
            deployButton_Pressed(deployButton, new RoutedEventArgs());
        }

        private void LoadShapeChoices()
        {
            var shapes = new List<ButtonChoice>();

            foreach (var kvp in Data.ButtonsData)
            {
                shapes.Add(new ButtonChoice(kvp.Key));
            }
            SelectionShape.ItemsSource = shapes;
        }

        private async void AppSelector_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is ShortcutAppVisual selectedAppVisual)
            {
                ShortcutApp selectedApp = selectedAppVisual.App;
                var filteredShortcuts = ShortcutsManager.GetShortcutsByApp(selectedApp);
                ShortcutsListView.ItemsSource = filteredShortcuts;

                foreach (var item in ShortcutSelectionGrid.Items)
                {
                    var container = ShortcutSelectionGrid.ContainerFromItem(item);
                    if (container != null)
                    {
                        var appButton = FindChild<Button>(container);
                        if (appButton != null && appButton.Tag is ShortcutApp appButtonTag)
                        {
                            var tag = appButton.Tag;
                            var visual = ShortcutsManager.ShortcutAppVisuals.FirstOrDefault(v => v.App == appButtonTag);

                            if (appButtonTag == selectedApp) // Fix: Use == operator for comparison
                            {
                                appButton.Background = visual?.ColorBrush ?? new SolidColorBrush(Microsoft.UI.Colors.LightGray);
                            }
                            else
                            {
                                appButton.Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(10, 0, 0, 0));
                            }
                        }
                    }
                }

                await Task.Delay(50);

                var shortcutSelected = _keypadData.SelectedSlot.Shortcut;

                foreach (var item in ShortcutsListView.Items)
                {
                    if (item is Shortcut shortcut && shortcut.Description == shortcutSelected.Description)
                    {
                        ShortcutsListView.SelectedItem = item;

                        var container = ShortcutsListView.ContainerFromItem(item) as ListViewItem;
                        if (container != null)
                        {
                            container.IsSelected = true;
                            container.Focus(FocusState.Programmatic);
                        }
                        break;
                    }
                }
            }
            else
            {
                var contextType = sender is FrameworkElement fe ? fe.DataContext?.GetType().Name : "null";
                Debug.WriteLine($"Invalid DataContext: {contextType}");
            }
        }

        private void Border_Loaded(object sender, RoutedEventArgs e)
        {
            var border = sender as Border;
            if (border != null)
            {
                border.Width = slotWidth;
                border.Height = slotHeight;
            }
        }

        private void PositionPopup(FrameworkElement sender)
        {
            if (PopupWindow == null || PopupWindow.Child == null)
                return;

            var windowRoot = (FrameworkElement)this;
            var stackposition = RootStack.TransformToVisual(windowRoot)
                        .TransformPoint(new Point(0, 0));

            var rectangle = sender as Border;

            if (rectangle != null)
            {
                var position = rectangle.TransformToVisual(RootStack).TransformPoint(new Windows.Foundation.Point(0, 0));

                var popupposition = PopupWindow.TransformToVisual(windowRoot)
                        .TransformPoint(new Point(0, 0));

                PopupWindow.HorizontalOffset = -popupposition.X + stackposition.X + position.X - popupWidth / 2 + rectangle.Width / 2;
                PopupWindow.VerticalOffset = -popupposition.Y + stackposition.Y + position.Y - popupHeight / 2 + rectangle.Height / 2;
            }
        }

        private void PositionShortcutPopup(FrameworkElement sender)
        {
            if (SelectionPopup == null || SelectionPopup.Child == null)
                return;

            var button = sender as FrameworkElement;

            if (button == null) return;

            var transform = button.TransformToVisual(null);
            var point = transform.TransformPoint(new Point(0, 0));

            var hwnd = WindowNative.GetWindowHandle(App.m_window);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);

            double m_windowHeight = appWindow.Size.Height;

            double popupUpdatedPosition = m_windowHeight - SelectionPopup.ActualHeight;

            double offsetX = point.X + 160;
            double offsetY = point.Y;

            switch (point.Y)
            {
                case < 200:
                    offsetY = point.Y - 50;
                    break;
                case < 350:
                    offsetY = point.Y - 100;
                    break;
                default:
                    offsetY = point.Y - 150;
                    break;
            }

            ShortcutPopup.HorizontalOffset = offsetX;
            ShortcutPopup.VerticalOffset = offsetY;
        }

        private async void Slot_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (_popupIsOpen)
            {
                e.Handled = true;
                return;
            }

            if (sender is Border border && border.DataContext is Slot slot)
            {
                _keypadData.SelectedSlot = slot;
            }

            if (sender is FrameworkElement element)
            {
                PositionPopup(element);
                PositionShortcutPopup(element);
            }

            if (_keypadData.SelectedSlot.Shortcut != null && _keypadData.SelectedSlot.Shortcut.App != ShortcutApp.none)
            {
                LeftButton.IsEnabled = false;
            }
            else
            {
                LeftButton.IsEnabled = false;
            }

            ShowShortcutPopup();

            await AnimateButtonsAsync();
            OptionsPopupOpen();
        }

        private void SlotBorder_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Border border && border.DataContext is Slot slot)
            {
                _keypadData.SelectedSlot = slot;

                var grid = (Grid)VisualTreeHelper.GetChild(border, 0);
                var buttonImage = (Image)grid.FindName("ButtonImage");
                var shadowImage = (Image)grid.FindName("ShadowImage");
                var hoverImage = (Image)grid.FindName("HoverShadowImage");
                var plusButton = (Button)grid.FindName("AddButton");

                if (shadowImage != null && hoverImage != null)
                {
                    var fadeIn = new DoubleAnimation
                    {
                        To = 1,
                        Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                        EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                    };

                    var fadeOut = new DoubleAnimation
                    {
                        To = 0,
                        Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                        EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                    };

                    var fadeNormOut = new DoubleAnimation
                    {
                        To = 0,
                        Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                        EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                    };

                    var storyboard = new Storyboard();

                    Storyboard.SetTarget(fadeIn, hoverImage);
                    Storyboard.SetTargetProperty(fadeIn, "Opacity");

                    Storyboard.SetTarget(fadeOut, shadowImage);
                    Storyboard.SetTargetProperty(fadeOut, "Opacity");

                    if (_keypadData.SelectedSlot.Shortcut.App == ShortcutApp.none)
                    {
                        Storyboard.SetTarget(fadeNormOut, buttonImage);
                        Storyboard.SetTargetProperty(fadeNormOut, "Opacity");
                        storyboard.Children.Add(fadeNormOut);

                        var scaleXAnim = new DoubleAnimation
                        {
                            To = 0.8,
                            Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                        };
                        Storyboard.SetTarget(scaleXAnim, buttonImage.RenderTransform);
                        Storyboard.SetTargetProperty(scaleXAnim, "ScaleX");
                        storyboard.Children.Add(scaleXAnim);

                        var scaleYAnim = new DoubleAnimation
                        {
                            To = 0.8,
                            Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                        };
                        Storyboard.SetTarget(scaleYAnim, buttonImage.RenderTransform);
                        Storyboard.SetTargetProperty(scaleYAnim, "ScaleY");
                        storyboard.Children.Add(scaleYAnim);

                        var scaleXAnimHover = new DoubleAnimation
                        {
                            From = 0.8,
                            To = 1,
                            Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                        };

                        Storyboard.SetTarget(scaleXAnimHover, hoverImage.RenderTransform);
                        Storyboard.SetTargetProperty(scaleXAnimHover, "ScaleX");
                        storyboard.Children.Add(scaleXAnimHover);

                        var scaleYAnimHover = new DoubleAnimation
                        {
                            From = 0.8,
                            To = 1,
                            Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                        };
                        Storyboard.SetTarget(scaleYAnimHover, hoverImage.RenderTransform);
                        Storyboard.SetTargetProperty(scaleYAnimHover, "ScaleY");
                        storyboard.Children.Add(scaleYAnimHover);

                        plusButton.Visibility = Visibility.Visible;
                        var fadePlusIn = new DoubleAnimation
                        {
                            To = 1,
                            Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                        };
                        Storyboard.SetTarget(fadePlusIn, plusButton);
                        Storyboard.SetTargetProperty(fadePlusIn, "Opacity");
                        storyboard.Children.Add(fadePlusIn);
                    }

                    storyboard.Children.Add(fadeIn);
                    storyboard.Children.Add(fadeOut);
                    storyboard.Begin();
                }
            }
        }

        private void SlotBorder_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Border border && border.DataContext is Slot slot)
            {
                var grid = (Grid)VisualTreeHelper.GetChild(border, 0);
                var buttonImage = (Image)grid.FindName("ButtonImage");
                var shadowImage = (Image)grid.FindName("ShadowImage");
                var hoverImage = (Image)grid.FindName("HoverShadowImage");
                var plusButton = (Button)grid.FindName("AddButton");

                if (shadowImage != null && hoverImage != null)
                {
                    var fadeOut = new DoubleAnimation
                    {
                        To = 0,
                        Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                        EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                    };

                    var fadeIn = new DoubleAnimation
                    {
                        To = 1,
                        Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                        EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                    };

                    var fadeNormIn = new DoubleAnimation
                    {
                        To = 1,
                        Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                        EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                    };

                    var storyboard = new Storyboard();

                    Storyboard.SetTarget(fadeOut, hoverImage);
                    Storyboard.SetTargetProperty(fadeOut, "Opacity");

                    Storyboard.SetTarget(fadeIn, shadowImage);
                    Storyboard.SetTargetProperty(fadeIn, "Opacity");

                    if (_keypadData.SelectedSlot.Shortcut.App == ShortcutApp.none)
                    {
                        Storyboard.SetTarget(fadeNormIn, buttonImage);
                        Storyboard.SetTargetProperty(fadeNormIn, "Opacity");
                        storyboard.Children.Add(fadeNormIn);

                        var scaleXAnim = new DoubleAnimation
                        {
                            To = 1,
                            Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                        };

                        Storyboard.SetTarget(scaleXAnim, buttonImage.RenderTransform);
                        Storyboard.SetTargetProperty(scaleXAnim, "ScaleX");
                        storyboard.Children.Add(scaleXAnim);

                        var scaleYAnim = new DoubleAnimation
                        {
                            To = 1,
                            Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                        };
                        Storyboard.SetTarget(scaleYAnim, buttonImage.RenderTransform);
                        Storyboard.SetTargetProperty(scaleYAnim, "ScaleY");
                        storyboard.Children.Add(scaleYAnim);

                        var scaleXAnimHover = new DoubleAnimation
                        {
                            To = 1,
                            Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                        };

                        Storyboard.SetTarget(scaleXAnimHover, hoverImage.RenderTransform);
                        Storyboard.SetTargetProperty(scaleXAnimHover, "ScaleX");
                        storyboard.Children.Add(scaleXAnimHover);

                        var scaleYAnimHover = new DoubleAnimation
                        {
                            To = 1,
                            Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                        };
                        Storyboard.SetTarget(scaleYAnimHover, hoverImage.RenderTransform);
                        Storyboard.SetTargetProperty(scaleYAnimHover, "ScaleY");
                        storyboard.Children.Add(scaleYAnimHover);

                        var fadePlusOut = new DoubleAnimation
                        {
                            To = 0,
                            Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                        };
                        Storyboard.SetTarget(fadePlusOut, plusButton);
                        Storyboard.SetTargetProperty(fadePlusOut, "Opacity");
                        storyboard.Children.Add(fadePlusOut);
                    }

                    storyboard.Children.Add(fadeOut);
                    storyboard.Children.Add(fadeIn);
                    storyboard.Begin();
                }
            }
        }

        private void TopButton_Click(object sender, RoutedEventArgs e)
        {
            _keypadData.SelectedSlot.Shortcut = ShortcutsManager.Shortcuts[0];

            OptionsPopupClose();
            UpdateButtonsVisualization();
        }

        private void UpdateButtonsVisualization()
        {
        }

        private void OptionsPopupOpen()
        {
            PopupWindow.IsOpen = true;
            _popupIsOpen = true;

            SelectionPopup_Opened();

            Overlay.Visibility = Visibility.Visible;
            Overlay.IsHitTestVisible = true;
        }

        private void OptionsPopupClose()
        {
            PopupWindow.IsOpen = false;
            _popupIsOpen = false;

            Overlay.Visibility = Visibility.Collapsed;
            Overlay.IsHitTestVisible = false;

            HideShortcutPopup();
        }

        private async void Overlay_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _ = AnimateButtonsOutAsync();
            await Task.Delay(100);
            OptionsPopupClose();
        }

        private void OptionsPopup_Dismiss_Closed(object sender, object e)
        {
            OptionsPopupClose();
        }
        private void LeftButton_Click(object sender, RoutedEventArgs e)
        {
            SelectionPopup_Opened();
            ShowSelectionPopup(sender, e);
        }
        private void RightButton_Click(object sender, RoutedEventArgs e)
        {
            SelectionPopup_Opened();
            ShowShortcutPopup();
        }

        private void UpdatePopupSelections()
        {
            foreach (var item in SelectionGrid.Items)
            {
                var container = SelectionGrid.ContainerFromItem(item) as GridViewItem;

                if (container != null)
                {
                    var button = FindChild<Button>(container);
                    if (button != null)
                    {
                        var buttonColorBrush = button.Background as SolidColorBrush;
                    }
                }
            }

            var keypadButton = _keypadData.SelectedSlot.Shortcut.ButtonTypeData;

            foreach (var item in SelectionShape.Items)
            {
                if (item is ButtonChoice choice)
                {
                    var container = SelectionShape.ContainerFromItem(choice) as ContentPresenter;
                    if (container != null)
                    {
                        var button = FindChild<Button>(container);
                        if (button != null)
                        {
                            if (choice.Type == keypadButton)
                            {
                                button.BorderBrush = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(50, 0x99, 0x99, 0x99));

                                choice.IsSelected = true;
                            }
                            else
                            {
                                button.BorderBrush = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(0, 0x99, 0x99, 0x99));

                                choice.IsSelected = false;
                            }

                        }
                    }
                }
            }
        }

        private void UpdateShortcutPopupSelections()
        {
            var filteredShortcuts = ShortcutsManager.GetShortcutsByApp(_keypadData.SelectedSlot.Shortcut.App);
            ShortcutsListView.ItemsSource = filteredShortcuts;
            ShortcutsListView.UpdateLayout();

            var shortcutSelected = _keypadData.SelectedSlot.Shortcut;

            foreach (var item in ShortcutSelectionGrid.Items)
            {
                var container = ShortcutSelectionGrid.ContainerFromItem(item);

                if (container != null)
                {
                    var button = FindChild<Button>(container);
                    if (button != null)
                    {
                        var tag = button.Tag;

                        if (tag != null && button.Tag is ShortcutApp appTag)
                        {
                            var visual = ShortcutsManager.ShortcutAppVisuals.FirstOrDefault(v => v.App == appTag);

                            if (appTag == shortcutSelected.App)
                            {
                                button.Background = visual?.ColorBrush ?? new SolidColorBrush(Microsoft.UI.Colors.LightGray);

                            }
                            else
                            {
                                button.Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(10, 0, 0, 0));
                            }
                        }
                    }
                }
            }

            ShortcutsListView.DispatcherQueue.TryEnqueue(() =>
            {
                foreach (var item in ShortcutsListView.Items)
                {
                    var container = ShortcutsListView.ContainerFromItem(item) as ListViewItem;

                    if (container != null)
                    {

                        var textBlock = FindChild<TextBlock>(container);

                        if (textBlock != null && textBlock.Tag is string tag)
                        {
                            var border = FindParent<Border>(textBlock);
                            if (border != null)
                            {
                                if (tag == shortcutSelected.Description)
                                {
                                    ShortcutsListView.SelectedItem = item;
                                }
                            }
                        }
                    }
                }
            });
        }

        private void ShortcutsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Shortcut clickedShortcut)
            {
                if (_keypadData.SelectedSlot != null)
                {
                    if (clickedShortcut != null)
                    {
                        _keypadData.SelectedSlot.Shortcut = clickedShortcut;
                        UpdateShortcutPopupSelections();
                    }
                }
            }
        }
        private void OnShapeSelected(object sender, RoutedEventArgs e)
        {
        }

        private void SetNewColor(object sender, RoutedEventArgs e)
        {
        }

        private void SelectionPopup_Opened()
        {

            TopButton.IsEnabled = true;
            LeftButton.IsEnabled = false;
            RightButton.IsEnabled = false;

            if (_keypadData.SelectedSlot.Shortcut != null && _keypadData.SelectedSlot.Shortcut.App != ShortcutApp.none)
            {
                TopButton.IsEnabled = true;
                LeftButton.IsEnabled = false;
            }
        }

        private void SelectionPopup_Closed()
        {
            TopButton.IsEnabled = true;
            LeftButton.IsEnabled = false;
            RightButton.IsEnabled = false;

            if (_keypadData.SelectedSlot.Shortcut != null && _keypadData.SelectedSlot.Shortcut.App != ShortcutApp.none)
            {
            }
            else
            {
                TopButton.IsEnabled = false;
                LeftButton.IsEnabled = false;
            }
        }

        private void ShortcutPopup_Dismiss_Closed(object sender, object e)
        {
        }

        private void SelectionPopup_Dismiss_Closed(object sender, object e)
        {
            SelectionPopup_Closed();
        }

        private void PopupBackground_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SelectionPopup_Closed();
        }

        private void ShowSelectionPopup(object sender, RoutedEventArgs e)
        {
            var button = sender as FrameworkElement;

            if (button == null) return;

            var transform = button.TransformToVisual(null);
            var point = transform.TransformPoint(new Point(0, 0));

            var hwnd = WindowNative.GetWindowHandle(App.m_window);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);

            double m_windowHeight = appWindow.Size.Height;

            double popupUpdatedPosition = m_windowHeight - SelectionPopup.ActualHeight;

            double offsetX = point.X - shapePopupWidth - 20;
            double offsetY = point.Y - popupUpdatedPosition / 2;

            switch (point.Y)
            {
                case < 200:
                    offsetY = point.Y - 100;
                    break;
                case < 350:
                    offsetY = point.Y - 200;
                    break;
                default:
                    offsetY = point.Y - 230;
                    break;
            }

            SelectionPopup.HorizontalOffset = offsetX;
            SelectionPopup.VerticalOffset = offsetY;

            SelectionPopup.IsOpen = true;

            DispatcherQueue.TryEnqueue(() =>
            {
                Task.Delay(100);

                UpdatePopupSelections();
            });
        }

        private void ShowShortcutPopup()
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                UpdateShortcutPopupSelections();
            });

            ShortcutPopup.IsOpen = true;
            PopupContentRoot.Opacity = 0;
            ShortcutPopup.Visibility = Visibility.Visible;

            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(250)
            };

            var storyboard = new Storyboard();
            Storyboard.SetTarget(fadeIn, PopupContentRoot);
            Storyboard.SetTargetProperty(fadeIn, "Opacity");
            storyboard.Children.Add(fadeIn);
            storyboard.Begin();
        }

        private void HideShortcutPopup()
        {
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200)
            };

            var storyboard = new Storyboard();
            Storyboard.SetTarget(fadeOut, PopupContentRoot);
            Storyboard.SetTargetProperty(fadeOut, "Opacity");
            storyboard.Children.Add(fadeOut);

            storyboard.Completed += (s, e) =>
            {
                ShortcutPopup.IsOpen = false;
            };

            storyboard.Begin();
        }

        private void HideSelectionPopup()
        {
            SelectionPopup.IsOpen = false;
        }

        private T? FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t)
                    return t;
                var result = FindChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        public static T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);
            while (parent != null)
            {
                if (parent is T typedParent)
                    return typedParent;

                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        public static T? FindElementByName<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T element && element.Name == name)
                    return element;

                var result = FindElementByName<T>(child, name);
                if (result != null)
                    return result;
            }
            return null;
        }

        private void SelectionGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is string selectedValue)
            {
                SelectionPopup.IsOpen = false;
                PopupWindow.IsOpen = false;
            }
        }

        private Task AnimateButtonsAsync()
        {
            const int animationDistance = 20;
            const int animationDurationMs = 300;

            TopButton.Opacity = 0;
            LeftButton.Opacity = 0;
            RightButton.Opacity = 0;

            TopButtonTransform.Y = 0;

            var storyboard = new Storyboard();

            var topOpacityAnim = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(animationDurationMs)
            };
            Storyboard.SetTarget(topOpacityAnim, TopButton);
            Storyboard.SetTargetProperty(topOpacityAnim, "Opacity");

            var topMoveAnim = new DoubleAnimation
            {
                To = animationDistance,
                Duration = TimeSpan.FromMilliseconds(animationDurationMs)
            };
            Storyboard.SetTarget(topMoveAnim, TopButtonTransform);
            Storyboard.SetTargetProperty(topMoveAnim, "Y");

            storyboard.Children.Add(topOpacityAnim);
            storyboard.Children.Add(topMoveAnim);

            storyboard.Begin();
            return Task.CompletedTask;
        }

        private Task AnimateButtonsOutAsync()
        {
            const int animationDurationMs = 100;

            var storyboard = new Storyboard();

            var topOpacityAnim = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(animationDurationMs)
            };
            Storyboard.SetTarget(topOpacityAnim, TopButton);
            Storyboard.SetTargetProperty(topOpacityAnim, "Opacity");

            var topMoveAnim = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(animationDurationMs)
            };
            Storyboard.SetTarget(topMoveAnim, TopButtonTransform);
            Storyboard.SetTargetProperty(topMoveAnim, "Y");

            storyboard.Children.Add(topOpacityAnim);
            storyboard.Children.Add(topMoveAnim);

            storyboard.Begin();
            return Task.CompletedTask;
        }

        private void deployButton_Pressed(object sender, RoutedEventArgs e)
        {
            if (_volumetricExperience == null)
            {
                _volumetricExperience = new VolumetricExperience("Spatial Pad", this);
            }
            else
            {
                _volumetricExperience.CreateSpatialPad(App.GetCurrentKeypad());
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

        private void LightPadToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (_volumetricExperience != null) _volumetricExperience.PadVolumes[App.GetCurrentKeypad().Index]?.SwitchDarkLightPad();

        }

        private DispatcherTimer? _scrollTimer;
        private ScrollViewer? _scrollViewer;
        private const double ScrollStep = 1.0;
        private const int ScrollIntervalMs = 20;

        private void Label_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Border border)
            {
                _scrollViewer = border.FindName("TextScrollViewer") as ScrollViewer;
                if (_scrollViewer == null)
                    return;

                if (_scrollTimer == null)
                {
                    _scrollTimer = new DispatcherTimer
                    {
                        Interval = TimeSpan.FromMilliseconds(ScrollIntervalMs)
                    };
                    _scrollTimer.Tick += ScrollTimer_Tick;
                    _scrollTimer.Start();
                }
            }
        }

        private void Label_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            _scrollTimer?.Stop();
            _scrollTimer = null;

            if (_scrollViewer != null)
            {
                _scrollViewer.ChangeView(0, null, null, true);
                _scrollViewer = null;
            }
        }

        private void ScrollTimer_Tick(object? sender, object e)
        {
            if (_scrollViewer == null) return;

            double maxOffset = _scrollViewer.ScrollableWidth;
            if (maxOffset <= 0) return;

            double newOffset = _scrollViewer.HorizontalOffset + ScrollStep;

            _scrollViewer.ChangeView(newOffset, null, null, true);
        }

        private void ChangeKeypadButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag?.ToString(), out int id))
            {
                App.SetCurrentKeypad(id);
                _keypadData = App.GetCurrentKeypad();

                if (_volumetricExperience != null) DeployButtonState(_volumetricExperience.IsVolumeOpen(id) ? "disabled" : "enabled");

                SlotGrid.ItemsSource = null;
                SlotGrid.ItemsSource = _keypadData.Slots;

                KeypadButtons.ItemsSource = null;
                KeypadButtons.ItemsSource = _keypads;

                LightPadToggle.IsOn = _keypadData.DarkMode;
            }
        }
    }

    public class ButtonChoice
    {
        public Data.ButtonType Type { get; set; }
        public string ImageUri { get; set; } = String.Empty;
        public string NormalImage { get; set; }
        public string ShadowImage { get; set; }
        private bool _isSelected;

        public ButtonChoice(Data.ButtonType type)
        {
            Type = type;
            NormalImage = VolumetricApp.GetAssetUri(Data.ButtonsData[type].NormalImage);
            ShadowImage = VolumetricApp.GetAssetUri(Data.ButtonsData[type].ShadowImage);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
                }
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
