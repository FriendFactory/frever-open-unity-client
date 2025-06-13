using System;
using System.Linq;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui
{
    public interface ICameraFocusAnimationCurveProvider
    {
        AnimationCurve GetAnimationCurve(long raceId);
    }
    
    [CreateAssetMenu(fileName = "CameraFocusAnimationCurves", menuName = "Friend Factory/Scriptable Objects/Camera Focus Animation Curves")]
    internal sealed class CameraFocusAnimationCurveProvider: ScriptableObject, ICameraFocusAnimationCurveProvider
    {
        [SerializeField] private RaceAnimationCurve[] _focusAnimationCurves;

        public AnimationCurve GetAnimationCurve(long raceId)
        {
            return _focusAnimationCurves.First(x => x.RaceId == raceId).AnimationCurve;
        }

        [Serializable]
        private struct RaceAnimationCurve
        {
            public long RaceId;
            public AnimationCurve AnimationCurve;
        }
    }
}