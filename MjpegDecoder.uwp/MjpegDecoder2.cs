using System;
using System.IO;
using System.Net;

//using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Core;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
//using System.Drawing;

namespace MjpegProcessor1
{
	public class MjpegDecoder1
	{

		// magic 2 byte header for JPEG images
		private readonly byte[] JpegHeader = new byte[] { 0xff, 0xd8 };

		// pull down 1024 bytes at a time
		private const int ChunkSize = 1024;

		// used to cancel reading the stream
		private bool _streamActive;

		// current encoded JPEG image
		public IBuffer CurrentFrame { get; private set; }
		//public byte[] CurrentFrame { get; private set; }

		// used to marshal back to UI thread
		//private SynchronizationContext _context;
		private readonly CoreDispatcher _dispatcher;

		// event to get the buffer above handed to you
		public event EventHandler<FrameReadyEventArgs> FrameReady;
		public event EventHandler<ErrorEventArgs> Error;

		public MjpegDecoder1()
		{
            //_context = SynchronizationContext.Current;

			if(CoreWindow.GetForCurrentThread() != null)
				_dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
		}


		public void ParseStream(Uri uri)
		{
			ParseStream(uri, null, null);
		}

		public void ParseStream(Uri uri, string username, string password)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
			if(!string.IsNullOrEmpty(username) || !string.IsNullOrEmpty(password))
				request.Credentials = new NetworkCredential(username, password);

			// asynchronously get a response
			request.BeginGetResponse(OnGetResponse, request);
		}

		public void StopStream()
		{
			_streamActive = false;
		}

		private void OnGetResponse(IAsyncResult asyncResult)
		{
			byte[] imageBuffer = new byte[1024 * 1024];

			// get the response
			HttpWebRequest req = (HttpWebRequest)asyncResult.AsyncState;

			try
			{
				HttpWebResponse resp = (HttpWebResponse)req.EndGetResponse(asyncResult);

				// find our magic boundary value
				string contentType = resp.Headers["Content-Type"];
				if(!string.IsNullOrEmpty(contentType) && !contentType.Contains("="))
					throw new Exception("Invalid content-type header.  The camera is likely not returning a proper MJPEG stream.");

				string boundary = resp.Headers["Content-Type"].Split('=')[1].Replace("\"", "");
				byte[] boundaryBytes = Encoding.UTF8.GetBytes(boundary.StartsWith("--") ? boundary : "--" + boundary);

				Stream s = resp.GetResponseStream();
				BinaryReader br = new BinaryReader(s);

				_streamActive = true;

				byte[] buff = br.ReadBytes(ChunkSize);

				while (_streamActive)
				{
					// find the JPEG header
					int imageStart = buff.Find(JpegHeader);

					if(imageStart != -1)
					{
						// copy the start of the JPEG image to the imageBuffer
						int size = buff.Length - imageStart;
						Array.Copy(buff, imageStart, imageBuffer, 0, size);

						while(true)
						{
							buff = br.ReadBytes(ChunkSize);

							// find the boundary text
							int imageEnd = buff.Find(boundaryBytes);
							if(imageEnd != -1)
							{
								// copy the remainder of the JPEG to the imageBuffer
								Array.Copy(buff, 0, imageBuffer, size, imageEnd);
								size += imageEnd;

								byte[] frame = new byte[size];
								Array.Copy(imageBuffer, 0, frame, 0, size);

								ProcessFrame(frame);

								// copy the leftover data to the start
								Array.Copy(buff, imageEnd, buff, 0, buff.Length - imageEnd);

								// fill the remainder of the buffer with new data and start over
								byte[] temp = br.ReadBytes(imageEnd);

								Array.Copy(temp, 0, buff, buff.Length - imageEnd, temp.Length);
								break;
							}

							// copy all of the data to the imageBuffer
							Array.Copy(buff, 0, imageBuffer, size, buff.Length);
							size += buff.Length;
						}
					}
				}
                /**/resp.Dispose();
			}
			catch(Exception ex)
			{
				if(Error != null)
					_dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Error(this, new ErrorEventArgs() { Message = ex.Message, ErrorCode = ex.HResult }));
                    //_context.Post(delegate { Error(this, new ErrorEventArgs(ex.Message)); }, null);
			}
		}

		private async void ProcessFrame(byte[] frame)
		{
            //CurrentFrame = frame;
			CurrentFrame = frame.AsBuffer();

			// need to get this back on the UI thread
			_context.Post(delegate
			{
				// resets the BitmapImage to the new frame
				BitmapImage.SetSource(new MemoryStream(frame, 0, frame.Length));

				// tell whoever's listening that we have a frame to draw
				if(FrameReady != null)
					FrameReady(this, new FrameReadyEventArgs { FrameBuffer = CurrentFrame, BitmapImage = BitmapImage });
			}, null);

			if(_dispatcher != null)
			{
				await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
				{
					// tell whoever's listening that we have a frame to draw
					if (FrameReady != null)
						FrameReady(this, new FrameReadyEventArgs { FrameBuffer = frame.AsBuffer() });
				});
			}
*/
		}
	}

	static class Extensions
	{
		public static int Find(this byte[] buff, byte[] search)
		{
			// enumerate the buffer but don't overstep the bounds
			for(int start = 0; start < buff.Length - search.Length; start++)
			{
				// we found the first character
				if(buff[start] == search[0])
				{
					int next;

					// traverse the rest of the bytes
					for(next = 1; next < search.Length; next++)
					{
						// if we don't match, bail
						if(buff[start+next] != search[next])
							break;
					}

					if(next == search.Length)
						return start;
				}
			}
			// not found
			return -1;	
		}
	}

	public class FrameReadyEventArgs : EventArgs
	{
		public IBuffer FrameBuffer { get; set; }
		public BitmapImage BitmapImage;

        public FrameReadyEventArgs(IBuffer/*byte[]*/ buffer)
        {
            FrameBuffer = buffer;
        }
	}

	public class ErrorEventArgs : EventArgs
	{
		public string Message { get; set; }

        public ErrorEventArgs(string message)
        {
            Message = message;
        }

		public int ErrorCode { get; set; }
	}
}
