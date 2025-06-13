using System;
using Abstract;
using UIManaging.Animated.Behaviours;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Crews.Popups.EditCrew
{
    internal sealed class EditCrewPrivacyPanel : BaseContextDataView<bool>, IEditCrewPanel
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _doneButton;
        [SerializeField] private CrewPrivacyButton _publicButton;
        [SerializeField] private CrewPrivacyButton _privateButton;
        
        [Space]
        [SerializeField] private AnimatedSlideInOutBehaviour _slideInOutBehaviour;

        [Inject] private PopupManager _popupManager;
        [Inject] private PopupManagerHelper _popupManagerHelper;

        private bool _originalPrivacy;
        
        public Action RequestBackAction { get; set; }
        public Action RequestCloseAction { get; set; }
        public Action<bool> RequestSaveAction { get; set; }

        private void OnEnable()
        {
            _backButton.onClick.AddListener(OnBackButtonClicked);
            _doneButton.onClick.AddListener(OnDoneButtonClicked);

            _publicButton.OnClicked += OnPublicButtonClicked;
            _privateButton.OnClicked += OnPrivateButtonClicked;
        }

        private void OnDisable()
        {
            _backButton.onClick.RemoveAllListeners();
            _doneButton.onClick.RemoveAllListeners();
            
            _publicButton.OnClicked -= OnPublicButtonClicked;
            _privateButton.OnClicked -= OnPrivateButtonClicked;
            _slideInOutBehaviour.SetOutPosition();
        }

        protected override void OnInitialized()
        {
            _originalPrivacy = ContextData;
        }

        public void Show()
        {
            gameObject.SetActive(true);
            _slideInOutBehaviour.PlayInAnimation(null);
            
            _publicButton.SetInteractable(!ContextData);
            _privateButton.SetInteractable(ContextData);
        }

        public void Hide()
        {
            _slideInOutBehaviour.PlayOutAnimation(OnAnimationCompleted);

            void OnAnimationCompleted()
            {
                gameObject.SetActive(false);
            }
        }

        public void OnCloseButtonClicked()
        {
            if (_originalPrivacy == !_publicButton.Interactable)
            {
                RequestCloseAction?.Invoke();
                return;
            }

            _popupManagerHelper.ShowEraseChangesPopup(OnEraseChangesAndCloseClicked, OnKeepEditingClicked);
        }

        private void OnPublicButtonClicked()
        {
            _publicButton.SetInteractable(false);
            _privateButton.SetInteractable(true);
        }

        private void OnPrivateButtonClicked()
        {
            _publicButton.SetInteractable(true);
            _privateButton.SetInteractable(false);
        }

        private void OnBackButtonClicked()
        {
            if (_originalPrivacy == !_publicButton.Interactable)
            {
                RequestBackAction?.Invoke();
            }
            
            _popupManagerHelper.ShowEraseChangesPopup(OnEraseChangesAndBackClicked, OnEraseChangesAndBackClicked);
        }

        private void OnDoneButtonClicked()
        {
            RequestSaveAction?.Invoke(!_publicButton.Interactable);
            RequestBackAction?.Invoke();
        }
        
        private void OnEraseChangesAndBackClicked()
        {
            RequestBackAction?.Invoke();
        }

        private void OnEraseChangesAndCloseClicked()
        {
            RequestCloseAction?.Invoke();
        }

        private void OnKeepEditingClicked()
        {
            _popupManager.ClosePopupByType(PopupType.DialogDarkV3Vertical);
        }
    }
}