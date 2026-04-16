using System;
using System.Collections.Generic;
using System.Diagnostics;
using WindowsInput;
using WindowsInput.Native;
using System.Runtime.InteropServices;
using System.Linq;
using System.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using Windows.UI;
using static Volumetric.Samples.SpatialPad.Data;

namespace Volumetric.Samples.SpatialPad
{
    public enum ShortcutApp
    {
        none,
        windows,
        word,
        excel,
        powertoys,
        teams,
    }

    public enum Key
    {
        Win,
        Ctrl,
        Alt,
        Shift,
        Tab,
        Enter,
        Space,
        Esc,
        A,
        B,
        C,
        D,
        E,
        F,
        H,
        I,
        K,
        L,
        M,
        N,
        O,
        P,
        R,
        S,
        T,
        V,
        W,
        F4,
        F5,
        [Description(";")]
        semicolon,
        [Description(".")]
        period,
        [Description(",")]
        comma,
        [Description("4")]
        four,
        [Description("5")]
        five,
        [Description("=")]
        equal,
        [Description("/")]
        divide,
    }

    public class Shortcut
    {
        public string Description;
        public ShortcutApp App;
        public ButtonType ButtonTypeData;
        public List<Key> Keys;
        public string KeysDescription
        {
            get
            {
                string keys = "";
                if (Keys != null)
                {
                    for (int i = 0; i < Keys.Count; i++)
                    {
                        var fieldVal = Keys[i].GetType().GetField(Keys[i].ToString());
                        DescriptionAttribute? attr = null;

                        if (fieldVal is not null)
                        {
                            attr = (DescriptionAttribute?)
                                Attribute.GetCustomAttribute(fieldVal, typeof(DescriptionAttribute));
                        }

                        keys += attr?.Description ?? Keys[i].ToString() + (i < Keys.Count - 1 ? " + " : "");
                    }
                }
                return keys;
            }
        }

        public Action Action
        {
            get
            {
                return () => ShortcutsManager.KeyboardShortcut(Keys);
            }
        }

        public Shortcut(string description, ShortcutApp app, ButtonType buttonType, List<Key>? keys = null)
        {
            Description = description;
            App = app;
            ButtonTypeData = buttonType;
            Keys = keys ?? new List<Key>();
        }
    }

    public static class ShortcutsManager
    {
        [DllImport("user32.dll")]
        private static extern nint GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(nint hWnd, out uint lpdwProcessId);
        public static InputSimulator input = new InputSimulator();

        public static Dictionary<Key, VirtualKeyCode> KeyCode = new Dictionary<Key, VirtualKeyCode>
        {
            { Key.Win, VirtualKeyCode.LWIN },
            { Key.Ctrl, VirtualKeyCode.CONTROL },
            { Key.Alt, VirtualKeyCode.MENU },
            { Key.Shift, VirtualKeyCode.SHIFT },
            { Key.Tab, VirtualKeyCode.TAB },
            { Key.Enter, VirtualKeyCode.RETURN },
            { Key.Space, VirtualKeyCode.SPACE },
            { Key.Esc, VirtualKeyCode.ESCAPE },
            { Key.A, VirtualKeyCode.VK_A },
            { Key.B, VirtualKeyCode.VK_B },
            { Key.C, VirtualKeyCode.VK_C },
            { Key.D, VirtualKeyCode.VK_D },
            { Key.E, VirtualKeyCode.VK_E },
            { Key.F, VirtualKeyCode.VK_F },
            { Key.H, VirtualKeyCode.VK_H },
            { Key.I, VirtualKeyCode.VK_I },
            { Key.K, VirtualKeyCode.VK_K },
            { Key.L, VirtualKeyCode.VK_L },
            { Key.M, VirtualKeyCode.VK_M },
            { Key.N, VirtualKeyCode.VK_N },
            { Key.O, VirtualKeyCode.VK_O },
            { Key.P, VirtualKeyCode.VK_P },
            { Key.R, VirtualKeyCode.VK_R },
            { Key.S, VirtualKeyCode.VK_S },
            { Key.T, VirtualKeyCode.VK_T },
            { Key.V, VirtualKeyCode.VK_V },
            { Key.W, VirtualKeyCode.VK_W },
            { Key.F4, VirtualKeyCode.F4 },
            { Key.F5, VirtualKeyCode.F5 },
            { Key.semicolon, VirtualKeyCode.OEM_1 },
            { Key.period, VirtualKeyCode.OEM_PERIOD },
            { Key.comma, VirtualKeyCode.OEM_COMMA },
            { Key.four, VirtualKeyCode.VK_4 },
            { Key.five, VirtualKeyCode.VK_5 },
            { Key.equal, VirtualKeyCode.OEM_PLUS },
            { Key.divide, VirtualKeyCode.OEM_2 },
        };

        // Create all shortcuts by app
        public static List<Shortcut> Shortcuts = new List<Shortcut>
        {
            { new Shortcut("None", ShortcutApp.none, ButtonType.none) },
            // Windows shortcuts
            { new Shortcut("Show/Hide Desktop", ShortcutApp.windows, ButtonType.win_desktop, new List<Key>{ Key.Win, Key.D }) },
            { new Shortcut("Open Clipboard", ShortcutApp.windows, ButtonType.win_clipboard, new List<Key>{ Key.Win, Key.V}) },
            { new Shortcut("Open File Explorer", ShortcutApp.windows, ButtonType.win_explorer, new List<Key>{ Key.Win, Key.E}) },
            { new Shortcut("Open Settings", ShortcutApp.windows, ButtonType.win_settings, new List<Key>{ Key.Win, Key.I}) },
            { new Shortcut("Open Snipping Tool", ShortcutApp.windows, ButtonType.win_snippingTool, new List<Key>{ Key.Win, Key.Shift, Key.S}) },
            // Word shortcuts
            { new Shortcut("Bold", ShortcutApp.word, ButtonType.word_bold, new List<Key>{Key.Ctrl, Key.B}) },
            { new Shortcut("Find", ShortcutApp.word, ButtonType.word_find, new List<Key>{Key.Ctrl, Key.F}) },
            { new Shortcut("Page break", ShortcutApp.word, ButtonType.word_pageBreak, new List<Key>{Key.Ctrl, Key.Enter}) },
            { new Shortcut("Replace", ShortcutApp.word, ButtonType.word_replace, new List<Key>{Key.Ctrl, Key.H}) },
            { new Shortcut("Select all", ShortcutApp.word, ButtonType.word_selectAll, new List<Key>{Key.Ctrl, Key.A}) },
            // Excel shortcuts        
            { new Shortcut("AutoSum", ShortcutApp.excel, ButtonType.excel_sum, new List<Key>{Key.Alt, Key.equal}) },
            { new Shortcut("Create table", ShortcutApp.excel, ButtonType.excel_table, new List<Key>{Key.Ctrl, Key.T}) },
            { new Shortcut("Currency format", ShortcutApp.excel, ButtonType.excel_currency, new List<Key>{Key.Ctrl, Key.Shift, Key.four}) },
            { new Shortcut("Insert current date", ShortcutApp.excel, ButtonType.excel_date, new List<Key>{Key.Ctrl, Key.semicolon}) },
            { new Shortcut("Percentage format", ShortcutApp.excel, ButtonType.excel_percent, new List<Key>{Key.Ctrl, Key.Shift, Key.five}) },
            // PowerToys shortcuts
            { new Shortcut("Advanced Paste", ShortcutApp.powertoys, ButtonType.powertoys_advancedPaste, new List<Key>{Key.Win, Key.Shift, Key.V}) },
            { new Shortcut("Always on Top", ShortcutApp.powertoys, ButtonType.powertoys_alwaysOnTop, new List<Key>{Key.Win, Key.Ctrl, Key.T}) },
            { new Shortcut("Color Picker", ShortcutApp.powertoys, ButtonType.powertoys_colorPicker, new List<Key>{Key.Win, Key.Shift, Key.C}) },
            { new Shortcut("Screen Ruler", ShortcutApp.powertoys, ButtonType.powertoys_screenRuler, new List<Key>{Key.Win, Key.Ctrl, Key.Shift, Key.M}) },
            { new Shortcut("Shortcut Guide", ShortcutApp.powertoys, ButtonType.powertoys_shortcutGuide, new List<Key>{Key.Win, Key.Shift, Key.divide}) },           
            // Teams shortcuts   
            { new Shortcut("Enable camera", ShortcutApp.teams, ButtonType.teams_enableCamera, new List<Key>{Key.Ctrl, Key.Shift, Key.O}) },
            { new Shortcut("Enable live captions", ShortcutApp.teams, ButtonType.teams_enableCaptions, new List<Key>{Key.Alt, Key.Shift, Key.C}) },
            { new Shortcut("Enable microphone", ShortcutApp.teams, ButtonType.teams_enableMic, new List<Key>{Key.Ctrl, Key.Shift, Key.M}) },
            { new Shortcut("New chat", ShortcutApp.teams, ButtonType.teams_chat, new List<Key>{Key.Ctrl, Key.N}) },
            { new Shortcut("Raise hand", ShortcutApp.teams, ButtonType.teams_raiseHand, new List<Key>{Key.Ctrl, Key.Shift, Key.K}) },
        };

        public static List<Shortcut> GetShortcutsByApp(ShortcutApp app)
        {
            var actualApp = app == ShortcutApp.none ? ShortcutApp.windows : app;

            return Shortcuts
                .Where(s => s.App == actualApp)
                .ToList();
        }

        public static List<ShortcutAppVisual> ShortcutAppVisuals =>
            Enum.GetValues(typeof(ShortcutApp))
                .Cast<ShortcutApp>()
                .Where(app => app != ShortcutApp.none)
                .Select(app => new ShortcutAppVisual(
                    app,
                    Shortcuts.Where(s => s.App == app).ToList()
                ))
            .ToList();

        // Trigger keyboard shortcut
        public static void KeyboardShortcut(List<Key> list)
        {
            if (list == null || list.Count == 0) return;

            var keyCodes = list.Select(k => KeyCode[k]).ToList();

            if (keyCodes.Count == 1)
            {
                input.Keyboard.KeyPress(keyCodes[0]);
            }
            else
            {
                var modifiers = keyCodes.Take(keyCodes.Count - 1);
                var mainKey = keyCodes.Last();
                input.Keyboard.ModifiedKeyStroke(modifiers, mainKey);
            }
        }

        public static string ForegroundApp = "";
        public static string CheckForegroundApp()
        {
            nint hwnd = GetForegroundWindow();
            GetWindowThreadProcessId(hwnd, out uint processId);
            Process process = Process.GetProcessById((int)processId);

            return process.ProcessName;
        }

        public static bool IsPowerToysRunning
        {
            get
            {
                return Process.GetProcessesByName("PowerToys").Any();
            }
        }
    }

    public class ShortcutAppVisual
    {
        public ShortcutApp App { get; set; }
        public string ImagePath { get; set; }
        public List<Shortcut> Shortcuts { get; set; }
        public SolidColorBrush ColorBrush { get; set; }

        public ShortcutAppVisual(ShortcutApp app, List<Shortcut> shortcuts)
        {
            App = app;
            Shortcuts = shortcuts;
            ImagePath = $"/Assets/Images/app_{app}.png";
            ColorBrush = GetBrushForApp(app);
        }

        private SolidColorBrush GetBrushForApp(ShortcutApp app)
        {
            string hex = app switch
            {
                ShortcutApp.windows => "#B3D7F2",
                ShortcutApp.word => "#D1DEF2",
                ShortcutApp.excel => "#B7D8C6",
                ShortcutApp.powertoys => "#FFFFFF",
                ShortcutApp.teams => "#C9CBEB",
                _ => "#CCCCCC"
            };

            return new SolidColorBrush(ToColor(hex));
        }

        private Color ToColor(string hex)
        {
            hex = hex.Replace("#", "");
            byte a = 255;
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            return Microsoft.UI.ColorHelper.FromArgb(a, r, g, b);
        }
    }
}

