using SharedAssetBundleScripts.Runtime.SetLocationScripts;
using UnityEngine;

namespace Extensions
{
    public static class SetLocationMediaPlayerExtensions
    {
        public static void PlayDefaultVideo(this UserPhotoVideoPlayer player)
        {
            player.PlayVideo($"{Application.streamingAssetsPath}/Video/white_noise.mp4");
        }
    }
}