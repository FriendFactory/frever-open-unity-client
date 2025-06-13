using System.Collections.Generic;
using UnityEngine;

namespace Modules.CameraSystem.CameraAnimations.Template
{
    internal sealed class SpinAnimation : EditableClip
    {
        public SpinAnimation(CameraAnimationProperty[] propertiesToAnimate, IDictionary<CameraAnimationProperty, AnimationCurve> animationCurves) 
            : base(propertiesToAnimate, animationCurves)
        {
            IsEndlessPlayable = true;
            IsLoopable = true;
        }

        public override float MaxSpeed => 100f;
        public override float DefaultSpeed => 15f;
    }
}
