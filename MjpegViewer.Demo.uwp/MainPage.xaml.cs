using System;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Zoomicon.Media.Streaming.Mjpeg;

namespace MjpegViewer.Demo.uwp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            this.InitializeComponent();
            InitMjpegDecoder();
        }

        private void btnGo_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                mjpegDecoder.ParseStream(new Uri(txtUri.Text));
            }
            catch(Exception ex)
            {
                ShowError($"{ex.Message}");
            }
        }

        #region MJPEG

        MjpegDecoder mjpegDecoder;

        private void InitMjpegDecoder()
        {
            mjpegDecoder = new MjpegDecoder();
            mjpegDecoder.FrameReady += MjpegDecoder_FrameReady;
            mjpegDecoder.Error += MjpegDecoder_ErrorAsync;
        }

        private void MjpegDecoder_FrameReady(object sender, FrameReadyEventArgs e)
        {
            imgMjpeg.Source = e.BitmapImage;
        }

        private void MjpegDecoder_ErrorAsync(object sender, ErrorEventArgs e)
        {
            ShowError($"{e.Message} ({e.ErrorCode})");
        }

        #endregion
        
        private async void ShowError(string msg)
        {
            await new MessageDialog(msg) { Title = "Error" }.ShowAsync();
        }
    }
}
