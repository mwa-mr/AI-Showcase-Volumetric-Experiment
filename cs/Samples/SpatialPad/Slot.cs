using System.ComponentModel;

namespace Volumetric.Samples.SpatialPad
{
    // 2D Slot
    public class Slot : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int Index;
        public VolumetricSlot? VolumetricSlot;

        private Shortcut _shortcut = null!;

        public Shortcut Shortcut
        {
            get => _shortcut;
            set
            {
                if (_shortcut != value)
                {
                    _shortcut = value;
                    if (VolumetricSlot != null) VolumetricSlot.UpdateModel();
                    OnPropertyChanged(nameof(Shortcut));
                    OnPropertyChanged(nameof(HasShortcut));
                    OnPropertyChanged(nameof(AppImagePath));

                    OnPropertyChanged(nameof(ButtonImageNormal));
                    OnPropertyChanged(nameof(ButtonImageShadow));
                    OnPropertyChanged(nameof(ButtonImageHoverShadow));
                    if (App.GetCurrentKeypad() != null) App.GetCurrentKeypad().SaveData();
                }
            }
        }

        public string AppImagePath
        {
            get
            {
                return $"/Assets/Images/app_{Shortcut.App}.png";
            }
        }
        public bool HasShortcut => Shortcut != null && Shortcut.App != ShortcutApp.none;

        public string ButtonImageNormal
        {
            get
            {
                if (Data.ButtonsData.ContainsKey(Shortcut.ButtonTypeData))
                {
                    var imageUrl = Data.ButtonsData[Shortcut.ButtonTypeData].NormalImage;
                    return imageUrl;
                }
                else
                {
                    return "/Assets/Images/slot.png";
                }
            }
        }

        public string ButtonImageShadow
        {
            get
            {
                if (Shortcut.App != ShortcutApp.none && Data.ButtonsData.ContainsKey(Shortcut.ButtonTypeData))
                {
                    return Data.ButtonsData[Shortcut.ButtonTypeData].ShadowImage;
                }
                else
                    return "/Assets/Images/slot_shadow.png";
            }
        }

        public string ButtonImageHoverShadow
        {
            get
            {
                if (Shortcut.App != ShortcutApp.none && Data.ButtonsData.ContainsKey(Shortcut.ButtonTypeData))
                {
                    return Data.ButtonsData[Shortcut.ButtonTypeData].ShadowHoverImage;
                }
                else
                    return "/Assets/Images/slot_hover.png";
            }
        }

        private string _currentShadowImage = null!;
        public string CurrentShadowImage
        {
            get => _currentShadowImage;
            set
            {
                if (_currentShadowImage != value)
                {
                    _currentShadowImage = value;
                    OnPropertyChanged(nameof(CurrentShadowImage));
                }
            }
        }

        public Slot(int index, Shortcut shortcut)
        {
            Index = index;
            Shortcut = shortcut;
            CurrentShadowImage = ButtonImageShadow;
        }
    }
}
