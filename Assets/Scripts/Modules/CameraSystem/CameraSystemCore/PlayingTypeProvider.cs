using Modules.CameraSystem.CameraAnimations.Players;
using Modules.CameraSystem.CameraAnimations.Template;

namespace Modules.CameraSystem.CameraSystemCore
{
    internal sealed class PlayingTypeProvider
    {
        public PlayingType GetPlayingType(TemplateCameraAnimationClip clip, TemplatePlayingMode mode)
        {
            return mode == TemplatePlayingMode.Teaser? GetPlayingTypeForTeaser(clip): GetPlayingTypeForTemplate(clip);
        }
        
        private PlayingType GetPlayingTypeForTemplate(TemplateCameraAnimationClip clip)
        {
            if (clip.IsReversible)
                return PlayingType.BouncingLoop;

            return clip.IsLoopable ? PlayingType.Loop : PlayingType.OneTimeAndKeepPlayingLastFrame;
        }
        
        private PlayingType GetPlayingTypeForTeaser(TemplateCameraAnimationClip clip)
        {
            if (!clip.IsLoopable)
            {
                return clip.IsReversible
                    ? PlayingType.OneTimeBouncingAndStop
                    : PlayingType.OneTimeAndStop;
            }

            return clip.IsReversible ? PlayingType.BouncingLoop : PlayingType.Loop;
        }

        public PlayingType GetPlayingTypeForRecordedAnimation()
        {
            return PlayingType.OneTimeAndKeepPlayingLastFrame;
        }
    }
}