using System.Collections.Generic;
using UnityEngine;

namespace Modules.CameraSystem.CameraAnimations
{
    public sealed class RecordedCameraAnimationClip : CameraAnimationClip
    {
        internal override AnimationType AnimationType => AnimationType.TransformBased;
        
        public RecordedCameraAnimationClip(IDictionary<CameraAnimationProperty, AnimationCurve> animationCurves) : base(animationCurves)
        {
        }
    }
}