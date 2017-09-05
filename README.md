# MjpegDecoder
MJPEG decoder client library for the Universal Windows Platform (UWP)

combining code from:
* https://github.com/BrianPeek/mjpeg/blob/master/MJPEG/MjpegProcessor/MjpegDecoder.cs
* https://github.com/follesoe/MjpegProcessor/blob/master/src/MjpegProcessor/MjpegDecoder.cs

with the following enhancement:
* event also provides BitmapImage and SoftwareImage apart from IBuffer

If you use an async FrameReady event handler, get the image with
BitmapImage img = await e.GetBitmapImage();
SoftwareBitmap img = await e.GetSoftwareBitmap();

If you use a non-async FrameReady event handler, get the image with
BitmapImage img = e.BitmapImage;
SoftwareBitmap img = e.SoftwareBitmap;

Test URLs:
* http://camera1.mairie-brest.fr/mjpg/video.mjpg?resolution=320x240