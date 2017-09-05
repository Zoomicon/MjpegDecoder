using System;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace Zoomicon.Media.Streaming.Mjpeg
{
    public class FrameReadyEventArgs : EventArgs
    {
        public IBuffer FrameBuffer { get; set; }

        public BitmapImage BitmapImage {
            get
            {
                return GetBitmapImage().GetAwaiter().GetResult();
            }
        }

        public async Task<BitmapImage> GetBitmapImage()
        {
            BitmapImage bitmapImage = new BitmapImage();
            using (InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream()) //see https://stackoverflow.com/questions/39370588/mjpeg-stream-decoder-for-universal-windows-platform-app
            {
                await ms.WriteAsync(FrameBuffer);
                ms.Seek(0);

                await bitmapImage.SetSourceAsync(ms);
            }
            return bitmapImage;
        }

        public SoftwareBitmap SoftwareBitmap
        {
            get
            {
                return GetSoftwareBitmap().GetAwaiter().GetResult();
            }
        }

        public async Task<SoftwareBitmap> GetSoftwareBitmap()
        {
            BitmapDecoder decoder;
            using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
            {
                await stream.WriteAsync(FrameBuffer);
                stream.Seek(0);

                decoder = await BitmapDecoder.CreateAsync(stream);
            }
            return await decoder.GetSoftwareBitmapAsync(decoder.BitmapPixelFormat, BitmapAlphaMode.Premultiplied);
        }

    }

    public class ErrorEventArgs : EventArgs
    {
        public string Message { get; set; }
        public int ErrorCode { get; set; }
    }
}
