using Common;
using Settings;
using UnityEngine;

namespace Modules.VideoRecording
{
    public class VideoRecorderSettings
    {
        public readonly Vector2Int Resolution;

        // if bitrate is 0 than codec default bitrate should be used
        public readonly int Bitrate;
        public readonly int MssaSamples;
        public readonly bool SaveToGallery;

        public VideoRecorderSettings(bool isLandscapeMode, bool isUltraHD, bool saveToGallery)
        {
            var useOptimized = AppSettings.UseOptimizedCapturingScale;
            if (isLandscapeMode)
            {
                if (isUltraHD)
                {
                    Resolution = Constants.VideoRenderingResolution.LANDSCAPE_2160;
                    Bitrate = 30000000;
                }
                else
                {
                    Resolution = (useOptimized) ? Constants.VideoRenderingResolution.LANDSCAPE_720 : Constants.VideoRenderingResolution.LANDSCAPE_1080;
                    Bitrate = 0;
                }
            }
            else
            {
                Resolution = useOptimized ? Constants.VideoRenderingResolution.PORTRAIT_720 : Constants.VideoRenderingResolution.PORTRAIT_1080;
                Bitrate = 0;
            }

            MssaSamples = AppSettings.UseOptimizedCaptureQuality ? Mathf.Max(QualitySettings.antiAliasing, 1) : 1;
            SaveToGallery = saveToGallery;
        }
    }
}