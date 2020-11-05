using System.Drawing;
using System.IO;

namespace EPiFaceRecognition.Extensions
{
    internal static class ImageExtensions
    {
        internal static byte[] ImageToByteArray(this Image ths)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ths.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }
        }

        internal static byte[] BitmapToByteArray(this Bitmap ths)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ths.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }
        }

        internal static Image ByteArrayToImage(this byte[] ths)
        {
            using (MemoryStream ms = new MemoryStream(ths))
            {
                return Image.FromStream(ms);
            }
        }

        internal static Bitmap ByteArrayToBitmap(this byte[] ths)
        {
            using (MemoryStream ms = new MemoryStream(ths))
            {
                return new Bitmap(ms);
            }
        }
    }
}
