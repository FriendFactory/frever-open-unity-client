using System.Collections.Generic;
using UIManaging.Animated.Behaviours;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.OnBoardingPage.UI
{
    public sealed class DataPrivacyOverlayConfiguration : PopupConfiguration
    {
        public DataPrivacyOverlayConfiguration() : base(PopupType.DataPrivacy, null)
        {
        }
    }

    internal class DataPrivacyOverlay : BasePopup<DataPrivacyOverlayConfiguration>
    {
        [SerializeField] private List<Button> _closeButtons;
        [SerializeField] private AnimatedFullscreenOverlayBehaviour _animatedFullscreenOverlayBehaviour;

        private void OnEnable()
        {
            if (Configs is null) return;
            
            _animatedFullscreenOverlayBehaviour.PlayInAnimation(null);
            _closeButtons.ForEach(b => b.onClick.AddListener(OnCloseButtonClick));
        }

        private void OnDisable()
        {
            _closeButtons.ForEach(b => b.onClick.RemoveAllListeners());
        }

        protected override void OnConfigure(DataPrivacyOverlayConfiguration configuration)
        {
        }

        private void OnCloseButtonClick()
        {
            _animatedFullscreenOverlayBehaviour.PlayOutAnimation(OnOutAnimationCompleted);

            void OnOutAnimationCompleted()
            {
                Hide(false);
            }
        }
    }
}