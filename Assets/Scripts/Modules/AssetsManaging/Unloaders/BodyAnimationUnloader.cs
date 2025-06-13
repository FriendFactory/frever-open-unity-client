using System;
using Modules.LevelManaging.Assets;

namespace Modules.AssetsManaging.Unloaders
{
    internal sealed class BodyAnimationUnloader : AssetUnloader
    {
        public override void Unload(IAsset asset, Action onSuccess)
        {
            var bodyAnim = asset as IBodyAnimationAsset;
            
            bodyAnim?.OnUnloadStarted();
            
            if (bodyAnim?.Bundle != null)
            {
                bodyAnim.Bundle.Unload(false);
            }
            onSuccess?.Invoke();
        }
    }
}