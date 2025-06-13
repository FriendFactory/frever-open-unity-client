using DG.Tweening;
using System;
using Extensions;
using Modules.VideoStreaming.UIAnimators;
using UnityEngine;

namespace UI.UIAnimators
{
    public sealed class MoveUiAnimator : RectTransformBasedUiAnimator
    {
        [SerializeField] private PositionSettings _appearPosition;
        [SerializeField] private PositionSettings _idlePosition;
        [SerializeField] private PositionSettings _disappearPosition;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public override Sequence PlayShowAnimation(Action callback = null)
        {
            animationTarget.SetPivotAndAnchors(_appearPosition.Pivot, _appearPosition.AnchorMin, _appearPosition.AnchorMax);
            animationTarget.anchoredPosition = _appearPosition.AnchoredPosition;
            return PlayAnimation(_idlePosition, callback);
        }

        public override Sequence PlayHideAnimation(Action callback = null)
        {
            return PlayAnimation(_disappearPosition, callback);
        }

        public override void PlayHideAnimationInstant()
        {
            PlayAnimationInstant(_disappearPosition);
        }

        public override void PlayShowAnimationInstant()
        {
            PlayAnimationInstant(_idlePosition);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        [ContextMenu("SetCurrentDataForAppear")]
        private void SetCurrentDataForAppear()
        {
            _appearPosition = new PositionSettings(animationTarget.anchoredPosition, animationTarget.anchorMin, animationTarget.anchorMax, animationTarget.pivot);
        }

        [ContextMenu("SetCurrentDataForIdle")]
        private void SetCurrentDataForIdle()
        {
            _idlePosition = new PositionSettings(animationTarget.anchoredPosition, animationTarget.anchorMin, animationTarget.anchorMax, animationTarget.pivot);
        }

        [ContextMenu("SetCurrentDataForDisappear")]
        private void SetCurrentDataForDisappear()
        {
            _disappearPosition = new PositionSettings(animationTarget.anchoredPosition, animationTarget.anchorMin, animationTarget.anchorMax, animationTarget.pivot);
        }
    }
}