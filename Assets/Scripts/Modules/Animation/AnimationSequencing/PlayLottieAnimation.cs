using System;
using BrunoMikoski.AnimationSequencer;
using DG.Tweening;
using Modules.Animation.Lottie;
using UnityEngine;

namespace Modules.AnimationSequencing
{
    [Serializable]
    public class PlayLottieAnimation : AnimationStepBase
    {
        [SerializeField] private LottieAnimationPlayer _animation;
        [SerializeField] private float _duration = 1f;
    
        public override string DisplayName => "Play Lottie Animation";

        public override void AddTweenToSequence(Sequence animationSequence)
        {
            animationSequence.AppendInterval(Delay);
            animationSequence.AppendCallback(() =>
                {
                    _animation.Rewind();
                    _animation.Play();
                }
            );
            animationSequence.AppendInterval(_duration);
        }

        public override void ResetToInitialState()
        {
            _animation.Rewind();
        }
    }
}
