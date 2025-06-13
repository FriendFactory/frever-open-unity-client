using System;
using Modules.LevelManaging.Assets;
using Object = UnityEngine.Object;

namespace Modules.AssetsManaging.Unloaders
{
    internal sealed class CaptionUnloader : AssetUnloader
    {
        public override void Unload(IAsset asset, Action onSuccess)
        {
            var captionAsset = asset as ICaptionAsset;
            Object.Destroy(captionAsset?.GameObject);

            onSuccess?.Invoke();
        }
    }
}