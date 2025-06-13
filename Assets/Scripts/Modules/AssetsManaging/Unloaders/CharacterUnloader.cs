using System;
using Modules.FreverUMA.ViewManagement;
using Modules.LevelManaging.Assets;
using Object = UnityEngine.Object;

namespace Modules.AssetsManaging.Unloaders
{
    internal sealed class CharacterUnloader : AssetUnloader
    {
        private readonly CharacterViewContainer _viewContainer;

        public CharacterUnloader(CharacterViewContainer viewContainer)
        {
            _viewContainer = viewContainer;
        }

        public override void Unload(IAsset asset, Action onSuccess)
        {
            var character = asset as ICharacterAsset;
            _viewContainer.UnloadAllCharacterViews(asset.Id);
            Object.Destroy(character?.GameObject);
            onSuccess?.Invoke();
        }
    }
}