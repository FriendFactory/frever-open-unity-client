using JetBrains.Annotations;
using Modules.CameraSystem.CameraSystemCore;
using Modules.LevelManaging.Assets;

namespace Modules.LevelManaging.Editing.Players.AssetPlayers
{
    [UsedImplicitly]
    internal sealed class CameraAnimationPlayer : AssetPlayerBase<ICameraAnimationAsset>
    {
        private readonly ICameraSystem _cameraSystem;
        
        public CameraAnimationPlayer(ICameraSystem cameraSystem)
        {
            _cameraSystem = cameraSystem;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void SetCameraOnLastPosition()
        {
            _cameraSystem.SetCameraOnEndPosition(Target.Clip);
        }

        public override void Simulate(float time)
        {
            _cameraSystem.Simulate(Target.Clip, time);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnPlay()
        {
            _cameraSystem.Enable(true);
            _cameraSystem.PlayAnimation(Target.Clip);
        }

        protected override void OnPause()
        {
            _cameraSystem.PauseAnimation();
        }

        protected override void OnResume()
        {
            _cameraSystem.ResumeAnimation();
        }

        protected override void OnStop()
        {
            _cameraSystem.StopAnimation(false);
        }
    }
}