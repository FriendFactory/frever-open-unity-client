using System;

namespace Common
{
    public sealed class ImageResolution
    {
        public float Width { get; }
        public float Height { get; }
        
        public ImageResolution(float width, float height)
        {
            Width = width;
            Height = height;
        }
        
        public bool IsAspectRatio_9_16()
        {
            const float factor916 = 0.5625f;
            return Math.Abs(Width / Height - factor916) < 0.001f;
        }
        
        public bool IsAspectRatio_16_9()
        {
            const float factor169 = 1.777f;
            return Math.Abs(Width / Height - factor169) < 0.001f;
        }
    }
}
