using Microsoft.UI.Xaml;
using System;

namespace Volumetric.Samples.SpatialPad
{
    public sealed partial class App : Application
    {
        public static Window? m_window { get; private set; } = null;

        public static bool AutoDeployRequested { get; private set; } = false;

        public static KeypadData[] Keypads = new KeypadData[5];
        public static KeypadData[] GetKeypads() => Keypads;

        private static int _currentKeypadId = 0;
        public static int CurrentKeypadId => _currentKeypadId;

        public App()
        {
            this.InitializeComponent();
            RequestedTheme = ApplicationTheme.Light;

            for (int i = 0; i < Keypads.Length; i++)
            {
                Keypads[i] = new KeypadData(i);
            }

            _currentKeypadId = 0;
        }

        public static KeypadData GetCurrentKeypad()
        {
            return Keypads[_currentKeypadId];
        }

        public static void SetCurrentKeypad(int id)
        {
            if (id >= 0 && id < Keypads.Length)
            {
                _currentKeypadId = id;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(id), "Keypad ID out of range");
            }
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            AutoDeployRequested = ShouldAutoDeploy();
            m_window = new MainWindow();
            m_window.Activate();
        }

        private static bool ShouldAutoDeploy()
        {
            string[] arguments = Environment.GetCommandLineArgs();

            foreach (string argument in arguments)
            {
                if (string.Equals(argument, "--autodeploy", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
