using System.Linq;
using Extensions;
using Models;
using Modules.AssetsManaging;
using Modules.FreverUMA.ViewManagement;
using Modules.LevelManaging.Assets;

namespace Modules.LevelManaging.Editing.Players.EventAssetsProviders
{
    internal sealed class CharacterAssetsProvider: EventAssetsProviderBase
    {
        private readonly CharacterViewContainer _characterViewContainer;
        public override DbModelType TargetType => DbModelType.Character;

        public CharacterAssetsProvider(IAssetManager assetManager, CharacterViewContainer characterViewContainer) : base(assetManager)
        {
            _characterViewContainer = characterViewContainer;
        }

        public override IAsset[] GetLoadedAssets(Event ev)
        {
            var characterAndOutfit = ev.CharacterController.Select(x => new
            {
                CharatcerId = x.Character.Id,
                x.OutfitId
            });
            var loadedCharacters = AssetManager.GetAllLoadedAssets(DbModelType.Character).Cast<ICharacterAsset>().ToArray();
            
            return loadedCharacters.Where(asset => characterAndOutfit.Any(x=>x.CharatcerId == asset.Id && HasLoadedOutfit(x.CharatcerId, x.OutfitId))).Cast<IAsset>().ToArray();
        }

        private bool HasLoadedOutfit(long characterId, long? outfitId)
        {
            return _characterViewContainer.HasPreparedView(characterId, outfitId);
        }
    }
}