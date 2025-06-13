using System.Linq;
using Common.TimeManaging;
using Extensions;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.Players.AssetPlayers;

namespace Modules.LevelManaging.Editing.Players.EventPlaying.Algorithms
{
    [UsedImplicitly]
    internal sealed class PreviewPlayingAlgorithm : EventSingleLoopPreviewAlgorithmBase
    {
        public PreviewPlayingAlgorithm(IPlayersManager playerManager, EventAssetsProvider eventAssetsProvider, ITimeSourceControl eventTimeSourceControl) : base(playerManager, eventAssetsProvider, eventTimeSourceControl)
        {
        }

        protected override void OnCanceled()
        {
            var cameraAnimPlayer = CurrentPlayers
                .First(x => x.TargetType == DbModelType.CameraAnimation) as CameraAnimationPlayer;
            cameraAnimPlayer.SetCameraOnLastPosition();
        }
    }
}