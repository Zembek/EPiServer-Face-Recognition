using EPiFaceRecognition.Contracts;
using EPiFaceRecognition.Extensions;
using EPiServer.DataAccess.Internal;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace EPiFaceRecognition.Services
{
    internal class ImageResizeService
    {
        internal static byte[] CutAndResizeImage(Image image, FaceImageDimensions imageDimensions, int outputWidth, int outputHeight)
        {
            byte[] faceOnly = CutImage(image, imageDimensions);
            return ResizeImage(faceOnly, outputWidth, outputHeight);
        }

        internal static byte[] CutAndResizeImage(byte[] image, FaceImageDimensions imageDimensions, int outputWidth, int outputHeight)
        {
            return CutAndResizeImage(image.ByteArrayToImage(), imageDimensions, outputWidth, outputHeight);
        }

        internal static byte[] CutImage(Image image, FaceImageDimensions imageDimensions)
        {
            var destRect = new Rectangle(0, 0, imageDimensions.Width, imageDimensions.Height);
            var destImage = new Bitmap(imageDimensions.Width, imageDimensions.Height);
            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, imageDimensions.MinX, imageDimensions.MinY, imageDimensions.Width, imageDimensions.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage.BitmapToByteArray();
        }

        internal static byte[] CutImage(byte[] image, FaceImageDimensions imageDimensions)
        {
            return CutImage(image.ByteArrayToImage(), imageDimensions);
        }

        internal static byte[] ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage.BitmapToByteArray();
        }

        internal static byte[] ResizeImage(byte[] image, int width, int height)
        {
            return ResizeImage(image.ByteArrayToImage(), width, height);
        }


    }
}
