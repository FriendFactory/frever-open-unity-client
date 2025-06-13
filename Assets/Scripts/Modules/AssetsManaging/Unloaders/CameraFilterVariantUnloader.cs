using System;
using Modules.LevelManaging.Assets;

namespace Modules.AssetsManaging.Unloaders
{
    internal sealed class CameraFilterVariantUnloader : AssetUnloader
    {
        public override void Unload(IAsset asset, Action onSuccess)
        {
            var variantAsset = asset as ICameraFilterVariantAsset;
            if (variantAsset == null) return;

            UnityEngine.Object.Destroy(variantAsset.GameObject);

            if (variantAsset.Bundle != null)
            {
                variantAsset.Bundle.Unload(false);
            }
            onSuccess?.Invoke();
        }
    }
}