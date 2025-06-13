using System.Threading.Tasks;
using Modules.FreverUMA.ViewManagement;
using Modules.LevelManaging.Assets;
using Extensions;
using JetBrains.Annotations;
using Bridge.Models.ClientServer.Assets;

namespace Modules.LevelManaging.Editing.AssetChangers
{
    /// <summary>
    /// It changes outfits on target characters(modifies target character meshes)
    /// </summary>
    [UsedImplicitly]
    internal sealed class OutfitChanger : BaseChanger
    {
        private readonly CharacterViewContainer _characterViewContainer;

        public OutfitChanger(CharacterViewContainer characterViewContainer)
        {
            _characterViewContainer = characterViewContainer;
        }

        public async Task Run(ICharacterAsset characterAsset, OutfitFullInfo outfit)
        {
            var havePreloadedView = _characterViewContainer.HasPreparedView(characterAsset.Id, outfit?.Id);
            if (!havePreloadedView)
            {
                await LoadViewAsync(characterAsset, outfit);
            }
            await ApplyCachedView(characterAsset, outfit);
            InvokeAssetUpdated(DbModelType.Outfit);
        }

        private Task LoadViewAsync(ICharacterAsset characterAsset, OutfitFullInfo outfit)
        {
            return _characterViewContainer.GetView(characterAsset.RepresentedModel, outfit);
        }

        private async Task ApplyCachedView(ICharacterAsset characterAsset, OutfitFullInfo outfit)
        {
            var newView = await _characterViewContainer.GetView(characterAsset.RepresentedModel, outfit);
            ApplyView(characterAsset, newView);
        }

        private static void ApplyView(ICharacterAsset characterAsset, CharacterView newView)
        {
            characterAsset.ChangeView(newView);
        }
    }
}