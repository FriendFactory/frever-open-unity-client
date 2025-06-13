using Extensions;
using Modules.LevelManaging.Assets;
using SharedAssetBundleScripts.Runtime.SetLocationScripts;
using UnityEngine.Video;

namespace Modules.LevelManaging.Editing.Players.AssetPlayers
{
    internal sealed class VideoAssetPlayer: GenericTimeDependAssetPlayer<IVideoClipAsset>
    {
        private UserPhotoVideoPlayer[] _mediaPlayers;
        private float VideoLengthSec => Target.RepresentedModel.Duration.Value.ToSeconds();

        public void SetTargetPlayers(params UserPhotoVideoPlayer[] targets)
        {
            _mediaPlayers = targets;
        }
        
        public override void Simulate(float time)
        {
            time = (StartTime + time)% VideoLengthSec;
            foreach (var player in _mediaPlayers)
            {
                SimulateAsync(Target.FilePath, time, player);
            }
        }

        private void SimulateAsync(string filePath, float time, UserPhotoVideoPlayer player)
        {
            player.VideoPlayer.url = filePath;
            player.VideoPlayer.Prepare();
            player.VideoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            
            player.PlayVideo(Target.FilePath, time);
            player.VideoPlayer.Pause();
        }

        protected override void OnPlay()
        {
            var startTime = StartTime % VideoLengthSec;
            foreach (var player in _mediaPlayers)
            {
                player.PlayVideo(Target.FilePath, startTime);
            }
        }
        
        protected override void OnPause()
        {
            foreach (var player in _mediaPlayers)
            {
                player.VideoPlayer.Pause();
            }
        }

        protected override void OnResume()
        {
            foreach (var player in _mediaPlayers)
            {
                player.VideoPlayer.Play();
            }
        }

        protected override void OnStop()
        {
            foreach (var player in _mediaPlayers)
            {
                //use Pause instead of Stop because Unity video player requires few frames
                //for video initialization playing after stop
                player.VideoPlayer.Pause();
            }
        }
    }
}