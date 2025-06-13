using System;
using Modules.LevelManaging.Assets;

namespace Modules.AssetsManaging.Unloaders
{
    internal class FaceAnimationUnloader : AssetUnloader
    {
        public override void Unload(IAsset asset, Action onSuccess)
        {
            onSuccess?.Invoke();
        }
    }
}
