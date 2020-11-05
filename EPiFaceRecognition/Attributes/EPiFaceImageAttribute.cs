using System;

namespace EPiFaceRecognition.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class EPiFaceImageAttribute : Attribute
    {
        private const double DEFAULT_PERCENTAGE = 70.0;

        public int Width { get; set; }
        public int Height { get; set; }
        public double FacePercentage { get; set; }

        public EPiFaceImageAttribute(int width, int height) : this(width, height, DEFAULT_PERCENTAGE) { }

        public EPiFaceImageAttribute(int width, int height, double facePercentage)
        {
            Width = width;
            Height = height;
            FacePercentage = facePercentage;
        }
    }
}
