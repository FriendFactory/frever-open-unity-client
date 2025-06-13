using System.Collections.Generic;
using System.Linq;
using Extensions;
using JetBrains.Annotations;
using Modules.CameraSystem.CameraSystemCore;
using Modules.LevelManaging.Editing.Players.AssetPlayers;

namespace Modules.LevelManaging.Editing.Players.EventPlaying.Algorithms
{
    [UsedImplicitly]
    internal sealed class PreRecordingPlayingAlgorithm : EventPlayingAlgorithm
    {
        private readonly ICameraSystem _cameraSystem;
        private readonly ICameraTemplatesManager _cameraTemplatesManager;

        public PreRecordingPlayingAlgorithm(IPlayersManager playerManager, EventAssetsProvider eventAssetsProvider, 
               ICameraSystem cameraSystem, ICameraTemplatesManager cameraTemplatesManager) : base(playerManager, eventAssetsProvider)
        {
            _cameraSystem = cameraSystem;
            _cameraTemplatesManager = cameraTemplatesManager;
        }

        protected override void OnPlayStarted()
        {
            SetupBodyAnimationRestarting();
            base.OnPlayStarted();
            PutCameraOnStartPosition();
        }

        protected override void OnNewPlayersAdded(ICollection<IAssetPlayer> players)
        {
            base.OnNewPlayersAdded(players);
            SetupBodyAnimationRestarting();
        }

        protected override IReadOnlyCollection<DbModelType> GetAssetTypesToIgnore()
        {
            var types = new[] {DbModelType.FaceAnimation, DbModelType.CameraAnimation};
            return AUDIO_ASSET_TYPES.Concat(types).ToArray();
        }

        private void PutCameraOnStartPosition()
        {
            var templateClip = _cameraTemplatesManager.CurrentTemplateClip;
            _cameraSystem.Simulate(templateClip, 0);
        }
        
        private void SetupBodyAnimationRestarting()
        {
            var bodyAnimationPlayers =
                CurrentPlayers.Where(x => x.TargetType == DbModelType.BodyAnimation).Cast<BodyAnimationPlayer>();
            foreach (var bodyAnimationPlayer in bodyAnimationPlayers)
            {
                bodyAnimationPlayer.SetAutoRestarting(true);
            }
        }
    }
}