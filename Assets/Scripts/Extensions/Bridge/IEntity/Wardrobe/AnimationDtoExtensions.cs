using Bridge.Models.ClientServer.Assets;
using UnityEngine;

namespace Extensions.Wardrobe
{
    public static class AnimationDtoExtensions
    {
        public static AnimationCurve ToAnimationCurve(this AnimationCurveDto dto)
        {
            var animCurve = new AnimationCurve();
            animCurve.preWrapMode = WrapMode.ClampForever;
            animCurve.postWrapMode = WrapMode.ClampForever;
            foreach (var frame in dto.Keys)
            {
                var keyFrame = new UnityEngine.Keyframe(frame.Time, frame.Value, frame.InTangent, frame.OutTangent, frame.InWeight, frame.OutWeight);
                animCurve.AddKey(keyFrame);
            }

            return animCurve;
        }
    }
}