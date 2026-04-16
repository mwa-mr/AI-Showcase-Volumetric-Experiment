using Microsoft.UI;
using Microsoft.UI.Xaml;
using WinRT.Interop;

namespace Volumetric.Samples.ProductConfigurator
{
    public sealed partial class MainWindow : Window
    {
        private Microsoft.UI.Windowing.AppWindow m_AppWindow;

        public MainWindow()
        {
            this.InitializeComponent();

            RootFrame.Navigate(typeof(ConfigPage));

            var hWnd = WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            m_AppWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

            m_AppWindow.Resize(new Windows.Graphics.SizeInt32(1200, 800));

            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(WindowTitleBar);

            var titleBar = m_AppWindow.TitleBar;

            double titleBarHeight = m_AppWindow.TitleBar.Height;
            WindowTitleBar.Height = titleBarHeight;

            titleBar.ButtonForegroundColor = Colors.Black;
        }
    }
}
