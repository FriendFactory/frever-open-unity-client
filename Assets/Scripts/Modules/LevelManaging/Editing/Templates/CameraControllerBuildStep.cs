using System.Collections.Generic;
using Extensions;
using Models;

namespace Modules.LevelManaging.Editing.Templates
{
    internal sealed class CameraControllerBuildStep: EventBuildStep
    {
        protected override void RunInternal()
        {
            var templateController = Template.GetCameraController();
            Destination.CameraController = new List<CameraController>();
            var controller = new CameraController();
            Destination.SetCameraController(controller);
            controller.ActivationCue = templateController.ActivationCue;
            controller.EndCue = templateController.EndCue;
            controller.TemplateSpeed = templateController.TemplateSpeed;
            controller.CameraAnimationTemplateId = templateController.CameraAnimationTemplateId;
            controller.StartFocusDistance = templateController.StartFocusDistance;
            controller.EndFocusDistance = templateController.EndFocusDistance;
            controller.EndDepthOfFieldOffset = templateController.EndDepthOfFieldOffset;
            controller.CameraNoiseSettingsIndex = templateController.CameraNoiseSettingsIndex;
            controller.LookAtIndex = templateController.LookAtIndex;
            controller.FollowAll = templateController.FollowAll;
            controller.FollowZoom = templateController.FollowZoom;
            controller.FollowSpawnPositionIndex = templateController.FollowSpawnPositionIndex;
        }
    }
}