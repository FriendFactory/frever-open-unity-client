using Bridge.Models.ClientServer.Crews;
using UIManaging.Animated.Behaviours;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UIManaging.Pages.Crews.Sidebar
{
    public class CrewSidebarPopup : BasePopup<CrewSidebarPopupConfiguration>
    {
        [SerializeField] private Button _outsideButton;
        [SerializeField] private CrewManagementView _managementView;
        
        [Space] 
        [SerializeField] private AnimatedFullscreenOverlayBehaviour _animatedFullscreenOverlayBehaviour;

        private void OnEnable()
        {
            _outsideButton.onClick.AddListener(OnOutsideButtonClick);
        }

        private void OnDisable()
        {
            _outsideButton.onClick.RemoveAllListeners();
        }

        protected override void OnConfigure(CrewSidebarPopupConfiguration configuration)
        {
            _managementView.Initialize(new CrewManagementViewModel(Configs.Model, Configs.ShowRequests));
        }

        public override void Show()
        {
            _animatedFullscreenOverlayBehaviour.PlayInAnimation(null);
            base.Show();
        }

        private void OnOutsideButtonClick()
        {
            Configs.OnSlideOutStarted?.Invoke();
            _animatedFullscreenOverlayBehaviour.PlayOutAnimation(()=>Hide(null));
        }
    }
}