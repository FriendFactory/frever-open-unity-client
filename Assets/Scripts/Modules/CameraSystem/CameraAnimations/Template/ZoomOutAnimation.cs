using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Modules.CameraSystem.CameraAnimations.Template
{
    internal class ZoomOutAnimation : ZoomAnimation
    {
        public ZoomOutAnimation(CameraAnimationProperty[] propertiesToAnimate, IDictionary<CameraAnimationProperty, AnimationCurve> animationCurves) 
            : base(propertiesToAnimate, animationCurves)
        {
        }

        public override void StartFrom(CameraAnimationFrame startFromFrame)
        {
            base.StartFrom(startFromFrame);
            UpdateLastKeyFrameOnRadiusProperties(MaxOrbitRadius);
            RefreshAnimationDuration();
        }
        
        private void UpdateLastKeyFrameOnRadiusProperties(float lastFrameValue)
        {
            var radiusProperties = new[] {CameraAnimationProperty.OrbitRadius, CameraAnimationProperty.HeightRadius};
            foreach (var property in radiusProperties)
            {
                var lastKeyIndex = AnimationCurves[property].keys.Length - 1;
                var lastKeyFrameTime = AnimationCurves[property].keys.Last().time;
                SetKeyFrame(AnimationCurves[property], lastKeyIndex, lastKeyFrameTime, lastFrameValue);
            }
        }
    }
}
