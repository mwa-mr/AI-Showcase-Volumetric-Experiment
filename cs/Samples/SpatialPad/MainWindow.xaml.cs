using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI;
using WinRT.Interop;

namespace Volumetric.Samples.SpatialPad
{
    public sealed partial class MainWindow : Window
    {
        private Microsoft.UI.Windowing.AppWindow m_AppWindow;
        public Frame MainFrame => mainFrame;

        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(typeof(DesignPage));

            var hWnd = WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            m_AppWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

            m_AppWindow.Resize(new Windows.Graphics.SizeInt32(800, 800));

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(WindowTitleBar);

            var titleBar = m_AppWindow.TitleBar;

            double titleBarHeight = m_AppWindow.TitleBar.Height;
            WindowTitleBar.Height = titleBarHeight;

            titleBar.ButtonForegroundColor = Colors.Black;
        }
    }
}
