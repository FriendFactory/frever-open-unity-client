using Extensions;
using JetBrains.Annotations;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.Players.AssetPlayers;
using Modules.LevelManaging.Editing.Players.EventAssetsProviders;
using Modules.LevelManaging.Editing.Players.PlayersSetup;

namespace Modules.LevelManaging.Editing.Players.AssetPlayerProfiles
{
    [UsedImplicitly]
    internal sealed class SetLocationPlayerProfile: GenericAssetPlayerProfile<ISetLocationAsset, SetLocationAssetPlayer, SetLocationPlayerSetup, SetLocationAssetProvider>
    {
        public override DbModelType AssetType => DbModelType.SetLocation;

        public SetLocationPlayerProfile(IAssetManager assetManager) : base(assetManager)
        {
        }

        protected override SetLocationAssetPlayer CreatePlayer()
        {
            return new SetLocationAssetPlayer();
        }

        protected override SetLocationPlayerSetup CreateSetup()
        {
            return new SetLocationPlayerSetup();
        }

        protected override SetLocationAssetProvider CreateEventAssetsProvider()
        {
            return new SetLocationAssetProvider(AssetManager);
        }
    }
}