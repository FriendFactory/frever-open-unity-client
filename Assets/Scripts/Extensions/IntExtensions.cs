using UnityEngine;

namespace Extensions
{
    public static class IntExtensions
    {
        public static string ToShortenedString(this int intValue)
        {
            if (intValue >= 1000000000)
            {
                return Mathf.Floor(intValue / 1000000000f) + "B";
            }

            if (intValue >= 1000000)
            {
                return Mathf.Floor(intValue / 1000000f) + "M";
            }

            if (intValue >= 1000)
            {
                return Mathf.Floor(intValue / 1000f) + "K";
            }

            return intValue.ToString();
        }
    }
}