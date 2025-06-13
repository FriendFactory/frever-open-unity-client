using System;
using Common.Abstract;
using JetBrains.Annotations;
using Modules.SignUp;
using UIManaging.Pages.Common.AccountDetails.UsernameInput;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.PopupSystem;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.EditUsername
{
    internal sealed class EditUsernameView: BaseContextPanel<EditUsernameModel>
    {
        [SerializeField] private UsernameInputPanel _usernameInputPanel;
        [SerializeField] private EditUsernameUpdateStatusInfo _updateStatusInfo;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _backButton;

        private UsernameInputModel _usernameInputModel;
        private UsernameInputPresenter _usernameInputPresenter;
        private ISignUpService _signUpService;

        public event Action MoveBackRequested;
        public event Action<string> UpdateRequested;

        // the simplest way to toggle a bunch of GOs and component states
        public UnityEvent OnUpdateEnabled;
        public UnityEvent OnUpdateDisabled;

        [Inject, UsedImplicitly]
        private void Construct(ISignUpService signUpService, LocalUserDataHolder localUserDataHolder, PopupManager popupManager)
        {
            _signUpService = signUpService;
            _usernameInputModel = new UsernameInputModel();
            _usernameInputPresenter = new UsernameInputPresenter(signUpService, localUserDataHolder);
        }
        
        protected override void OnInitialized()
        {
            _usernameInputModel.Input = ContextData.SelectedUsername;
            _usernameInputModel.IsValid = false;

            _saveButton.interactable = ContextData.SelectedUsername != ContextData.OriginalUsername;
            
            _usernameInputPresenter.Initialize(_usernameInputModel, _usernameInputPanel);
            _usernameInputPanel.Initialize(_usernameInputModel);
            
            _updateStatusInfo.Initialize(ContextData.UsernameUpdateStatus);

            _usernameInputModel.ValidationEvent += OnValidationEvent;
            _usernameInputModel.InputValidated += OnInputValidated;

            _saveButton.onClick.AddListener(OnSaveRequested);
            _backButton.onClick.AddListener(OnMoveBackRequested);
            
            if (ContextData.UsernameUpdateStatus.CanUpdate)
            {
                OnUpdateEnabled.Invoke();
            }
            else
            {
                OnUpdateDisabled.Invoke();
            }
        }

        protected override void BeforeCleanUp()
        {
            _usernameInputModel.ValidationEvent -= OnValidationEvent;
            _usernameInputModel.InputValidated -= OnInputValidated;
            
            _usernameInputPresenter.CleanUp();
            _usernameInputPanel.CleanUp();
            
            _saveButton.onClick.RemoveListener(OnSaveRequested);
            _backButton.onClick.RemoveListener(OnMoveBackRequested);
        }

        private void OnValidationEvent(ValidationEventState state)
        {
            switch (state)
            {
                case ValidationEventState.Started:
                    _saveButton.interactable = false;
                    ContextData.SelectedUsername = _signUpService.SelectedUserName;
                    break;
                case ValidationEventState.Finished:
                    RefreshSaveButtonState();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private void RefreshSaveButtonState()
        {
            _saveButton.interactable = _usernameInputModel.IsValid && ContextData.SelectedUsername != ContextData.OriginalUsername;
        }

        private void OnInputValidated(UsernameValidationResult result) => RefreshSaveButtonState();
        private void OnMoveBackRequested() => MoveBackRequested?.Invoke();
        private void OnSaveRequested() => UpdateRequested?.Invoke(_usernameInputPanel.Input);
    }
}