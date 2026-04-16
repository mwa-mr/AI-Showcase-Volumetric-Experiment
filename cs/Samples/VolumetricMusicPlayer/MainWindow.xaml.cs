using CSCore;
using CSCore.Codecs;
using CSCore.DSP;
using CSCore.SoundOut;
using CSCore.Streams;
using Microsoft.Graphics.Canvas;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Diagnostics;
using System.IO;
using VolumetricAudioVisualization;
using Windows.Foundation;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using WinformsVisualization.Visualization;

namespace VolumetricMusicPlayer
{
    public sealed partial class MainWindow : Window
    {
        private readonly ISoundOut _soundOut;
        private ISampleSource _sampleSource;
        private IWaveSource _source;
        private NotificationSource _notificationSource;
        private LineSpectrum _lineSpectrum;
        private Stopwatch _stopwatch = new Stopwatch();
        private float _lastUpdateTime = 0f;
        private float _aveFps = 0f;

        private VolumetricAppManager m_volumetricAppManager;

        CapturableView _offscreenView;
        Window _offscreenWindow;
        XamlSnapshot _helper;

        public MainWindow()
        {
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);
            OverlappedPresenter presenter = appWindow.Presenter as OverlappedPresenter;

            AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, ("Assets/icon.ico")));
            appWindow.ResizeClient(new Windows.Graphics.SizeInt32(728, 600));
            presenter.IsResizable = false;
            presenter.IsMaximizable = false;
            appWindow.Title = "Volumetric Music";

            this.InitializeComponent();

            _soundOut = new WasapiOut();

            LiveView.canvas.Draw += canvas_Draw;

            LiveView.PlayButton.Click += Button_Click;
            LiveView.PauseButton.Click += Button_Click;
            LiveView.NextButton.Click += Button_Click;
            LiveView.PreviousButton.Click += Button_Click;
            LiveView.OpenButton.Click += Button_Click;
            LiveView.SettingsButton.Click += (s, e) =>
            {
                SettingsPanel.Visibility = Visibility.Visible;
            };
            CloseSettingsButton.Click += (s, e) =>
            {
                SettingsPanel.Visibility = Visibility.Collapsed;
            };

            LiveView.MainGrid.CornerRadius = new CornerRadius(0);

            m_volumetricAppManager = new VolumetricAppManager();

            _stopwatch.Start();

            var fastTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1 / 72) };
            fastTimer.Tick += (s, e) =>
            {
                if (_sampleSource != null)
                {
                    //float elapsedTime = (float)_stopwatch.Elapsed.TotalMilliseconds - _lastUpdateTime;
                    //_lastUpdateTime = (float)_stopwatch.Elapsed.TotalMilliseconds;
                    //var fps = 1000f / elapsedTime;
                    //_aveFps = Lerp(_aveFps, fps, 0.05f);
                    //Trace.TraceInformation($"App FPS: {_aveFps:0.0}  ({fps:0.0})  {elapsedTime:0.0} ms");

                    if (_soundOut.PlaybackState == PlaybackState.Playing)
                    {
                        LiveView.canvas.Invalidate();
                    }
                }
            };
            fastTimer.Start();

            var slowTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1 / 2) };
            slowTimer.Tick += (s, e) =>
            {
                if (_soundOut.PlaybackState == PlaybackState.Playing)
                {
                    LiveView.PlayButton.Visibility = Visibility.Collapsed;
                    LiveView.PauseButton.Visibility = Visibility.Visible;
                }
                else
                {
                    LiveView.PlayButton.Visibility = Visibility.Visible;
                    LiveView.PauseButton.Visibility = Visibility.Collapsed;
                }
                if (_sampleSource != null)
                {
                    if (_soundOut.PlaybackState != PlaybackState.Stopped)
                    {
                        LiveView.SetTimeAndPosition(_sampleSource.GetLength().TotalSeconds, _sampleSource.GetPosition().TotalSeconds);
                        _offscreenView?.SetTimeAndPosition(_sampleSource.GetLength().TotalSeconds, _sampleSource.GetPosition().TotalSeconds);
                    }
                    if (SettingsPanel.Visibility == Visibility.Visible)
                    {
                        Stats.Text = m_volumetricAppManager.Volume?.GetStats() ?? "No stats available";
                    }
                }
            };
            slowTimer.Start();

            StartCaptureTimer();

            this.AppWindow.Closing += (s, e) =>
            {
                fastTimer.Stop();
                slowTimer.Stop();
                _soundOut.Dispose();
                _sampleSource?.Dispose();
                _source?.Dispose();
                _notificationSource?.Dispose();
                _helper.Dispose();
                App.Current.Exit();
            };

            ShowFilePicker();
        }

        private void Rows_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
        }


        private string _imagePath = Path.GetTempFileName() + ".png";

        void InitializeOffscreenCapture()
        {
            // force layout on MainRoot before reading ActualWidth
            MainRoot.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            MainRoot.Arrange(new Rect(0, 0, MainRoot.DesiredSize.Width, MainRoot.DesiredSize.Height));
            MainRoot.UpdateLayout();

            // now ActualWidth is set
            double w = MainRoot.ActualWidth;
            double h = MainRoot.ActualHeight;

            // 1) create a fresh copy of the same view
            _offscreenView = new CapturableView
            {
                Width = w,
                Height = h
            };

            _offscreenView.ButtonPanel.Visibility = Visibility.Collapsed; // hide buttons in offscreen view

            _offscreenWindow = new Window
            {
                Content = _offscreenView
            };

            _offscreenWindow.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(
            _X: -10000,
            _Y: -10000,
            _Width: 1200,
            _Height: 1000));
            //_offscreenWindow.Activate();

            // 3) prepare your snapshot helper
            _helper = new XamlSnapshot(
              root: _offscreenView,
              opts: new XamlSnapshot.Options
              {
                  FilePath = _imagePath,
                  MaxFileSize = 8_000_000,
                  Format = XamlSnapshot.ImageFormat.Png
              });
        }

        void StartCaptureTimer()
        {
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += async (s, e) =>
            {
                if (_offscreenView == null || _offscreenWindow == null)
                    InitializeOffscreenCapture();
                else
                {
                    await _helper.CaptureAsync();
                    m_volumetricAppManager.Volume?.UpdateTexture("", _imagePath);
                }
            };
            timer.Start();
        }

        async void ShowFilePicker()
        {
            var picker = new FileOpenPicker();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            try
            {
                picker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
                foreach (string ext in CodecFactory.SupportedFilesFilterEn.Split("|")[1].Split(";"))
                {
                    picker.FileTypeFilter.Add(ext.Replace("*", ""));
                }
                var file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    var filename = file.Path;
                    LoadAudio(filename);
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions, e.g., user canceled the picker or no file was selected
                Console.WriteLine($"Error picking file: {ex.Message}");
                ContentDialog errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Failed to load audio file. Please try again.\n{ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                ContentDialogResult result = await errorDialog.ShowAsync();
            }
        }

        static float Lerp(float firstFloat, float secondFloat, float by)
        {
            return firstFloat * (1 - by) + secondFloat * by;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).CommandParameter.ToString() == "play")
            {
                if (_soundOut.PlaybackState == PlaybackState.Stopped)
                {
                    //LoadAudio();
                }
                else
                {
                    _soundOut.Play();
                }
            }
            else if (((Button)sender).CommandParameter.ToString() == "pause")
            {
                _soundOut.Pause();
            }
            else if (((Button)sender).CommandParameter.ToString() == "back")
            {
                if (_sampleSource.GetPosition() > TimeSpan.FromSeconds(10))
                    _sampleSource.SetPosition(_sampleSource.GetPosition() - TimeSpan.FromSeconds(10));
                else
                    _sampleSource.SetPosition(TimeSpan.Zero);
            }
            else if (((Button)sender).CommandParameter.ToString() == "forward")
            {
                if (_sampleSource.GetPosition() < _sampleSource.GetLength() - TimeSpan.FromSeconds(10))
                    _sampleSource.SetPosition(_sampleSource.GetPosition() + TimeSpan.FromSeconds(10));
                else
                    _sampleSource.SetPosition(TimeSpan.Zero);
            }
            else if (((Button)sender).CommandParameter.ToString() == "stop")
            {
                CleanupPlayback();
            }
            else if (((Button)sender).CommandParameter.ToString() == "open")
            {
                ShowFilePicker();
            }
        }

        private void CleanupPlayback()
        {
            if (_soundOut != null)
            {
                _soundOut.Stop();
            }
            if (_sampleSource != null)
            {
                _sampleSource.Dispose();
            }
            if (_notificationSource != null)
            {
                _notificationSource.Dispose();
            }
        }

        public BitmapImage ToImage(byte[] array)
        {
            var bitmapImage = new BitmapImage();
            try
            {
                using (var stream = new InMemoryRandomAccessStream())
                {
                    using (var writer = new DataWriter(stream))
                    {
                        writer.WriteBytes(array);
                        writer.StoreAsync().GetAwaiter().GetResult();
                        writer.FlushAsync().GetAwaiter().GetResult();
                        writer.DetachStream(); // Detach the stream to avoid closing it prematurely

                    }
                    stream.Seek(0);
                    bitmapImage.SetSource(stream);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error converting byte array to image: {ex.Message}");
                // Optionally, you can set a default image or handle the error accordingly
                bitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/DefaultAlbumArt.png")); // Default image path
            }
            return bitmapImage;
        }

        private void LoadAudio(string filename)
        {
            CleanupPlayback();
            var source = CodecFactory.Instance.GetCodec(filename);
            _sampleSource = source.ToSampleSource();
            SetupSampleSource(_sampleSource);
            _soundOut.Initialize(_source);
            _soundOut.Play();
            //_soundOut.Pause();

            try
            {
                TagLib.File tagFile = TagLib.File.Create(filename);
                Debug.WriteLine($"Loaded file: {tagFile.Name}");
                _offscreenView.TrackNameLabel.Text = LiveView.TrackNameLabel.Text = tagFile.Tag.Title ?? "Unknown Title";
                _offscreenView.ArtistNameLabel.Text = LiveView.ArtistNameLabel.Text = tagFile.Tag.FirstAlbumArtist ?? "Unknown Artist";
                _offscreenView.AlbumNameLabel.Text = LiveView.AlbumNameLabel.Text = tagFile.Tag.Album ?? "Unknown Album";
                _offscreenView.AlbumImage.ImageSource = LiveView.AlbumImage.ImageSource = ToImage(tagFile.Tag.Pictures.Length > 0 ? tagFile.Tag.Pictures[0].Data.Data : Array.Empty<byte>());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading audio metadata: {ex.Message}");
                _offscreenView.TrackNameLabel.Text = LiveView.TrackNameLabel.Text = "Unknown Title";
                _offscreenView.ArtistNameLabel.Text = LiveView.ArtistNameLabel.Text = "Unknown Artist";
                _offscreenView.AlbumNameLabel.Text = LiveView.AlbumNameLabel.Text = "Unknown Album";
                _offscreenView.AlbumImage.ImageSource = LiveView.AlbumImage.ImageSource = ToImage(Array.Empty<byte>()); // Set a default image if metadata fails
            }
        }

        private void SetupSampleSource(ISampleSource aSampleSource)
        {
            const FftSize fftSize = FftSize.Fft4096;
            //create a spectrum provider which provides fft data based on some input
            var spectrumProvider = new BasicSpectrumProvider(aSampleSource.WaveFormat.Channels,
                aSampleSource.WaveFormat.SampleRate, fftSize);

            _lineSpectrum = new LineSpectrum(fftSize)
            {
                SpectrumProvider = spectrumProvider,
                UseAverage = true,
                BarCount = 1100,
                BarSpacing = 0,
                IsXLogScale = true,
                MinimumFrequency = 20,
                MaximumFrequency = 18000,
                ScalingStrategy = ScalingStrategy.Sqrt
            };

            //the SingleBlockNotificationStream is used to intercept the played samples
            var notificationSource = new SingleBlockNotificationStream(aSampleSource);
            //pass the intercepted samples as input data to the spectrumprovider (which will calculate a fft based on them)
            notificationSource.SingleBlockRead += (s, a) => spectrumProvider.Add(a.Left, a.Right);

            _source = notificationSource.ToWaveSource(16);
        }

        private void GenerateLineSpectrum(Size size, CanvasDrawingSession drawingSession)
        {
            if (_lineSpectrum == null || _soundOut.PlaybackState != PlaybackState.Playing)
                return;
            var data = _lineSpectrum.DrawSpectrumLine(size, drawingSession, 1.75f);
            m_volumetricAppManager.Volume?.SetData(data, (int)Rows.Value, (int)History.Value);
        }

        private void canvas_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            if (_lineSpectrum == null || _soundOut.PlaybackState != PlaybackState.Playing)
                return;
            GenerateLineSpectrum(sender.Size, args.DrawingSession);
        }
    }
}
