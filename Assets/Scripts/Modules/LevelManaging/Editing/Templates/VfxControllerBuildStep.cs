using Extensions;
using Models;

namespace Modules.LevelManaging.Editing.Templates
{
    internal sealed class VfxControllerBuildStep: EventBuildStep
    {
        protected override void RunInternal()
        {
            if (Template.GetVfx() == null) return;
           
            AddControllerIfNotExist();
            ApplyVfxAsset();
            ApplyMetadata();
        }

        private void AddControllerIfNotExist()
        {
            if(Destination.GetVfxController() != null) return;
            Destination.SetVfxController(new VfxController());
        }

        private void ApplyVfxAsset()
        {
            Destination.SetVfx(Template.GetVfx());
        }

        private void ApplyMetadata()
        {
            var templateController = Template.GetVfxController();
            var destController = Destination.GetVfxController();

            destController.Looping = templateController.Looping;
            destController.ActivationCue = templateController.ActivationCue;
            destController.AnimationSpeed = templateController.AnimationSpeed;
            destController.EndCue = templateController.EndCue;
        }
    }
}