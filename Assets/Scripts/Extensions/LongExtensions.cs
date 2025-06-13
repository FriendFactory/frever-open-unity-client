using UnityEngine;

namespace Extensions
{
    public static class LongExtensions
    {
        public static string ToShortenedString(this long longValue)
        {
            if (longValue >= 1000000000)
            {
                return Mathf.Floor(longValue / 1000000f) + "B";
            }

            if (longValue >= 1000000)
            {
                return Mathf.Floor(longValue / 1000000f) + "M";
            }

            if (longValue >= 1000)
            {
                return Mathf.Floor(longValue / 1000f) + "K";
            }

            return longValue.ToString();
        }
    }
}