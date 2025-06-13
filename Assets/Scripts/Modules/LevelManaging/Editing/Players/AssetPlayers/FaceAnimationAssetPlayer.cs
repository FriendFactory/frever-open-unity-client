using Modules.FaceAndVoice.Face.Playing.Core;
using Modules.LevelManaging.Assets;

namespace Modules.LevelManaging.Editing.Players.AssetPlayers
{
    internal sealed class FaceAnimationAssetPlayer: GenericTimeSourceDependAssetPlayer<IFaceAnimationAsset>
    {
        private FaceAnimPlayer[] _targetFaces;

        private FaceAnimationClip FaceAnimationClip => Target.FaceAnimationClip;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void SetTargetFaces(params FaceAnimPlayer[] targetFaces)
        {
            _targetFaces = targetFaces;
        }

        public override void Simulate(float time)
        {
            foreach (var face in _targetFaces)
            {
                face.SetClip(FaceAnimationClip);
                face.Simulate(time);
            }
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnPlay()
        {
            foreach (var face in _targetFaces)
            {
                face.SetClip(FaceAnimationClip);
                face.Play(TimeSource);
            }
        }

        protected override void OnPause()
        {
            foreach (var face in _targetFaces)
            {
                face.Pause();
            }
        }

        protected override void OnResume()
        {
            foreach (var face in _targetFaces)
            {
                face.Resume();
            }
        }

        protected override void OnStop()
        {
            foreach (var face in _targetFaces)
            {
                face.Stop();
            }
        }
    }
}