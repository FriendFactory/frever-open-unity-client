using Bridge.Models.ClientServer.Assets;

namespace Modules.AssetsManaging.LoadArgs
{
    public sealed class CameraAnimLoadArgs: LoadAssetArgs<CameraAnimationFullInfo>
    {
        public bool LoadFromMemoryImmediate;//loads data in the same frame
        public string AnimationString;
    }
}