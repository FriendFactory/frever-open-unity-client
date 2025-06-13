using UnityEngine;

namespace Extensions
{
    public static class TimeExtensions
    {
        public static long ToMilliseconds(this float seconds)
        {
            return seconds.ToMilli();
        }
        
        public static float ToSeconds(this float milliseconds)
        {
            return milliseconds.ToKilo();
        }
        
        public static int ToMilliseconds(this int seconds)
        {
            return seconds.ToMilli();
        }
        
        public static float ToSeconds(this int milliseconds)
        {
            return milliseconds.ToKilo();
        }
        
        public static long ToMilliseconds(this long seconds)
        {
            return seconds.ToMilli();
        }
        
        public static float ToSeconds(this long milliseconds)
        {
            return milliseconds.ToKilo();
        }
        
        /// <summary>
        /// clamps seconds to value between 0 and 999
        /// </summary>
        public static float ToSecondsClamped(this long milliseconds)
        {
            var seconds = milliseconds.ToKilo();
            return Mathf.Clamp(seconds, 0, 999f);
        }
    }
}