using DG.Tweening;
using UnityEngine;

namespace UIManaging.Animated.Sequences
{
    public struct AnimatePositionModel
    {
        public bool UseLocalPosition;
        public bool UseRelativePosition;
        public bool UseCurrentPosition;
        public bool UseAnchoredPosition;

        public float Time;

        public Vector3 FromPosition;
        public Vector3 ToPosition;

        public Ease Ease;
    }
}