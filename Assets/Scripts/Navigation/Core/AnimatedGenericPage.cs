using System;
using Modules.VideoStreaming.UIAnimators;
using UnityEngine;

namespace Navigation.Core
{
    public abstract class AnimatedGenericPage<T> : GenericPage<T> where T : PageArgs
    {
        [SerializeField] private PageUiAnimator _pageUiAnimator;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        protected bool IsOtherPageOpenedOnTop { get; set; }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnDisplayStart(T args)
        {
            _pageUiAnimator.PrepareForDisplay();
            
            if (!IsOtherPageOpenedOnTop)
            {
                _pageUiAnimator.PlayShowAnimation(() => base.OnDisplayStart(args));
            }
            else
            {
                base.OnDisplayStart(args);
                IsOtherPageOpenedOnTop = false;
            }
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            if (IsOtherPageOpenedOnTop)
            {
                base.OnHidingBegin(onComplete);
            }
            else
            {
                _pageUiAnimator.PlayHideAnimation(()=>OnHideAnimationFinished(onComplete));
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void OnHideAnimationFinished(Action onComplete)
        {
            // Check if page is not destroyed yet (scene unloaded)
            if (this != null)
            {
                base.OnHidingBegin(onComplete);
            }
            else
            {
                onComplete?.Invoke();
            }
        }
    }
}