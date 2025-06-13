using AdvancedInputFieldPlugin;
using Bridge;
using TMPro;
using UIManaging.Common.Args.Buttons;
using UIManaging.Common.PageHeader;
using UIManaging.Localization;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.SupportCreatorPage
{
    public class SupportCreatorPopup : BasePopup<InformationPopupConfiguration>
    {
        [SerializeField] private PageHeaderView _pageHeaderView;
        [SerializeField] private AdvancedInputField _codeInputField;
        [SerializeField] private Button _submitCodeButton;
        [SerializeField] private TMP_Text _errorText;
        [SerializeField] private int _codeMinLength = 4;

        [Inject] private IBridge _bridge;
        [Inject] private PopupManager _popupManager;
        [Inject] private StorePageLocalization _localization;
        
        private void Awake()
        {
            _submitCodeButton.onClick.AddListener(SubmitCode);
            _codeInputField.OnValueChanged.AddListener(OnInputValueChanged);
        }

        private void OnInputValueChanged(string text)
        {
            _submitCodeButton.interactable = text.Length >= _codeMinLength;
        }

        protected override void OnConfigure(InformationPopupConfiguration configuration)
        {
            
        }

        public override void Show()
        {
            base.Show();
            
            _codeInputField.Text = string.Empty;
            _errorText.text = string.Empty;
            _submitCodeButton.interactable = false;
            
            _pageHeaderView.Init(new PageHeaderArgs(_localization.SupportCreatorHeader, new ButtonArgs(string.Empty, Hide)));
        }

        private async void SubmitCode()
        {
            _submitCodeButton.interactable = false;
            
            _errorText.text = string.Empty;
            
            var result = await _bridge.SubmitSupportCreatorCode(_codeInputField.Text);

            _submitCodeButton.interactable = true;

            if (!result.IsSuccess)
            {
                _errorText.text = _localization.SupportCreatorErrorMessage;
                return;
            }
            
            Hide(true);
            
            DisplayCreatorSupportSuccessPopup(result.Model.GroupId, result.Model.GroupNickName);
        }

        private void DisplayCreatorSupportSuccessPopup(long groupId, string nickname)
        {
            var config = new SupportCreatorSuccessPopupConfiguration
            {
                PopupType = PopupType.SupportCreatorSuccess,
                Title = _localization.SupportCreatorSuccessMessage,
                Description = string.Format(_localization.SupportCreatorDefaultSupportMessage, nickname),
                SupportedCreatorGroupId = groupId
            };
            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(config.PopupType);
        }
    }
}