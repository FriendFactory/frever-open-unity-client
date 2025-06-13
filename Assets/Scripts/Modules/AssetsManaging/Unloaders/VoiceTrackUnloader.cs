using System;
using Modules.LevelManaging.Assets;

namespace Modules.AssetsManaging.Unloaders
{
    internal sealed class VoiceTrackUnloader : AssetUnloader
    {
        public override void Unload(IAsset asset, Action onSuccess)
        {
            asset.CleanUp();
            onSuccess?.Invoke();
        }
    }
}
