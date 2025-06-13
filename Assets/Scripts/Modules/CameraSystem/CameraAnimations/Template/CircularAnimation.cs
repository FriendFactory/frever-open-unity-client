using System.Collections.Generic;
using UnityEngine;

namespace Modules.CameraSystem.CameraAnimations.Template
{
    internal class CircularAnimation : EditableClip
    {
        public CircularAnimation(CameraAnimationProperty[] propertiesToAnimate, IDictionary<CameraAnimationProperty, AnimationCurve> animationCurves) 
            : base(propertiesToAnimate, animationCurves)
        {
            IsLoopable = true;
            IsEndlessPlayable = true;
        }

        public override float MaxSpeed => 100f;
        public override float DefaultSpeed => 15f;

        public override float GetValueAtTime(CameraAnimationProperty property, float time)
        {
            if (time > Length)
            {
                time %= Length;
            }
            return base.GetValueAtTime(property, time);
        }
    }
}
