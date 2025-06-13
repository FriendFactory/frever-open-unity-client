using System;
using System.Collections;
using Common;
using DG.Tweening;
using UnityEngine;

namespace UI.UIAnimators
{
    public class SequentialUiAnimationPlayer : BaseUiAnimationPlayer
    {
        private Coroutine _lastRoutine;

        public override void PlayShowAnimation(Action callback = null)
        {
            _lastRoutine = CoroutineSource.Instance.StartCoroutine(PlayShowSequence(callback));
        }

        private IEnumerator PlayShowSequence(Action callback)
        {
            foreach (var animator in animators)
            {
                yield return animator.PlayShowAnimation().WaitForCompletion();
            }
            
            callback?.Invoke();
        }

        public override void PlayHideAnimation(Action callback = null)
        {
            _lastRoutine = CoroutineSource.Instance.StartCoroutine(PlayHideSequence(callback));
        }

        private IEnumerator PlayHideSequence(Action callback)
        {
            foreach (var animator in animators)
            {
                yield return animator.PlayHideAnimation().WaitForCompletion();
            }
            
            callback?.Invoke();
        }

        public override void PlayShowAnimationInstant()
        {
            foreach (var animator in animators)
            {
                animator.PlayShowAnimationInstant();
            }
        }

        public override void PlayHideAnimationInstant()
        {
            foreach (var animator in animators)
            {
                animator.PlayHideAnimationInstant();
            }
        }

        public override void CancelCurrentAnimation()
        {
            if (null == _lastRoutine) return;
            
            StopCoroutine(_lastRoutine);
            _lastRoutine = null;
        }
    }
}