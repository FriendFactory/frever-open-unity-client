using System.Collections.Generic;
using UnityEngine;

namespace Modules.CameraSystem.CameraAnimations.Template
{
    internal sealed class PanDownSpinSpiralAnimation : ZoomAnimation
    {
        private static readonly IReadOnlyDictionary<CameraAnimationProperty, float> PropertyAnchors =  new Dictionary<CameraAnimationProperty, float>
        {
            {CameraAnimationProperty.Dutch, -180},
            {CameraAnimationProperty.AxisY, 0},
            {CameraAnimationProperty.OrbitRadius, 1},
            {CameraAnimationProperty.HeightRadius, 2}
        };

        protected override IReadOnlyDictionary<CameraAnimationProperty, float> PropertyValueAnchor => PropertyAnchors;
        
        public override float MaxSpeed => SpeedToFinishAnimationInOneSecond;
        public override float DefaultSpeed => 100f;

        public PanDownSpinSpiralAnimation(CameraAnimationProperty[] propertiesToAnimate, IDictionary<CameraAnimationProperty, AnimationCurve> animationCurves) 
            : base(propertiesToAnimate, animationCurves)
        {
        }
    }
}
