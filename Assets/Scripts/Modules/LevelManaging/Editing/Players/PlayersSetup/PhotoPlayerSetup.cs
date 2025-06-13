using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using Extensions;
using Models;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.Players.AssetPlayers;

namespace Modules.LevelManaging.Editing.Players.PlayersSetup
{
    internal abstract class TextureBaseAssetPlayerSetup<TEntity, TTextureAsset, TPlayer> : GenericSetup<TTextureAsset, TPlayer> 
        where TEntity: IEntity
        where TTextureAsset: ITextureAsset<TEntity>
        where TPlayer: TextureAssetPlayer<TTextureAsset, TEntity>
    {
        private readonly IAssetManager _assetManager;

        internal TextureBaseAssetPlayerSetup(IAssetManager assetManager)
        {
            _assetManager = assetManager;
        }

        protected override void SetupPlayer(TPlayer player, Event ev)
        {
            var setLocationId = ev.GetSetLocation().Id;
            var setLocationAsset = _assetManager.GetAllLoadedAssets<ISetLocationAsset>().First(x => x.Id == setLocationId);
            player.SetTargetPlayers(setLocationAsset.MediaPlayer);
        }
    }
    
    internal sealed class PhotoPlayerSetup: TextureBaseAssetPlayerSetup<PhotoFullInfo, IPhotoAsset, PhotoAssetPlayer>
    {
        public PhotoPlayerSetup(IAssetManager assetManager) : base(assetManager)
        {
        }
    }

    internal sealed class SetLocationBackgroundPlayerSetup : TextureBaseAssetPlayerSetup<SetLocationBackground, ISetLocationBackgroundAsset, SetLocationBackgroundAssetPlayer>
    {
        public SetLocationBackgroundPlayerSetup(IAssetManager assetManager) : base(assetManager)
        {
        }
    }
}