using System.Linq;
using Bridge.Models.ClientServer.StartPack.Metadata;
using UnityEngine;

namespace Modules.WatermarkManagement
{
    internal static class WatermarkSettings
    {
        public static Texture2D WatermarkTexture;
        public static float Opacity = 1;
        public static bool IsOn;
        public static VideoOrientation Orientation;
        public static Watermark Watermark;
        public static Camera TargetCamera;

        public static PositionSettings GetPositionSettings()
        {
            return Watermark.Positions.First(x => x.VideoOrientation == Orientation);
        }
    }
}