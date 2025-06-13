using System.Collections;
using UI.UIAnimators;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Common.TabsManager
{
    public class AnimatedTabView : TabView
    {
        [SerializeField] private BaseUiAnimationPlayer _uiAnimatorPlayer;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        protected override void OnEnable()
        {
            base.OnEnable();
            RefreshAnimationStateInstant(Toggle.isOn);
            StartCoroutine(RebuildLayout());
        }
       
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public override void RefreshVisuals()
        {
            base.RefreshVisuals();
            RefreshAnimationStateInstant(Toggle.isOn);
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnBeforeOnToggleValueChangedEvent(bool value)
        {
            base.OnBeforeOnToggleValueChangedEvent(value);
            RefreshAnimationState(value);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void RefreshAnimationStateInstant(bool isOn)
        {
            if (isOn)
            {
                _uiAnimatorPlayer?.PlayShowAnimationInstant();
            }
            else
            {
                _uiAnimatorPlayer?.PlayHideAnimationInstant();
            }
        }

        private void RefreshAnimationState(bool isOn)
        {
            if (isOn)
            {
                _uiAnimatorPlayer?.PlayShowAnimation();
            }
            else
            {
                _uiAnimatorPlayer?.PlayHideAnimation();
            }
        }
        
        private IEnumerator RebuildLayout()
        {
            yield return new WaitForEndOfFrame();
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform) transform);
        }
    }
}