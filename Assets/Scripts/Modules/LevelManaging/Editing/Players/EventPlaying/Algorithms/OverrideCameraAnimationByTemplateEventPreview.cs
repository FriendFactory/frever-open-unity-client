using System.Collections.Generic;
using System.Linq;
using Common.TimeManaging;
using Extensions;
using JetBrains.Annotations;
using Modules.CameraSystem.CameraSystemCore;
using Modules.LevelManaging.Editing.Players.AssetPlayers;

namespace Modules.LevelManaging.Editing.Players.EventPlaying.Algorithms
{
    /// <summary>
    /// Overrides camera animation asset by template
    /// Can be used for camera animation editing in PiP screen, when user chooses other template types
    /// </summary>
    [UsedImplicitly]
    internal sealed class OverrideCameraAnimationByTemplateEventPreview : EventSingleLoopPreviewAlgorithmBase
    {
        private readonly ICameraSystem _cameraSystem;
        private readonly ICameraTemplatesManager _cameraTemplatesManager;

        public OverrideCameraAnimationByTemplateEventPreview(IPlayersManager playerManager, EventAssetsProvider eventAssetsProvider, ITimeSourceControl eventTimeSourceControl, 
               ICameraSystem cameraSystem, ICameraTemplatesManager cameraTemplatesManager) 
            : base(playerManager, eventAssetsProvider, eventTimeSourceControl)
        {
            _cameraSystem = cameraSystem;
            _cameraTemplatesManager = cameraTemplatesManager;
        }

        public override void Simulate(float time, params DbModelType[] targetTypes)
        {
            base.Simulate(time, targetTypes);
            if (targetTypes.Contains(DbModelType.CameraAnimation))
            {
                SimulateTemplate(time);
            }
        }

        protected override void OnPlayStarted()
        {
            base.OnPlayStarted();
            _cameraSystem.Enable(true);
            var templateClip = _cameraTemplatesManager.CurrentTemplateClip;
            _cameraSystem.PlayTemplate(templateClip, TemplatePlayingMode.Continuous);
        }

        protected override void OnEventDone()
        {
            _cameraSystem.StopAnimation();
            base.OnEventDone();
        }

        protected override IReadOnlyCollection<DbModelType> GetAssetTypesToIgnore()
        {
            return new[] {DbModelType.CameraAnimation};
        }

        protected override void OnCanceled()
        {
            base.OnCanceled();
            _cameraSystem.StopAnimation();
        }

        private void SimulateTemplate(float time)
        {
            var currentTemplateClip = _cameraTemplatesManager.CurrentTemplateClip;
            _cameraSystem.Simulate(currentTemplateClip, time);
        }
    }
}