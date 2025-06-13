using Bridge.Models.ClientServer.Assets;
using UnityEngine;

namespace Modules.AssetsManaging.LoadArgs
{
    public sealed class CharacterLoadArgs: LoadAssetArgs<CharacterFullInfo>
    {
        public LayerMask? Layer;
        public OutfitFullInfo Outfit;
        public bool OptimizeMemory;
    }
}