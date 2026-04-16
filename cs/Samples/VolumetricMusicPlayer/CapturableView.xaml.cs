using CSCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace VolumetricMusicPlayer
{
    public sealed partial class CapturableView : UserControl
    {
        public CapturableView()
        {
            InitializeComponent();
            TrackNameLabel.Text = "";
            ArtistNameLabel.Text = "";
            AlbumNameLabel.Text = "";
            TimeLabel1.Text = "0:00";
            TimeLabel2.Text = "0:00";
        }

        public void SetTimeAndPosition(double maxtime, double position)
        {
            var timespan = TimeSpan.FromSeconds(maxtime);
            TimeLabel2.Text = timespan.ToString(@"m\:ss");
            timespan = TimeSpan.FromSeconds(position);
            TimeLabel1.Text = timespan.ToString(@"m\:ss");
            ProgressBar.Value = position;
            ProgressBar.Maximum = maxtime;
        }

    }
}
