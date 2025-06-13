using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Modules.LevelManaging.Assets
{
    /// <summary>
    /// Helps to get available caption assets from render features (which happens every frame)
    /// Keeps all loaded caption assets
    /// </summary>
    internal static class CaptionAssetsRegister
    {
        private static readonly List<ICaptionAsset> CAPTION_ASSETS = new List<ICaptionAsset>();

        public static ICollection<ICaptionAsset> CaptionAssets
        {
            get
            {
                if (Application.isEditor && !Application.isPlaying && CAPTION_ASSETS.Any())
                {
                    CAPTION_ASSETS.Clear();
                }
                return CAPTION_ASSETS;
            }
        }

        public static void Register(ICaptionAsset captionAsset)
        {
            if (!CAPTION_ASSETS.Contains(captionAsset))
            {
                CAPTION_ASSETS.Add(captionAsset);
            }
        }

        public static void Unregister(ICaptionAsset captionAsset)
        {
            if (CAPTION_ASSETS.Contains(captionAsset))
            {
                CAPTION_ASSETS.Remove(captionAsset);
            }
        }
    }
}