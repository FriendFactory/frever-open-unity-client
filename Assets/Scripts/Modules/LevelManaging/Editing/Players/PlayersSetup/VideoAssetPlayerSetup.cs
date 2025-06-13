using System.Linq;
using Extensions;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.Players.AssetPlayers;
using Event = Models.Event;

namespace Modules.LevelManaging.Editing.Players.PlayersSetup
{
    internal sealed class VideoAssetPlayerSetup: GenericSetup<IVideoClipAsset,VideoAssetPlayer>
    {
        private readonly IAssetManager _assetManager;

        public VideoAssetPlayerSetup(IAssetManager assetManager)
        {
            _assetManager = assetManager;
        }

        protected override void SetupPlayer(VideoAssetPlayer player, Event ev)
        {
            var setLocationId = ev.GetSetLocation().Id;
            var setLocationAsset =
                _assetManager.GetAllLoadedAssets<ISetLocationAsset>().First(x => x.Id == setLocationId);

            player.SetTargetPlayers(setLocationAsset.MediaPlayer);
            var setLocationController = ev.GetSetLocationController();
            var activationCue = setLocationController.VideoActivationCue ?? 0;
            player.SetStartTime(activationCue.ToSeconds());
        }
    }
}