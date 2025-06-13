using System;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Modules.CameraSystem.CameraAnimations;
using Modules.CameraSystem.CameraAnimations.Template;

namespace Modules.CameraSystem.CameraSystemCore
{
    public interface ICameraTemplatesManager
    {
        CameraAnimationFrame TemplatesStartFrame { get; }
        TemplateCameraAnimationClip CurrentTemplateClip { get; }
        event Action<TemplateCameraAnimationClip> TemplateAnimationChanged;
        void SetupCameraAnimationTemplates(CameraAnimationTemplate[] models, long defaultTemplateId);
        void SetStartFrameForTemplates(CameraAnimationFrame frame);
        void SaveCurrentCameraStateAsStartFrameForTemplates();
        void UpdateStartPositionForTemplate(TemplateCameraAnimationClip targetClip);
        void ChangeTemplateAnimation(long id);
        TemplateCameraAnimationClip GetTemplateClipById(long id);
        void PrepareClipForSimulationFrame(CameraAnimationClip clip);
    }
}
