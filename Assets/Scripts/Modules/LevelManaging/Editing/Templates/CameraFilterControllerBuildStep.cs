using Extensions;
using Models;

namespace Modules.LevelManaging.Editing.Templates
{
    internal sealed class CameraFilterControllerBuildStep: EventBuildStep
    {
        protected override void RunInternal()
        {
            if (Template.GetCameraFilterController() == null) return;
            
            AddCameraFilterController();
            ApplyCameraFilterAsset();
            ApplyMetadata();
        }

        private void AddCameraFilterController()
        {
            Destination.SetCameraFilterController(new CameraFilterController());
        }
        
        private void ApplyCameraFilterAsset()
        {
            var filter = Template.GetCameraFilter();
            var filterVariant = Template.GetCameraFilterVariant();
            Destination.SetCameraFilter(filter, filterVariant.Id);
        }

        private void ApplyMetadata()
        {
            var templateController = Template.GetCameraFilterController();
            var destController = Destination.GetCameraFilterController();

            destController.ActivationCue = templateController.ActivationCue;
            destController.EndCue = templateController.EndCue;
            destController.CameraFilterValue = templateController.CameraFilterValue;
            destController.StackNumber = templateController.StackNumber;
        }
    }
}