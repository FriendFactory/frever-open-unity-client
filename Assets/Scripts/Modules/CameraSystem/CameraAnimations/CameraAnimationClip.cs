using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Modules.CameraSystem.CameraAnimations
{
    public abstract class CameraAnimationClip
    {
        private static readonly int CAMERA_PROPERTIES_COUNT = Enum.GetValues(typeof(CameraAnimationProperty)).Length;

        public readonly IDictionary<CameraAnimationProperty, AnimationCurve> AnimationCurves;

        public float Length { get; protected set; }
        internal abstract AnimationType AnimationType { get; }
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        protected CameraAnimationClip(IDictionary<CameraAnimationProperty, AnimationCurve> animationCurves)
        {
            AnimationCurves = animationCurves;
            SetupDuration();
        }

        private void SetupDuration()
        {
            var anyCurve = AnimationCurves.First().Value;
            var lastCurveKey = anyCurve[anyCurve.length - 1];
            Length = lastCurveKey.time;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public virtual float GetValueAtTime(CameraAnimationProperty property, float time)
        {
            return AnimationCurves[property].Evaluate(time);
        }

        public CameraAnimationFrame GetFrame(float time)
        {
            var frameData = new Dictionary<CameraAnimationProperty, float>(CAMERA_PROPERTIES_COUNT);
            GetValuesAtTime(time, ref frameData);
            return new CameraAnimationFrame(frameData);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void GetValuesAtTime(float time, ref Dictionary<CameraAnimationProperty, float> destination)
        {
            destination.Clear();
            foreach (var property in AnimationCurves.Keys)
            {
                destination.Add(property, GetValueAtTime(property, time));
            }
        }
    }
}