using Extensions;
using Models;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.Players.AssetPlayers;

namespace Modules.LevelManaging.Editing.Players.PlayersSetup
{
    internal abstract class AudioPlayerSetup<TAsset, TPlayer> : GenericSetup<TAsset, TPlayer> 
        where TAsset: IAudioAsset where TPlayer:  AssetPlayerBase<TAsset>, IAudioAssetPlayer
    {
        protected override void SetupPlayer(TPlayer player, Event ev)
        {
            var musicController = ev.GetMusicController();
            var startTime = musicController.ActivationCue.ToSeconds();
            player.SetStartTime(startTime);
            var volume = musicController.LevelSoundVolume / 100f;
            player.SetVolume(volume);
        }
    }
}