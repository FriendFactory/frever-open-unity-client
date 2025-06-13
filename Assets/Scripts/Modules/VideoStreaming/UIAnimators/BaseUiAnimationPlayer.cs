using System;
using UnityEngine;

namespace UI.UIAnimators
{
    public abstract class BaseUiAnimationPlayer : MonoBehaviour
    {
        [SerializeField] protected BaseUiAnimator[] animators;
        
        public abstract void PlayShowAnimation(Action callback = null);
        public abstract void PlayHideAnimation(Action callback = null);
        public abstract void PlayShowAnimationInstant();
        public abstract void PlayHideAnimationInstant();
        public abstract void CancelCurrentAnimation();

        private void OnDestroy()
        {
            CancelCurrentAnimation();
        }
    }
}