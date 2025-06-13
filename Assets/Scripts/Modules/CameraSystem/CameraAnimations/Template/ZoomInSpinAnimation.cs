using System.Collections.Generic;
using UnityEngine;

namespace Modules.CameraSystem.CameraAnimations.Template
{
    internal sealed class ZoomInSpinAnimation : ZoomAnimation
    {
        public ZoomInSpinAnimation(CameraAnimationProperty[] propertiesToAnimate, IDictionary<CameraAnimationProperty, AnimationCurve> animationCurves) 
            : base(propertiesToAnimate, animationCurves)
        {
        }

        public override float MaxSpeed => Mathf.Clamp(SpeedToFinishAnimationInOneSecond, 0, 360f);
        public override float DefaultSpeed => 15f;
    }
}
