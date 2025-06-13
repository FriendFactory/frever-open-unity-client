using System;
using Common;
using UnityEngine;

#pragma warning disable CS0649

namespace UI.UIAnimators
{
    public class ParallelUiAnimationPlayer : BaseUiAnimationPlayer
    {
        private Coroutine _lastRoutine;
        
        private float GetLongestAnimationDuration()
        {
            var maxDuration = 0f;

            if(animators != null)
            { 
                for (int i = 0; i < animators.Length; i++)
                {
                    if (animators[i] == null) continue;
                    if (maxDuration < animators[i].Duration)
                    {
                        maxDuration = animators[i].Duration;
                    }
                }
            }
            return maxDuration;
        }

        public override void PlayShowAnimation(Action callback = null)
        {
            CancelCurrentAnimation();
            
            for (int i = 0; i < animators.Length; i++)
            {
                animators[i].PlayShowAnimation();
            }

            _lastRoutine = CoroutineSource.Instance.ExecuteWithDelay(GetLongestAnimationDuration(), () => callback?.Invoke());
        }

        public override void PlayHideAnimation(Action callback = null)
        {
            CancelCurrentAnimation();
            
            for (int i = 0; i < animators.Length; i++)
            {
                animators[i].PlayHideAnimation();
            }
            
            _lastRoutine = CoroutineSource.Instance.ExecuteWithDelay(GetLongestAnimationDuration(), () => callback?.Invoke());
        }

        public override void PlayShowAnimationInstant()
        {
            CancelCurrentAnimation();
            
            for (int i = 0; i < animators.Length; i++)
            {
                animators[i].PlayShowAnimationInstant();
            }
        }

        public override void PlayHideAnimationInstant()
        {
            CancelCurrentAnimation();
            
            for (int i = 0; i < animators.Length; i++)
            {
                animators[i].PlayHideAnimationInstant();
            }
        }

        public override void CancelCurrentAnimation()
        {
            if (_lastRoutine == null) return;
            
            CoroutineSource.Instance.StopCoroutine(_lastRoutine);
            _lastRoutine = null;
        }
    }
}
