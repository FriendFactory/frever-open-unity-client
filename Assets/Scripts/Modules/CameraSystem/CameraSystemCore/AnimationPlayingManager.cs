using Modules.CameraSystem.CameraAnimations;
using Modules.CameraSystem.CameraAnimations.Players;
using Modules.CameraSystem.CameraAnimations.Template;
using Modules.CameraSystem.Extensions;

namespace Modules.CameraSystem.CameraSystemCore
{
    internal sealed class AnimationPlayingManager
    {
        private const float RECORDED_CLIP_PLAYBACK_SPEED = 1;
        
        private readonly CameraAnimator _animator;
        private readonly PlayingTypeProvider _playingTypeProvider;
        private readonly ICameraTemplatesManager _cameraTemplatesManager;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public AnimationPlayingManager(CameraAnimator animator, PlayingTypeProvider playingTypeProvider, ICameraTemplatesManager cameraTemplatesManager)
        {
            _animator = animator;
            _playingTypeProvider = playingTypeProvider;
            _cameraTemplatesManager = cameraTemplatesManager;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void PlayTemplateAnimation(TemplateCameraAnimationClip clip, TemplatePlayingMode mode)
        {
            var playingType = _playingTypeProvider.GetPlayingType(clip, mode);
            SetPlayingType(playingType);
            Play(clip);
        }
        
        public void PlayAnimation(RecordedCameraAnimationClip target)
        {
            var playingType = _playingTypeProvider.GetPlayingTypeForRecordedAnimation();
            SetPlayingType(playingType);
            Play(target);
        }

        public void PauseAnimation()
        {
            _animator.Pause();
        }

        public void ResumeAnimation()
        {
            _animator.Resume();
        }

        public void StopAnimation(bool returnToStartPosition = true)
        {
            _animator.SetResetAnimationOnStop(returnToStartPosition);
            _animator.Stop();
        }

        public void SetCameraOnStartPosition(CameraAnimationClip clip)
        {
            _animator.SetAnimation(clip);
            _animator.SetAnimationOnFirstFrame();
        }
        
        public void SetCameraOnEndPosition(CameraAnimationClip clip)
        {
            _animator.SetAnimation(clip);
            _animator.SetAnimationOnLastFrame();
        }

        public void Simulate(CameraAnimationClip clip, float time)
        {
            _animator.Stop();
            _animator.SetAnimation(clip);
            SetAnimatorPlaybackSpeed(clip);
            _animator.Simulate(time);
        }

        public void SimulateTemplate(TemplateCameraAnimationClip clip, float time)
        {
            var playingType = _playingTypeProvider.GetPlayingType(clip, TemplatePlayingMode.Continuous);
            _animator.SetPlayingType(playingType);
            Simulate(clip, time);
        }

        public void Simulate(CameraAnimationFrame frame)
        {
            _animator.Stop();
            _animator.Simulate(frame);
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void SetPlayingType(PlayingType type)
        {
            _animator.SetPlayingType(type);
        }
         
        private void Play(CameraAnimationClip target)
        {
            _animator.SetResetAnimationOnStop(false);
            _animator.SetAnimation(target);
            SetAnimatorPlaybackSpeed(target);
            _animator.Play();
        }

        private void SetAnimatorPlaybackSpeed(CameraAnimationClip target)
        {
            var speed = GetPlaybackSpeed(target);
            _animator.SetSpeed(speed);
        }
        
        private float GetPlaybackSpeed(CameraAnimationClip clip)
        {
            return clip.IsTemplate() ? _animator.PlaybackSpeed : RECORDED_CLIP_PLAYBACK_SPEED;
        }
    }
}