using System;
using Modules.LevelManaging.Assets;
using Object = UnityEngine.Object;

namespace Modules.AssetsManaging.Unloaders
{
    internal sealed class VfxUnloader : AssetUnloader
    {
        public override void Unload(IAsset asset, Action onSuccess)
        {
            var vfx = asset as IVfxAsset;
            
            vfx?.OnUnloadStarted();
           
            Object.Destroy(vfx?.GameObject);

            if (vfx?.Bundle != null)
            {
                vfx.Bundle.Unload(false);
            }

            onSuccess?.Invoke();
        }
    }
}
