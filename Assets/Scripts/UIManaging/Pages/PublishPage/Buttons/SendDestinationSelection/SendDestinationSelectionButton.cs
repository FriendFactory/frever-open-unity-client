using System;
using UIManaging.Localization;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.PublishPage.Buttons.SendDestinationSelection
{
    [RequireComponent(typeof(Button))]
    internal sealed class SendDestinationSelectionButton: MonoBehaviour
    {
        [SerializeField] private ShareDestinationPreview _shareDestinationPreview;
        [SerializeField] private CanvasGroup _canvasGroup;
        
        [Inject] private PopupManager _popupManager;
        [Inject] private LocalUserDataHolder _localUserDataHolder;
        [Inject] private ShareToPopupLocalization _localization;
        
        private ShareDestinationData _preselectedDestinationData;
        private Button _button;
        
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action<ShareDestinationData> DestinationsSelected;
        public event Action DestinationSelectionCancelled;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClick);
            _shareDestinationPreview.Init(_localUserDataHolder.GroupId);
        }

        private void OnEnable()
        {
            if (!_canvasGroup) return;
            
            _canvasGroup.alpha = _localUserDataHolder.UserProfile.ChatAvailableAfterTime > DateTime.UtcNow ? 0.5f : 1f;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void SetPreselectedData(ShareDestinationData preselectedDestinationData)
        {
            _preselectedDestinationData = preselectedDestinationData;
            if (_preselectedDestinationData != null)
            {
                _shareDestinationPreview.Show(_preselectedDestinationData);
            }
        }
        
        public void OpenSelectionPopup()
        {
            var config = new ShareToPopupConfiguration(OnShareDestinationSelected, true, 
                                                       preselectedDestinationData: _preselectedDestinationData, 
                                                       onCancelled: ()=>DestinationSelectionCancelled?.Invoke(), 
                                                       completeButtonText:_localization.DoneButton);
            
            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(config.PopupType);
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void OnClick()
        {
            if (_localUserDataHolder.UserProfile.ChatAvailableAfterTime > DateTime.UtcNow)
            {
                var config = new DirectMessagesLockedPopupConfiguration();
                    
                _popupManager.SetupPopup(config);
                _popupManager.ShowPopup(config.PopupType);
            }
            else
            {
                OpenSelectionPopup();
            }
        }

        private void OnShareDestinationSelected(ShareDestinationData shareDestinationData)
        {
            _popupManager.ClosePopupByType(PopupType.ShareTo);
            DestinationsSelected?.Invoke(shareDestinationData);
        }
    }
}