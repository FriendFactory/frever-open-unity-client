using UnityEngine;

namespace Extensions
{
    public static class Color32Extension
    {
        public static Color32 ConvertFromIntColor(this Color32 color, int intColor)
        {
            color.a = (byte)((intColor) & 0xFF);
            color.b = (byte)((intColor >> 8) & 0xFF);
            color.g = (byte)((intColor >> 16) & 0xFF);
            color.r = (byte)((intColor >> 24) & 0xFF);
            return color;
        }

        public static int ConvertToInt(this Color32 color)
        {
            var rgba = (color.r << 24) + (color.g << 16) + (color.b << 8) + color.a;
            return rgba;
        }
    }
}