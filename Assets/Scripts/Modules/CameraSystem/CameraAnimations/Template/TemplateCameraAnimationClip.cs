using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Modules.CameraSystem.CameraAnimations.Template
{
    public abstract class TemplateCameraAnimationClip : CameraAnimationClip
    {
        private const float MIN_SPEED = 0f;

        protected readonly CameraAnimationProperty[] PropertiesToAnimate;

        public float MinSpeed => MIN_SPEED;
        public abstract float MaxSpeed { get; }
        public abstract float DefaultSpeed { get; }
        public bool IsReversible { get; set; }
        public bool IsLoopable { get; set; }
        public bool IsEndlessPlayable { get; set; }
        public long Id { get; set; }
        public bool SpeedIsAdjustable { get; set; } = true;
        public float MaxOrbitRadius { get; set; }
        internal override AnimationType AnimationType => AnimationType.CinemachineBased;

        protected TemplateCameraAnimationClip(CameraAnimationProperty[] propertiesToAnimate, IDictionary<CameraAnimationProperty, AnimationCurve> animationCurves) 
            : base(animationCurves)
        {
            PropertiesToAnimate = propertiesToAnimate;
        }

        public bool DoesAnimateProperty(CameraAnimationProperty property)
        {
            return PropertiesToAnimate.Contains(property);
        }

        public abstract void StartFrom(CameraAnimationFrame startFromFrame);
    }
}