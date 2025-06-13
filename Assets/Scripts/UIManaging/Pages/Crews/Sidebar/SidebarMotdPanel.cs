using Abstract;
using AdvancedInputFieldPlugin;
using Bridge;
using Bridge.Models.ClientServer.Crews;
using Common;
using Extensions;
using Modules.ContentModeration;
using Modules.Crew;
using UIManaging.Localization;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Crews.Sidebar
{
    internal sealed class SidebarMotdPanel : BaseContextDataView<SidebarMotdPanelModel>
    {
        [SerializeField] private Button _clearButton;
        [SerializeField] private Button _disabledInputFieldButton;
        [SerializeField] private AdvancedInputField _advancedInputField;
        [SerializeField] private Button _saveButton;

        [Inject] private CrewService _crewService;
        [Inject] private IBridge _bridge;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private TextContentValidator _textContentValidator;
        [Inject] private CrewPageLocalization _localization;
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            _crewService.CrewModelUpdated -= OnCrewModelUpdated;
        }

        private void OnEnable()
        {
            _saveButton.onClick.AddListener(OnSaveButtonClicked);
            _clearButton.onClick.AddListener(OnClearButtonClicked);
            _disabledInputFieldButton.onClick.AddListener(OnDisabledInputFieldClicked);
        }

        private void OnDisable()
        {
            _saveButton.onClick.RemoveAllListeners();
            _clearButton.onClick.RemoveAllListeners();
            _disabledInputFieldButton.onClick.RemoveAllListeners();
        }

        protected override void OnInitialized()
        {
            _crewService.CrewModelUpdated += OnCrewModelUpdated;
            _advancedInputField.SetText(ContextData.Message);

            Refresh();
        }

        private void Refresh()
        {
            var isEditable = ContextData.UserRoleId <= Constants.Crew.COORDINATOR_ROLE_ID;
            if (!isEditable)
            {
                SetUpInputFieldForNormalUser();
                return;
            }
            _saveButton.SetActive(true);
            _advancedInputField.interactable = true;
            _clearButton.SetActive(true);
            _disabledInputFieldButton.enabled = false;
        }

        private void OnClearButtonClicked()
        {
            _advancedInputField.Clear();
        }

        private void SetUpInputFieldForNormalUser()
        {
            _saveButton.SetActive(false);
            _clearButton.SetActive(false);
            _advancedInputField.interactable = false;
            _disabledInputFieldButton.enabled = true;
            _advancedInputField.SetText(ContextData.Message ?? " ");
        }
        
        private async void OnSaveButtonClicked()
        {
            var passed = await _textContentValidator.ValidateTextContent(_advancedInputField.GetText());
            if (!passed) return;
            
            ContextData.Message = _advancedInputField.GetText();
            await _crewService.UpdateMotD(ContextData.Message);
        }

        private void OnDisabledInputFieldClicked()
        {
            _snackBarHelper.ShowInformationSnackBar(_localization.EditAccessRestrictedSnackbarMessage);
        }

        private void OnCrewModelUpdated(CrewModel crewModel)
        {
            ContextData.Message = crewModel.MessageOfDay;
            ContextData.UserRoleId = _crewService.LocalUserMemberData.RoleId;
            Refresh();
        }
    }
}