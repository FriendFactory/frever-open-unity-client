using UnityEngine;

namespace Common
{
    [CreateAssetMenu(fileName = "AnimationCurve", menuName = "ScriptableObjects/AnimationCurve")]
    public sealed class AnimationCurveScriptableObject: ScriptableObject
    {
        public AnimationCurve Curve;
    }
}