using System.Collections.Generic;
using UnityEngine;

namespace Modules.CameraSystem.CameraAnimations.Template
{
    internal sealed class ZoomOutSpinSpiralAnimation : ZoomOutAnimation
    {
        private const float MAX_ANIMATION_SPEED_ALLOWED = 360f;

        public ZoomOutSpinSpiralAnimation(CameraAnimationProperty[] propertiesToAnimate, IDictionary<CameraAnimationProperty, AnimationCurve> animationCurves) 
            : base(propertiesToAnimate, animationCurves)
        {
        }

        public override float MaxSpeed => Mathf.Clamp(SpeedToFinishAnimationInOneSecond, 0, MAX_ANIMATION_SPEED_ALLOWED);
        public override float DefaultSpeed => 15f;

    }
}
