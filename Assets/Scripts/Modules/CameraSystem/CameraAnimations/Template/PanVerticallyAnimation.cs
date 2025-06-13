using System.Collections.Generic;
using UnityEngine;

namespace Modules.CameraSystem.CameraAnimations.Template
{
    internal sealed class PanVerticallyAnimation : EditableClip
    {
        public PanVerticallyAnimation(CameraAnimationProperty[] propertiesToAnimate, IDictionary<CameraAnimationProperty, AnimationCurve> animationCurves) 
            : base(propertiesToAnimate, animationCurves)
        {
        }

        public override float MaxSpeed => 2f;
        public override float DefaultSpeed => 0.1f;
        
    }
}
