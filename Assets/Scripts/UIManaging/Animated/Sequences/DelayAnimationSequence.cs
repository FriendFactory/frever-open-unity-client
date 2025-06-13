using System;
using DG.Tweening;
using UnityEngine;

namespace UIManaging.Animated.Sequences
{
    [Serializable]
    public class DelayAnimationSequence : AnimationSequenceBase
    {
        [SerializeField] private float _time = 1.0f;

        public override Sequence Build()
        {
            return DOTween.Sequence();
        }

        public override void Play(GameObject animatedObject)
        {
            var sequence = DOTween.Sequence();

            sequence.AppendInterval(_time);

            sequence.Play();
        }
    }
}