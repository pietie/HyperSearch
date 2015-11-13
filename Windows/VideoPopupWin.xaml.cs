using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HyperSearch.Windows
{
    /// <summary>
    /// Interaction logic for VideoPopupWin.xaml
    /// </summary>
    public partial class VideoPopupWin : Window
    {
        private Guid? _lastRequestGuid = Guid.Empty;

        public VideoPopupWin()
        {
            InitializeComponent();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (video.Source != null) video.Stop();

            video.Source = null;
            video.Visibility = System.Windows.Visibility.Hidden;

            var g = _lastRequestGuid.Value;

            Util.SetTimeout(80, new Action(() =>
            {
                if (this.IsVisible && g == _lastRequestGuid.Value)
                {
                    this.Hide();
                }
            }));
        }

        public void LoadVideo(string systemName, string gameName)
        {
            try
            {
                _lastRequestGuid = Guid.NewGuid();

                var mp4Path = Global.BuildFilePathInHyperspinDir("Media\\{0}\\Video\\{1}.mp4", systemName, gameName);
                var flvPath = Global.BuildFilePathInHyperspinDir("Media\\{0}\\Video\\{1}.mp4", systemName, gameName);

                if (File.Exists(mp4Path))
                {
                    video.Source = new Uri(mp4Path, UriKind.Absolute);
                }
                else if (File.Exists(flvPath))
                {
                    video.Source = new Uri(flvPath, UriKind.Absolute);
                }

                else
                {
                    var mp4NoVideo = Global.BuildFilePathInHyperspinDir("Media\\Frontend\\Video\\No Video.mp4");
                    var flvNoVideo = Global.BuildFilePathInHyperspinDir("Media\\Frontend\\Video\\No Video.flv");

                    if (File.Exists(mp4NoVideo)) video.Source = new Uri(mp4NoVideo, UriKind.Absolute);
                    else video.Source = new Uri(flvNoVideo, UriKind.Absolute);
                }

                if (video.Source != null) video.Play();
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex);
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void video_MediaOpened(object sender, RoutedEventArgs e)
        {
            video.Visibility = System.Windows.Visibility.Visible;
        }
    }
}
