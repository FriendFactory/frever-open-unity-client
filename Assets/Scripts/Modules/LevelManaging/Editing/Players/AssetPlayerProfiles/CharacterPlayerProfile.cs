using Extensions;
using JetBrains.Annotations;
using Modules.AssetsManaging;
using Modules.FreverUMA.ViewManagement;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.AssetChangers;
using Modules.LevelManaging.Editing.Players.AssetPlayers;
using Modules.LevelManaging.Editing.Players.EventAssetsProviders;
using Modules.LevelManaging.Editing.Players.PlayersSetup;

namespace Modules.LevelManaging.Editing.Players.AssetPlayerProfiles
{
    [UsedImplicitly]
    internal sealed class CharacterPlayerProfile: GenericAssetPlayerProfile<ICharacterAsset, CharacterPlayer, CharacterPlayerSetup, CharacterAssetsProvider>
    {
        public override DbModelType AssetType => DbModelType.Character;
        private readonly CharacterViewContainer _characterViewContainer; 
        private readonly ILayerManager _layerManager;
        
        public CharacterPlayerProfile(IAssetManager assetManager, ILayerManager layerManager, CharacterViewContainer characterViewContainer) : base(assetManager)
        {
            _layerManager = layerManager;
            _characterViewContainer = characterViewContainer;
        }
        
        protected override CharacterPlayer CreatePlayer()
        {
            return new CharacterPlayer(_layerManager);
        }

        protected override CharacterPlayerSetup CreateSetup()
        {
            return new CharacterPlayerSetup();
        }

        protected override CharacterAssetsProvider CreateEventAssetsProvider()
        {
            return new CharacterAssetsProvider(AssetManager, _characterViewContainer);
        }
    }
}