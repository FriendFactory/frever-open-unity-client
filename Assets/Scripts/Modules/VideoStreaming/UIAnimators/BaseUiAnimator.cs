using System;
using DG.Tweening;
using UnityEngine;

namespace UI.UIAnimators
{
    public abstract class BaseUiAnimator : MonoBehaviour
    {
        [SerializeField] protected float duration = 0.35f;
        [SerializeField] protected AnimationCurve easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public float Duration => duration;

        public abstract Sequence PlayShowAnimation(Action callback = null);
        public abstract Sequence PlayHideAnimation(Action callback = null);
        public abstract void PlayShowAnimationInstant();
        public abstract void PlayHideAnimationInstant();
    }
}