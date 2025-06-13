using Modules.CameraSystem.CameraAnimations;
using Modules.CameraSystem.CameraAnimations.Template;

namespace Modules.CameraSystem.Extensions
{
    internal static class CameraAnimationExtension
    {
        public static bool IsTemplate(this CameraAnimationClip clip)
        {
            return clip is TemplateCameraAnimationClip;
        }
    }
}