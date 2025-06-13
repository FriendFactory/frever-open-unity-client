using System.Threading.Tasks;
using AdvancedInputFieldPlugin;
using Bridge;
using Common;
using Newtonsoft.Json;
using TMPro;
using UIManaging.Animated.Behaviours;
using UIManaging.Localization;
using UIManaging.PopupSystem.Configurations;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.PopupSystem.Popups.Share
{
    public sealed class EditTemplatePopup : ConfigurableBasePopup<EditTemplatePopupConfiguration>
    {
        private const string TEMPLATE_INFO_PLAYER_PREFS_KEY = "TemplateInfoKey";
        
        private const int MIN_TEMPLATE_NAME_LENGTH = 4;
        private const int MAX_TEMPLATE_NAME_LENGTH = 25;
        
        [SerializeField] private GameObject _templateInfo;
        [SerializeField] private GameObject _templateInfoButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _outsideButton;
        [SerializeField] private Button _saveButton;
        [SerializeField] private TextMeshProUGUI _templateName;
        [SerializeField] private AdvancedInputField _inputField;
        [SerializeField] private SlideInOutBehaviour _slideInOut;
        [SerializeField] private EditTemplateLocalization _localization;

        [Inject] private IBridge _bridge;
        [Inject] private SnackBarHelper _snackBarHelper;
        
        private void OnEnable()
        {
            _closeButton.onClick.AddListener(Cancel);
            _outsideButton.onClick.AddListener(Cancel);
            _saveButton.onClick.AddListener(OnSave);
            _inputField.OnValueChanged.AddListener(OnNameInputChanged);
        }

        private void OnDisable()
        {
            _closeButton.onClick.RemoveListener(Cancel);
            _outsideButton.onClick.RemoveListener(Cancel);
            _saveButton.onClick.RemoveListener(OnSave);
            _inputField.OnValueChanged.RemoveListener(OnNameInputChanged);
        }

        protected override void OnConfigure(EditTemplatePopupConfiguration configuration)
        {
            var templateInfoActivated = PlayerPrefs.GetInt(TEMPLATE_INFO_PLAYER_PREFS_KEY, 0) != 0;
            
            _slideInOut.SlideIn();
            _inputField.Text = configuration.TemplateName ?? string.Empty;
            _templateName.text = configuration.TemplateName ?? _localization.TemplateNamePlaceholder;
            _saveButton.interactable = !string.IsNullOrEmpty(configuration.TemplateName);
            _templateInfo.SetActive(!templateInfoActivated);
            _templateInfoButton.SetActive(templateInfoActivated);

            if (!templateInfoActivated)
            {
                PlayerPrefs.SetInt(TEMPLATE_INFO_PLAYER_PREFS_KEY, 1);
            }
        }

        public override void Hide()
        {
            _slideInOut.SlideOut(() => base.Hide(null));
        }

        public override void Hide(object result)
        {
            _slideInOut.SlideOut(() => base.Hide(result));
        }

        private void Cancel()
        {
            Config.BackButton?.Invoke();
            Hide();
        }
        
        private void OnNameInputChanged(string input)
        {
            _templateName.text = string.IsNullOrEmpty(input) ? _localization.TemplateNamePlaceholder : input;
            _saveButton.interactable = !string.IsNullOrEmpty(input);
        }
        
        private async void OnSave()
        {
            _saveButton.interactable = false;
            
            var newName = _inputField.Text.TrimEnd();
            
            var error = string.Empty;
            if (newName.Length < MIN_TEMPLATE_NAME_LENGTH || string.IsNullOrWhiteSpace(newName))
            {
                error = string.Format(_localization.TemplateNameMinLengthMessage, MIN_TEMPLATE_NAME_LENGTH);
            }
            
            if (newName.Length > MAX_TEMPLATE_NAME_LENGTH)
            {
                error = string.Format(_localization.TemplateNameMaxLengthMessage, MAX_TEMPLATE_NAME_LENGTH);
            }

            var isNameValid = string.IsNullOrEmpty(error);
            if (!isNameValid)
            {
                _saveButton.interactable = true;
                _snackBarHelper.ShowFailSnackBar(error);
                return;
            }

            var isNameAvailable = await CheckTemplateNameAvailable(newName);
            if (!isNameAvailable)
            {
                _saveButton.interactable = true;
                return;
            }

            Config.NameChanged?.Invoke(true, newName);

            Hide();
        }

        private async Task<bool> CheckTemplateNameAvailable(string input)
        {
            var result = await _bridge.CheckTemplateName(input);

            if (result.IsSuccess) return true;
            
            var error = JsonConvert.DeserializeObject<ServerError>(result.ErrorMessage);

            if (error == null)
            {
                _snackBarHelper.ShowFailSnackBar("Failed to deserialize error message");
                return false;
            }
            
            switch (error.ErrorCode)
            {
                case Constants.ErrorMessage.TEMPLATE_MODERATION_FAILED_ERROR_CODE:
                    _snackBarHelper.ShowFailSnackBar(_localization.TemplateNameErrorMessage);
                    break;
                default:
                    _snackBarHelper.ShowFailSnackBar(error.ErrorCode);
                    break;
            }
            
            return false;
        }
    }
}