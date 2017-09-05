using System;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace Zoomicon.Media.Streaming.MJpeg
{
    public class FrameReadyEventArgs : EventArgs
    {
        public IBuffer FrameBuffer { get; set; }

        public BitmapImage BitmapImage {
            get
            {
                BitmapImage bitmapImage = new BitmapImage();
                using (InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream()) //see https://stackoverflow.com/questions/39370588/mjpeg-stream-decoder-for-universal-windows-platform-app
                {
                    ms.WriteAsync(FrameBuffer).GetAwaiter().GetResult();
                    ms.Seek(0);

                    bitmapImage.SetSourceAsync(ms).GetAwaiter().GetResult();
                }
                return bitmapImage;
            }
        }

        public SoftwareBitmap SoftwareBitmap
        {
            get
            {
                InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream();
                stream.WriteAsync(FrameBuffer).GetAwaiter().GetResult();
                stream.Seek(0);
                BitmapDecoder decoder = BitmapDecoder.CreateAsync(stream).GetAwaiter().GetResult();
                return decoder.GetSoftwareBitmapAsync(decoder.BitmapPixelFormat, BitmapAlphaMode.Premultiplied).GetAwaiter().GetResult();
            }
        }

    }

    public class ErrorEventArgs : EventArgs
    {
        public string Message { get; set; }
        public int ErrorCode { get; set; }
    }
}
