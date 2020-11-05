using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Drawing;

namespace EPiFaceRecognition.Contracts
{
    internal class FaceImageDimensions
    {
        internal int MinY { get; private set; }
        internal int MaxY { get; private set; }
        internal int MinX { get; private set; }
        internal int MaxX { get; private set; }
        internal int Width => MaxX - MinX;
        internal int Height => MaxY - MinY;

        internal FaceImageDimensions(DetectedFace faceToInclude, double facePercentage, Image imageDetails) : this(faceToInclude, facePercentage, imageDetails.Height, imageDetails.Width)
        { }

        internal FaceImageDimensions(DetectedFace faceToInclude, double facePercentage, int imageHeight, int imageWidth)
        {
            FaceRectangle faceRectangle = faceToInclude.FaceRectangle;
            int imageSize = Calculate100Size(faceRectangle.Height > faceRectangle.Width ? faceRectangle.Height : faceRectangle.Width, facePercentage);
            Calculate(faceRectangle, imageSize, imageHeight, imageWidth);
        }

        private void Calculate(FaceRectangle faceRectangle, int imageSize, int imageHeight, int imageWidth)
        {
            int heightDifference = (int)Math.Floor((imageSize - faceRectangle.Height) / 2.0);
            int widthDifference = (int)Math.Floor((imageSize - faceRectangle.Width) / 2.0);
            MinY = Math.Max(faceRectangle.Top - heightDifference, 0);
            MaxY = Math.Min(MinY + faceRectangle.Height + (2 * heightDifference), imageHeight);

            MinX = Math.Max(faceRectangle.Left - widthDifference, 0);
            MaxX = Math.Min(MinX + faceRectangle.Width + (2 * widthDifference), imageWidth);
        }

        private int Calculate100Size(int value, double facePercentage)
        {
            return (int)Math.Floor((value * 100.0) / facePercentage);
        }
    }
}
