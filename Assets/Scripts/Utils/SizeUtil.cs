using ByteSizeLib;
using UnityEngine;

namespace Utils
{
    public static class SizeUtil
    {
        public static long ConvertMbToBytes(float mb)
        {
            return (long)ByteSize.FromMegaBytes(mb).Bytes;
        }
        
        public static string FormatSize(float sizeMb)
        {
            if (sizeMb < 1)
            {
                return $"{Mathf.FloorToInt(sizeMb * 1000)} kB";
            }
            
            return sizeMb - Mathf.Floor(sizeMb) < 0.01f ? $"{Mathf.FloorToInt(sizeMb)} MB" : $"{sizeMb:0.00} MB";
        }
    }
}