using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CsBoids
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private readonly BoidManager m_boidManager;
        private readonly BoidsVolume m_volumetricBoid;
        private readonly bool _initialized;
        public MainWindow()
        {
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);
            OverlappedPresenter presenter = appWindow.Presenter as OverlappedPresenter;

            appWindow.ResizeClient(new Windows.Graphics.SizeInt32(600, 525));
            presenter.IsResizable = false;
            presenter.IsMaximizable = false;
            appWindow.Title = "BoidsVolume";

            m_boidManager = new BoidManager();
            this.InitializeComponent();

            maxVelocty.Value = m_boidManager.MaxVelocty;
            neighborDistance.Value = m_boidManager.NeighborDistance;
            maxRotationAngle.Value = m_boidManager.MaxRotationAngle;
            cohesionWeight.Value = m_boidManager.CohesionWeight;
            separationWeight.Value = m_boidManager.SeparationWeight;
            alignmentWeight.Value = m_boidManager.AlignmentWeight;
            seekWeight.Value = m_boidManager.SeekWeight;
            socializeWeight.Value = m_boidManager.SocializeWeight;
            arrivalMaxSpeed.Value = m_boidManager.ArrivalMaxSpeed;
            arrivalSlowingDistance.Value = m_boidManager.ArrivalSlowingDistance;

            _initialized = true;

            m_volumetricBoid = new BoidsVolume("CsBoids", m_boidManager);
        }

        private void maxVelocty_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!_initialized)
            {
                return;
            }

            m_boidManager.MaxVelocty = (float)e.NewValue;
        }
        private void arrivalSlowingDistance_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!_initialized)
            {
                return;
            }

            m_boidManager.ArrivalSlowingDistance = (float)e.NewValue;
        }
        private void arrivalMaxSpeed_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!_initialized)
            {
                return;
            }

            m_boidManager.ArrivalMaxSpeed = (float)e.NewValue;
        }
        private void maxRotationAngle_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!_initialized)
            {
                return;
            }

            m_boidManager.MaxRotationAngle = (float)e.NewValue;
        }
        private void neighborDistance_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!_initialized)
            {
                return;
            }

            m_boidManager.NeighborDistance = (float)e.NewValue;
        }
        private void cohesionWeight_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!_initialized)
            {
                return;
            }

            m_boidManager.CohesionWeight = (float)e.NewValue;
        }
        private void separationWeight_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!_initialized)
            {
                return;
            }

            m_boidManager.SeparationWeight = (float)e.NewValue;
        }
        private void alignmentWeight_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!_initialized)
            {
                return;
            }

            m_boidManager.AlignmentWeight = (float)e.NewValue;
        }
        private void seekWeight_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!_initialized)
            {
                return;
            }

            m_boidManager.SeekWeight = (float)e.NewValue;
        }
        private void socializeWeight_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!_initialized)
            {
                return;
            }

            m_boidManager.SocializeWeight = (float)e.NewValue;
        }
    }
}
