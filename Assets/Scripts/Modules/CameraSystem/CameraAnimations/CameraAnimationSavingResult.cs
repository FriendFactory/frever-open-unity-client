namespace Modules.CameraSystem.CameraAnimations
{
    public struct CameraAnimationSavingResult
    {
        public readonly string FilePath;
        public readonly string AnimationString;

        public CameraAnimationSavingResult(string filePath, string animationString)
        {
            FilePath = filePath;
            AnimationString = animationString;
        }
    }
}