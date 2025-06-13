using Extensions;
using Models;

namespace Modules.LevelManaging.Editing.Templates
{
    internal sealed class MusicControllerBuildStep: EventBuildStep
    {
        protected override void RunInternal()
        {
            if (Template.GetMusicController() == null) return;
            
            AddControllerIfNotExist();
            ApplyAssets();
            ApplyMetadata();
        }

        private void AddControllerIfNotExist()
        {
            if (Destination.GetMusicController() != null) return;
            Destination.SetMusicController(new MusicController());
        }

        private void ApplyAssets()
        {
            Destination.SetSong(Template.GetSong());
            Destination.SetUserSound(Template.GetUserSound());
            Destination.SetExternalTrack(Template.GetExternalTrack());
        }

        private void ApplyMetadata()
        {
            var templateController = Template.GetMusicController();
            var destController = Destination.GetMusicController();

            destController.ActivationCue = templateController.ActivationCue;
            destController.EndCue = templateController.EndCue;
            destController.LevelSoundVolume = templateController.LevelSoundVolume;
        }
    }
}