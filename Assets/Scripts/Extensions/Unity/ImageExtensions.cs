using System;
using UnityEngine.UI;

namespace Extensions
{
    public static class ImageExtensions
    {
        public static void SetAlpha(this Graphic image, float value)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));
            
            var color = image.color;
            color.a = value;
            image.color = color;
        }
    }
}