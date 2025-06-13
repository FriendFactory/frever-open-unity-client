using System;
using Modules.LevelManaging.Assets;

namespace Modules.AssetsManaging.Unloaders
{
    internal abstract class AssetUnloader
    {
        public abstract void Unload(IAsset asset, Action onSuccess = null);
    }
}