using Bridge.Models.ClientServer.Assets;

namespace Modules.AssetsManaging.LoadArgs
{
    public sealed class VfxLoadArgs: LoadAssetArgs<VfxInfo>
    {
        public bool StopVfxAudioOnLoad;
    }
}