using UnityEngine;

namespace Extensions
{
    public static class ColorExtension
    {
        public static Color SetR(this Color color, float r)
        {
            return new Color(r, color.g, color.b, color.a);
        }
        
        public static Color SetG(this Color color, float g)
        {
            return new Color(color.r, g, color.b, color.a);
        }
        
        public static Color SetB(this Color color, float b)
        {
            return new Color(color.r, color.g, b, color.a);
        }
        
        public static Color SetAlpha(this Color color, float a)
        {
            return new Color(color.r, color.g, color.b, a);
        }
        
        public static Color HexToColor(string hex)
        {
            hex = hex.Replace("#", "");
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            byte a = 255;        // Assuming full opacity
            if (hex.Length == 8) // If alpha value is included
                a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
        
            return new Color32(r, g, b, a);
        }

        public static string ToHexRgb(this Color color)
        {
            Color32 color32 = color;
            return "#" + color32.r.ToString("X2") + color32.g.ToString("X2") + color32.b.ToString("X2");
        }
    }
}