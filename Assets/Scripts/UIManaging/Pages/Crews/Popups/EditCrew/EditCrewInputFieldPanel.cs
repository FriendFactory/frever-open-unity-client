using System;
using Abstract;
using AdvancedInputFieldPlugin;
using Common;
using Modules.ContentModeration;
using UIManaging.Animated.Behaviours;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Crews.Popups.EditCrew
{
    internal sealed class EditCrewInputFieldPanel : BaseContextDataView<string>, IEditCrewPanel
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _doneButton;
        
        [Space]
        [SerializeField] private AnimatedSlideInOutBehaviour _slideInOutBehaviour;
        [SerializeField] private AdvancedInputField _inputField;

        [Inject] private PopupManager _popupManager;
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] private SnackBarHelper _snackbarHelper;
        [Inject] private TextContentValidator _textContentValidator;

        private string _newValue;
        
        public Action RequestBackAction { get; set; }
        public Action RequestCloseAction { get; set; }
        public Action<string> RequestSaveAction;

        private void OnEnable()
        {
            _backButton.onClick.AddListener(OnCloseButtonClicked);
            _doneButton.onClick.AddListener(OnDoneButtonClicked);
            _inputField.OnEndEdit.AddListener(OnEndEdit);
        }

        private void OnDisable()
        {
            _backButton.onClick.RemoveAllListeners();
            _doneButton.onClick.RemoveAllListeners();
            _inputField.OnEndEdit.RemoveAllListeners();
            
            RequestCloseAction = null;
            RequestCloseAction = null;
            RequestSaveAction = null;
            
            _slideInOutBehaviour.SetOutPosition();
        }

        protected override void OnInitialized()
        {
            _newValue = ContextData;
            _inputField.SetText(ContextData);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            _slideInOutBehaviour.PlayInAnimation(null);
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
            if (_newValue.Equals(ContextData) || string.IsNullOrEmpty(_newValue))
            {
                RequestBackAction?.Invoke();
                return;
            }
            
            _popupManagerHelper.ShowEraseChangesPopup(OnEraseChangesClicked, OnKeepEditingClicked);
        }

        private async void OnDoneButtonClicked()
        {
            if (!await _textContentValidator.ValidateTextContent(_newValue))
            {   
                return;
            }

            RequestSaveAction?.Invoke(_newValue);
            RequestBackAction?.Invoke();
        }

        private void OnEraseChangesClicked()
        {
            RequestBackAction?.Invoke();
        }

        private void OnKeepEditingClicked()
        {
            _popupManager.ClosePopupByType(PopupType.DialogDarkV3Vertical);
        }

        private void OnEndEdit(string text, EndEditReason reason)
        {
            _newValue = text;
        }
        
    }
}