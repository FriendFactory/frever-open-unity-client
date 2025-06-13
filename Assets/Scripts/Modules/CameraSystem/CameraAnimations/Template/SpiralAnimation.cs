using System.Collections.Generic;
using UnityEngine;

namespace Modules.CameraSystem.CameraAnimations.Template
{
    internal sealed class SpiralAnimation : CircularAnimation
    {
        private static readonly IReadOnlyDictionary<CameraAnimationProperty, float> PropertyAnchors =  new Dictionary<CameraAnimationProperty, float>
        {
            {CameraAnimationProperty.OrbitRadius, 2},
            {CameraAnimationProperty.HeightRadius, 2}
        };

        protected override IReadOnlyDictionary<CameraAnimationProperty, float> PropertyValueAnchor => PropertyAnchors;

        public SpiralAnimation(CameraAnimationProperty[] propertiesToAnimate, IDictionary<CameraAnimationProperty, AnimationCurve> animationCurves) 
            : base(propertiesToAnimate, animationCurves)
        {
        }
    }
}
