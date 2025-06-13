using Bridge.Models.ClientServer.Level.Full;
using Common;
using UnityEngine;

namespace Extensions
{
    public static class CaptionExtensions
    {
        public static void SetNormalizedPosition(this CaptionFullInfo caption, Vector2 position)
        {
            caption.PositionX = ClampPosition((int)(position.x * Constants.Captions.INT_POS_MULTIPLIER));
            caption.PositionY = ClampPosition((int)(position.y * Constants.Captions.INT_POS_MULTIPLIER));
        }

        private static int ClampPosition(int value)
        {
            return Mathf.Clamp(value, Constants.Captions.POS_MIN, Constants.Captions.POS_MAX);
        }

        public static Vector2 GetNormalizedPosition(this CaptionFullInfo caption)
        {
            return new Vector2
            {
                x = caption.PositionX / Constants.Captions.INT_POS_MULTIPLIER,
                y = caption.PositionY / Constants.Captions.INT_POS_MULTIPLIER
            };
        }
        
        public static Vector2Int NormalizedToConvertedCaptionPosition(this Vector2 normalized)
        {
            return new Vector2Int
            {
                x = (int)(normalized.x * Constants.Captions.INT_POS_MULTIPLIER),
                y = (int)(normalized.y * Constants.Captions.INT_POS_MULTIPLIER)
            };
        }
        
        public static Vector2 ToNormalizedPosition(this Vector2Int position)
        {
            return new Vector2
            {
                x = position.x / Constants.Captions.INT_POS_MULTIPLIER,
                y = position.y / Constants.Captions.INT_POS_MULTIPLIER
            };
        }
    }
}