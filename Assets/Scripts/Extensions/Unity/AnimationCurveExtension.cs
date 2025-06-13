using System.Linq;
using UnityEngine;

namespace Extensions
{
    public static class AnimationCurveExtension
    {
        public static void UpdateDuration(this AnimationCurve curve, float newDuration)
        {
            if (curve.keys.Length == 0) return;
            
            var currentDuration = curve.keys.Last().time;
            
            for (var i = 0; i < curve.keys.Length; i++)
            {
                var durationMultiplier = curve.keys[i].time/currentDuration;
                var newTime = newDuration * durationMultiplier;
                var keyFrameValue = curve.keys[i].value;
                curve.RemoveKey(i);
                curve.AddKey(newTime, keyFrameValue);
            }
        }
    }
}
