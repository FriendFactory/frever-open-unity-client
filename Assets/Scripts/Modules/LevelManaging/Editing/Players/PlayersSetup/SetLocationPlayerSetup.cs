using Extensions;
using Models;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.Players.AssetPlayers;

namespace Modules.LevelManaging.Editing.Players.PlayersSetup
{
    internal sealed class SetLocationPlayerSetup: GenericSetup<ISetLocationAsset, SetLocationAssetPlayer>
    {
        protected override void SetupPlayer(SetLocationAssetPlayer player, Event ev)
        {
            var controller = ev.GetSetLocationController();
            var startTime = controller.ActivationCue/1000f;
            player.SetStartTime(startTime);

            var setLocation = ev.GetSetLocation();
            var playDefaultVideo = (setLocation.AllowPhoto ||
                                    setLocation.AllowVideo) && controller.Photo == null && controller.VideoClip == null;
            player.SetPlayDefaultBackground(playDefaultVideo);

            player.SetPictureInPictureSettings(controller.PictureInPictureSettings);
        }
    }
}