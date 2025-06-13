using System.Collections.Generic;
using System.Linq;
using Extensions;
using UnityEngine;

namespace Modules.CameraSystem.CameraAnimations.Template
{
    internal class ZoomAnimation : EditableClip
    {
        private const float MAX_ANIMATION_SPEED_ALLOWED = 22.5f;
        private const float TIME_BUFFER = 0.001f; 
        
        private static readonly IReadOnlyDictionary<CameraAnimationProperty, float> PropertyAnchors =  new Dictionary<CameraAnimationProperty, float>
        {
            {CameraAnimationProperty.OrbitRadius, 1},
            {CameraAnimationProperty.HeightRadius, 2}
        };
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        protected float SpeedToFinishAnimationInOneSecond => GetSpeedToCompleteAnimationIn(1);
        public override float MaxSpeed => Mathf.Clamp(SpeedToFinishAnimationInOneSecond, 0, MAX_ANIMATION_SPEED_ALLOWED);
        public override float DefaultSpeed => 1.5f;

        protected override IReadOnlyDictionary<CameraAnimationProperty, float> PropertyValueAnchor => PropertyAnchors;
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public ZoomAnimation(CameraAnimationProperty[] propertiesToAnimate, IDictionary<CameraAnimationProperty, AnimationCurve> animationCurves) 
            : base(propertiesToAnimate, animationCurves)
        {
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override void StartFrom(CameraAnimationFrame startFromFrame)
        {
            base.StartFrom(startFromFrame);
            RefreshAnimationDuration();
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected void RefreshAnimationDuration()
        {
            // Duration if speed is 1
            // TIME_BUFFER is used to prevent frames from being overwritten and removed because frames get same time when updating duration. 
            var unScaledDuration = Mathf.Clamp(GetMaxAnimatedDistance(), TIME_BUFFER, float.MaxValue);
            
            foreach (var animationCurve in AnimationCurves)
            {
                if (ShouldSkip(animationCurve.Key)) continue;
                animationCurve.Value.UpdateDuration(unScaledDuration);
            }

            RefreshDuration();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        /// <summary>
        /// Max distance the animation will travel in a animated property.
        /// Used to adjust the max speed and duration of the animation.
        /// </summary>
        private float GetMaxAnimatedDistance()
        {
            float delta = 0;
            foreach (var property in PropertiesToAnimate)
            {
                var firstKeyFrame = AnimationCurves[property].keys[0];
                foreach (var keyFrame in AnimationCurves[property].keys)
                {
                    var deltaValueFirstAndCurrentFrame = Mathf.Abs(firstKeyFrame.value - keyFrame.value);
                    if (delta > deltaValueFirstAndCurrentFrame) continue;
                    delta = deltaValueFirstAndCurrentFrame;
                }
            }

            return delta;
        }
        
        private float GetSpeedToCompleteAnimationIn(float seconds)
        {
            var distance = GetMaxAnimatedDistance();
            return distance / seconds;
        }

        private void RefreshDuration()
        {
            var lastKeyFrameTime = AnimationCurves[PropertiesToAnimate.First()].keys.Last().time;
            Length = lastKeyFrameTime;
        }
    }
}
