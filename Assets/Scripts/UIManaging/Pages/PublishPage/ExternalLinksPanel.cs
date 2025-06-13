using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Abstract;
using AdvancedInputFieldPlugin;
using Models;
using TMPro;
using UIManaging.Common.InputFields;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.PublishPage
{
    public class ExternalLinksPanel : BaseContextDataView<ExternalLinksModel>
    {
        [Serializable]
        private struct IconByLinkType
        {
            public ExternalLinkType LinkType;
            public GameObject Icon;
        } 
        
        private const float LINK_UPDATE_TIME = 0.5f;
        private const float FADE_INACTIVE_VALUE = 0.5f;
        private const string PROTOCOL_REGEX = "^http(s?):\\/\\/.*";
        private const string OTHER_LINK_REGEX = "^(http(s?):\\/\\/.)[-a-zA-Z0-9@:%._\\+~#=]{2,256}\\.[a-z]{2,6}\\b([-a-zA-Z0-9@:%_\\+.~#?&//=]*)";

        private static readonly Dictionary<ExternalLinkType, string> SPECIAL_REGEX = new Dictionary<ExternalLinkType, string>()
        {
            { ExternalLinkType.Discord, "^http(s?):\\/\\/(www\\.)?discord\\.(gg|com|me|io)\\/[-a-zA-Z0-9@:%._\\+~#=]{1,256}" }
        };
        
        [SerializeField] private Toggle _useExternalLinkToggle;
        [SerializeField] private CanvasGroup _inactiveFade;
        [SerializeField] private AdvancedInputField _linkInputField;
        [SerializeField] private TextMeshProUGUI _errorMessage;
        [SerializeField] private Button _saveButton;
        [SerializeField] private List<IconByLinkType> _iconsByLinkType;

        [Inject] private InputFieldAdapterFactory _inputFieldAdapterFactory;
        [Inject] private SnackBarHelper _snackBarHelper;
        
        private IInputFieldAdapter _linkInputAdapter;
        private Coroutine _updateLinkCoroutine;

        //---------------------------------------------------------------------
        // BaseContextDataView
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            _useExternalLinkToggle.onValueChanged.AddListener(OnToggleValueChanged);
            _useExternalLinkToggle.SetIsOnWithoutNotify(ContextData.IsActive);
            
            _linkInputAdapter = _inputFieldAdapterFactory.CreateInstance(_linkInputField);
            _linkInputAdapter.Text = ContextData.CurrentLink;
            _linkInputAdapter.OnValueChanged += OnLinkChanged;
            
            _saveButton.onClick.AddListener(OnSaveClick);
            
            UpdateLink();
            UpdateCanvasGroup(ContextData.IsActive);
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            
            _useExternalLinkToggle.onValueChanged.RemoveListener(OnToggleValueChanged);
            
            _linkInputAdapter.OnValueChanged -= OnLinkChanged;
            _linkInputAdapter?.Dispose();
            
            _saveButton.onClick.RemoveListener(OnSaveClick);

            if (_updateLinkCoroutine != null)
            {
                StopCoroutine(_updateLinkCoroutine);
                _updateLinkCoroutine = null;
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void OnSaveClick()
        {
            var link = _linkInputAdapter.Text;
            
            FixLinkProtocol(ref link);

            var linkType = GetLinkType(link);

            if (linkType == ExternalLinkType.Invalid)
            {
                _errorMessage.text = string.IsNullOrEmpty(_linkInputAdapter.Text)
                    ? "URL can not be empty"
                    : "Invalid URL";
                _errorMessage.gameObject.SetActive(true);
                return;
            }
            
            ContextData.CurrentLink = link;
            ContextData.OnSave?.Invoke(linkType, link);
            UpdateLink();
            
            _snackBarHelper.ShowInformationShortSnackBar("Link added");
        }

        private void OnLinkChanged(string link)
        {
            if (_updateLinkCoroutine != null)
            {
                StopCoroutine(_updateLinkCoroutine);
            }
            
            _updateLinkCoroutine = StartCoroutine(UpdateLinkCoroutine());
        }

        private void OnToggleValueChanged(bool active)
        {
            UpdateCanvasGroup(active);
            UpdateLink();

            if (active)
            {
                return;
            }
            
            var link = _linkInputAdapter.Text;
            
            FixLinkProtocol(ref link);
            
            ContextData.OnSave?.Invoke(ExternalLinkType.Invalid, link);
        }

        private void UpdateCanvasGroup(bool active)
        {
            _inactiveFade.alpha = active ? 1 : FADE_INACTIVE_VALUE;
            _inactiveFade.interactable = active;
            _linkInputAdapter.Interactable = active;
        }

        private IEnumerator UpdateLinkCoroutine()
        {
            yield return new WaitForSeconds(LINK_UPDATE_TIME);

            UpdateLink();
            _updateLinkCoroutine = null;
        }

        private void UpdateLink()
        {
            var link = _linkInputAdapter.Text;
            var linkType = GetLinkType(link);

            var fixedLink = link;
            FixLinkProtocol(ref fixedLink);

            foreach (var iconByLinkType in _iconsByLinkType)
            {
                iconByLinkType.Icon.SetActive(iconByLinkType.LinkType == linkType);
            }
            
            _errorMessage.gameObject.SetActive(false);
        }

        private static ExternalLinkType GetLinkType(string link)
        {
            FixLinkProtocol(ref link);
            
            foreach (var linkType in SPECIAL_REGEX.Keys)
            {
                if (Regex.IsMatch(link, SPECIAL_REGEX[linkType]))
                {
                    return linkType;
                }
            }

            if (Regex.IsMatch(link, OTHER_LINK_REGEX))
            {
                return ExternalLinkType.OtherLink;
            }

            return ExternalLinkType.Invalid;
        }

        private static void FixLinkProtocol(ref string link)
        {
            if (!Regex.IsMatch(link, PROTOCOL_REGEX))
            {
                link = $"https://{link}";
            }
        }
    }

    public class ExternalLinksModel
    {
        public bool IsActive;
        public string CurrentLink;
        public Action<ExternalLinkType, string> OnSave;
    }
}