using System.Collections.Generic;
using UnityEngine;

namespace Modules.CameraSystem.CameraAnimations.Template
{
    internal sealed class SlideSpin : EditableClip
    {
        public SlideSpin(CameraAnimationProperty[] propertiesToAnimate, IDictionary<CameraAnimationProperty, AnimationCurve> animationCurves) 
            : base(propertiesToAnimate, animationCurves)
        {
        }

        public override float MaxSpeed => 180f;
        public override float DefaultSpeed => 15f;
    }
}
