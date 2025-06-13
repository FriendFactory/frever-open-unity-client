using System.Collections.Generic;
using System.Linq;
using Extensions;
using Modules.CameraSystem.CameraSystemCore;
using Modules.LevelManaging.Editing.Players.AssetPlayers;

namespace Modules.LevelManaging.Editing.Players.EventPlaying.Algorithms
{
    internal sealed class RecordingPlayingAlgorithm: EventPlayingAlgorithm
    {
        private readonly ICameraSystem _cameraSystem;
        private readonly ICameraTemplatesManager _cameraTemplatesManager;

        public RecordingPlayingAlgorithm(IPlayersManager playerManager, EventAssetsProvider eventAssetsProvider, 
               ICameraSystem cameraSystem, ICameraTemplatesManager cameraTemplatesManager) 
            : base(playerManager, eventAssetsProvider)
        {
            _cameraSystem = cameraSystem;
            _cameraTemplatesManager = cameraTemplatesManager;
        }

        protected override void OnPlayStarted()
        {
            base.OnPlayStarted();
            _cameraTemplatesManager.SaveCurrentCameraStateAsStartFrameForTemplates();
            var templateClip = _cameraTemplatesManager.CurrentTemplateClip;
            _cameraSystem.PlayTemplate(templateClip, TemplatePlayingMode.Continuous);
        }

        protected override void OnStopped()
        {
            _cameraSystem.PauseAnimation();
        }
        
        protected override IReadOnlyCollection<DbModelType> GetAssetTypesToIgnore()
        {
            return new[] {DbModelType.VoiceTrack};
        }
    }
}