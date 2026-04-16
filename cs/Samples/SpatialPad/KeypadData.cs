using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Volumetric.Samples.SpatialPad
{
    public class ShortcutID
    {
        public string Name { get; set; }
        public string App { get; set; }
        public int Index { get; set; }
        public ShortcutID(string name, string app, int index)
        {
            Name = name;
            App = app;
            Index = index;
        }
        public override string ToString()
        {
            return $"{Name} ({App})";
        }
    }
    public class KeyPadConfigData
    {
        public List<ShortcutID> Shortcuts { get; set; } = new List<ShortcutID>();
        public bool DarkMode { get; set; } = false;
    }
    public class KeypadData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int Index;
        public ObservableCollection<Slot> Slots = new();
        public Slot SelectedSlot = null!;
        public bool IsSelected => App.CurrentKeypadId == Index;

        private bool _darkMode;
        private SettingsManager<KeyPadConfigData> config;
        public bool DarkMode
        {
            get => _darkMode;
            set
            {
                if (_darkMode != value)
                {
                    _darkMode = value;
                    OnPropertyChanged(nameof(DarkMode));

                    if (App.GetCurrentKeypad() != null) App.GetCurrentKeypad().SaveData();
                }
            }
        }
        public KeypadData(int index)
        {
            Index = index;
            config = new SettingsManager<KeyPadConfigData>("SpatialPad", $"KeyPad_{index}_Config.json");
            LoadData();
        }
        private void loadDefaultSlots()
        {
            Slots = new ObservableCollection<Slot>();

            if (config.Settings.Shortcuts.Count == 9)
            {
                for (int i = 0; i < 9; i++)
                {
                    var shortcutID = config.Settings.Shortcuts[i];
                    var shortcut = ShortcutsManager.Shortcuts[shortcutID.Index];
                    if (shortcutID.Index < 0 || shortcutID.Index >= ShortcutsManager.Shortcuts.Count)
                    {
                        shortcut = ShortcutsManager.Shortcuts[0];
                    }
                    Slots.Add(new Slot(i, shortcut));
                }
            }
            else
            {
                var random = new Random();
                int shortcutCount = ShortcutsManager.Shortcuts.Count;

                for (int i = 0; i < 9; i++)
                {
                    var ixShortcut = random.Next(shortcutCount);
                    var randomShortcut = ShortcutsManager.Shortcuts[ixShortcut];
                    Slots.Add(new Slot(i, randomShortcut));
                }
            }

            SelectedSlot = Slots[0];
            SaveData();
        }

        public void SaveData()
        {
            config.Settings.Shortcuts.Clear();
            foreach (var slot in Slots)
            {
                config.Settings.Shortcuts.Add(new ShortcutID(slot.Shortcut.ButtonTypeData.ToString(), slot.Shortcut.App.ToString(), ShortcutsManager.Shortcuts.IndexOf(slot.Shortcut)));
            }
            config.Settings.DarkMode = DarkMode;
            config.Save();
        }
        public void LoadData()
        {
            DarkMode = config.Settings.DarkMode;

            Slots = new ObservableCollection<Slot>();

            if (config.Settings.Shortcuts.Count == 9)
            {
                for (int i = 0; i < 9; i++)
                {

                    var shortcutID = config.Settings.Shortcuts[i];
                    var shortcut = ShortcutsManager.Shortcuts[shortcutID.Index];
                    if (shortcutID.Index < 0 || shortcutID.Index >= ShortcutsManager.Shortcuts.Count)
                    {
                        shortcut = ShortcutsManager.Shortcuts[0];
                    }
                    Slots.Add(new Slot(i, shortcut));
                }
            }
            else
            {
                loadDefaultSlots();
            }

            SelectedSlot = Slots[0];
        }

    }
}
