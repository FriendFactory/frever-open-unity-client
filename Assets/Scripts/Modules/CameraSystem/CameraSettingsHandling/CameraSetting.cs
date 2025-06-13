using Modules.CameraSystem.CameraAnimations;

namespace Modules.CameraSystem.CameraSettingsHandling
{
    public sealed class CameraSetting
    {
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public long Id { get; set; }
        public float OrbiRadiusStart { get; set; }
        public float OrbitRadiusMaxDefault { get; set; }
        public float FoVMax { get; set; } = 70f;
        public float FoVMin { get; set; } = 20f;
        public float FoVStart { get; set; } = 0;
        public float FoVCurrent { get; set; }
        public float DepthOfFieldMax { get; set; }
        public float DepthOfFieldMin { get; set; }
        public float DepthOfFieldStart { get; set; }
        public float OrbitRadiusMin { get; set; }
        public float OrbitRadiusMax { get; set; }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public float GetStartValueWithProperty(CameraAnimationProperty property)
        {
            switch (property)
            {
                case CameraAnimationProperty.OrbitRadius:
                    return OrbiRadiusStart;
                case CameraAnimationProperty.FieldOfView:
                   return FoVStart;
                case CameraAnimationProperty.DepthOfField:
                    return DepthOfFieldStart;
                default:
                    return 0;
            }
        }
    }
}