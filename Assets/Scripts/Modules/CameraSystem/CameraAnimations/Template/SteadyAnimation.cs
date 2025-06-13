using System.Collections.Generic;
using UnityEngine;

namespace Modules.CameraSystem.CameraAnimations.Template
{
    internal sealed class SteadyAnimation : EditableClip
    {
        public SteadyAnimation(CameraAnimationProperty[] propertiesToAnimate, IDictionary<CameraAnimationProperty, AnimationCurve> animationCurves) 
            : base(propertiesToAnimate, animationCurves)
        {
        }

        public override float MaxSpeed => 0f;
        public override float DefaultSpeed => 0f;
    }
}
