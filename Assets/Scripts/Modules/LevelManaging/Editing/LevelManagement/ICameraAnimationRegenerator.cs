using Modules.CameraSystem.CameraAnimations;

namespace Modules.LevelManaging.Editing.LevelManagement
{
    public interface ICameraAnimationRegenerator
    {
        void ReGenerateAnimationForTargetEvent();
        void ReGenerateAnimationProperties(params CameraAnimationProperty[] props);
    }
}