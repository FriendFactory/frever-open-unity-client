using System;
using System.Collections.Generic;
using UIManaging.Animated.Behaviours;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Crews.Sidebar
{
    public class LeaveCrewPopup : BasePopup<LeaveCrewPopupConfiguration>
    {
        [SerializeField] private List<Button> _closeButtons;
        
        [SerializeField] private Button _leaveButton;
        [SerializeField] private AnimatedFullscreenOverlayBehaviour _animatedFullscreenOverlayBehaviour;

        [Inject] private PopupManager _popupManager;

        private void OnEnable()
        {
            if (Configs is null) return;
            
            _animatedFullscreenOverlayBehaviour.PlayInAnimation(null);
            _closeButtons.ForEach(b => b.onClick.AddListener(OnCloseButtonClicked));
            _leaveButton.onClick.AddListener(OnLeaveButtonClicked);
        }

        private void OnDisable()
        {
            _closeButtons.ForEach(b => b.onClick.RemoveAllListeners());
            _leaveButton.onClick.RemoveAllListeners();
        }

        protected override void OnConfigure(LeaveCrewPopupConfiguration configuration)
        {
        }

        private void OnCloseButtonClicked()
        {
            _animatedFullscreenOverlayBehaviour.PlayOutAnimation(OnOutAnimationCompleted);

            void OnOutAnimationCompleted()
            {
                Hide(false);
            }
        }

        private void OnLeaveButtonClicked()
        {
            var config = new DialogDarkPopupConfiguration()
            {
                PopupType = PopupType.DialogDarkV3,
                Title = "Are you sure?",
                YesButtonText = "Leave",
                YesButtonSetTextColorRed = true,
                OnYes = () => Hide(true),
                NoButtonText = "Cancel",
                OnNo = OnCloseButtonClicked,
                Description = "Are you sure you want to leave the crew?"
            };
            
            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(config.PopupType, true);
        }
    }
}