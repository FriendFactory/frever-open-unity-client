using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Modules.CameraSystem.CameraAnimations.Template
{
    internal abstract class EditableClip : TemplateCameraAnimationClip
    {
        private static readonly IReadOnlyDictionary<CameraAnimationProperty, float> EmptyDictionary = new Dictionary<CameraAnimationProperty, float>();
        protected virtual IReadOnlyDictionary<CameraAnimationProperty, float> PropertyValueAnchor => EmptyDictionary;

        private readonly CameraAnimationProperty[] _ignoreProperties =
        {
            CameraAnimationProperty.PositionX,
            CameraAnimationProperty.PositionY,
            CameraAnimationProperty.PositionZ,
            CameraAnimationProperty.RotationX,
            CameraAnimationProperty.RotationY,
            CameraAnimationProperty.RotationZ
        };

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        protected EditableClip(CameraAnimationProperty[] propertiesToAnimate, IDictionary<CameraAnimationProperty, AnimationCurve> animationCurves) 
            : base(propertiesToAnimate, animationCurves)
        {
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override void StartFrom(CameraAnimationFrame startFromFrame)
        {
            UpdateAnimationWithNewStartFrame(startFromFrame);
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected void SetKeyFrame(AnimationCurve curve, int keyIndex, float time, float value)
        {
            curve.RemoveKey(keyIndex);
            curve.AddKey(time, value);
        }
        
        protected bool ShouldSkip(CameraAnimationProperty property)
        {
            return _ignoreProperties.Contains(property);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void UpdateAnimationWithNewStartFrame(CameraAnimationFrame startFromFrame)
        {
            foreach (var property in startFromFrame.Select(x=>x.Key).Where(x=> !ShouldSkip(x)))
            {
                if(!AnimationCurves.ContainsKey(property)) AnimationCurves.Add(property, new AnimationCurve());
                
                UpdateKeyFramesWithNewStartFrame(AnimationCurves[property], startFromFrame, property);
            }
        }

        private void UpdateKeyFramesWithNewStartFrame(AnimationCurve curve, CameraAnimationFrame newStartFrame, CameraAnimationProperty property)
        {
            if (curve.keys.Length == 0)
            {
                curve.AddKey(0, newStartFrame.GetValue(property));
                return;
            }
            
            var currentCameraAnimationFrame = GetFrame(0);
            var valueToAdd = CalculateFrameDelta(currentCameraAnimationFrame, newStartFrame, property);
            AddValueToKeyFrames(curve, valueToAdd, property);
        }

        private void AddValueToKeyFrames(AnimationCurve curve, float valueToAdd, CameraAnimationProperty property)
        {
            for (var i = 0; i < curve.keys.Length; i++)
            {
                var currentKeyFrameValue = curve.keys[i].value;
                var isFirstKeyFrame = i == 0;

                if(IsKeyFrameValueAnchored(property, currentKeyFrameValue) && !isFirstKeyFrame) continue;

                var newFrameValue = currentKeyFrameValue + valueToAdd;
                var keyFrameTime = curve.keys[i].time;
                SetKeyFrame(curve, i, keyFrameTime, newFrameValue);
            }
        }

        private float CalculateFrameDelta(CameraAnimationFrame currentFrame, CameraAnimationFrame targetFrame, CameraAnimationProperty property)
        {
            return targetFrame.GetValue(property) - currentFrame.GetValue(property);
        }

        /// <summary>
        /// Used to check keyframes we would not like to update value on. Destination points for animations are usually anchored points.
        /// Example: Pan Down animation has a YAxis = 0 keyframe destination point. This keyframe value we would like to never change because we always want it to move towards that destination.
        /// </summary>
        private bool IsKeyFrameValueAnchored(CameraAnimationProperty property, float keyFrameValue)
        {
            if (PropertyValueAnchor == null || !PropertyValueAnchor.TryGetValue(property, out var anchorValue))
                return false;
            
            return Math.Abs(anchorValue - keyFrameValue) < 0.0001f;
        }
    }
}