using System;
using DG.Tweening;
using Extensions;
using UI.UIAnimators;
using UnityEngine;

namespace Modules.VideoStreaming.UIAnimators
{
    public sealed class PageUiAnimator : RectTransformBasedUiAnimator
    {
        [SerializeField] private GameObject _inputBlocker;
        
        private PositionSettings _showPosition = new PositionSettings(new Vector2(0,0),new Vector2(1,0.5f), new Vector2(1,0.5f), new Vector2(0,0.5f) );
        private readonly PositionSettings _idlePosition = new PositionSettings(new Vector2(0,0),new Vector2(1,0.5f), new Vector2(1,0.5f), new Vector2(1,0.5f) );
        private readonly PositionSettings _hidePosition = new PositionSettings(new Vector2(0,0),new Vector2(1,0.5f), new Vector2(1,0.5f), new Vector2(0,0.5f) );
        
        public void PrepareForDisplay()
        {
            animationTarget.SetPivotAndAnchors(new Vector2(1f, 0.5f));
            animationTarget.SetSizeToFitParent();
        }
        public override Sequence PlayShowAnimation(Action callback = null)
        {
            EnableBlocker(true);
            animationTarget.SetPivotAndAnchors(_showPosition.Pivot, _showPosition.AnchorMin, _showPosition.AnchorMax);
            animationTarget.anchoredPosition = _showPosition.AnchoredPosition;
            callback += () => EnableBlocker(false);
            return PlayAnimation(_idlePosition, callback);
        }

        public override Sequence PlayHideAnimation(Action callback = null)
        {
            EnableBlocker(true);
            callback += () => EnableBlocker(false);
            return PlayAnimation(_hidePosition, callback);
        }

        public override void PlayShowAnimationInstant()
        {
            PlayAnimationInstant(_idlePosition);
        }

        public override void PlayHideAnimationInstant()
        {
            PlayAnimationInstant(_hidePosition);
        }

        private void EnableBlocker(bool isEnabled)
        {
            if (_inputBlocker) _inputBlocker.SetActive(isEnabled);
        }
    }
}
