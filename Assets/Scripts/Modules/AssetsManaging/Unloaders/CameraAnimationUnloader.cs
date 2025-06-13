using System;
using Modules.LevelManaging.Assets;

namespace Modules.AssetsManaging.Unloaders
{
    internal sealed class CameraAnimationUnloader : AssetUnloader
    {
        public override void Unload(IAsset asset, Action onSuccess)
        {
            //Camera animation doesn't require unloading
            onSuccess?.Invoke();
        }
    }
}
