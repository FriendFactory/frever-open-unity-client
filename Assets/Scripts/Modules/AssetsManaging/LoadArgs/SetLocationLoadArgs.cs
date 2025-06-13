using Bridge.Models.ClientServer.Assets;
using UnityEngine;

namespace Modules.AssetsManaging.LoadArgs
{
    public sealed class SetLocationLoadArgs: LoadAssetArgs<SetLocationFullInfo>
    {
        public LayerMask PictureInPictureLayerMask;
        internal static float PictureInPictureRenderScale = 1;
    }
}