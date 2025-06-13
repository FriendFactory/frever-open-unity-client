using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UIManaging.Common.Hashtags;
using UIManaging.Common.InputFields;
using UIManaging.Common.Ui;
using UIManaging.Localization;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;
using static UIManaging.Common.Ui.MentionsPanel;

namespace UIManaging.Pages.SharingPage.Ui
{
    internal sealed class DescriptionPanel : MonoBehaviour
    {
#if ADVANCEDINPUTFIELD_TEXTMESHPRO 
        [SerializeField] private IgnoredDeselectableAreaAdvancedInputField _descriptionInput;
#else 
        [SerializeField] private DescriptionInputField _descriptionInput;
#endif
        [Header("Character Limit")]
        [SerializeField] private int _characterLimit = 250;
        [SerializeField] private Text _characterLimitText;
        [SerializeField] private Color _characterLimitNormal;
        [SerializeField] private Color _characterLimitExceeded;
        [Header("Mentions")]
        [SerializeField] private Button _mentionButton;
        [SerializeField] private MentionsPanel _mentionsPanel;
        [Header("Hashtags")]
        [SerializeField] private Button _hashtagButton;
        [SerializeField] private HashtagsPanel _hashtagsPanel;

        [Inject] private PublishPageLocalization _localization;
        
        private SnackBarHelper _snackBarHelper;
        private InputFieldAdapterFactory _inputFieldAdapterFactory;
        private DescriptionInputDecorator _inputDecorator;
        private bool _isKeyboardVisible;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public bool IsCharLimitExceeded => _inputDecorator.IsCharLimitExceeded;

        public IInputFieldAdapter InputFieldAdapter { get; private set; }
        
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action<bool> CharacterLimitExceededStatusChanged;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        [UsedImplicitly]
        private void Construct(SnackBarHelper snackBarHelper, InputFieldAdapterFactory inputFieldAdapterFactory)
        {
            _snackBarHelper = snackBarHelper;
            _inputFieldAdapterFactory = inputFieldAdapterFactory;
        }
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            InputFieldAdapter = _inputFieldAdapterFactory.CreateInstance(_descriptionInput, true);
            _inputDecorator = new DescriptionInputDecorator(InputFieldAdapter, _hashtagsPanel,
                                                            _characterLimit, _characterLimitText,
                                                            _characterLimitNormal, _characterLimitExceeded);
            _mentionsPanel.Init(InputFieldAdapter);
            _descriptionInput.AddIgnoreDeselectOnRect(_hashtagButton.GetComponent<RectTransform>(), _mentionButton.GetComponent<RectTransform>());
        }

        private void OnEnable()
        {
            _mentionButton.onClick.AddListener(ShowMentionsPanel);
            _hashtagButton.onClick.AddListener(OnHashtagButtonClicked);
            InputFieldAdapter.OnKeyboardHeightChanged += OnKeyboardHeightChanged;
            _inputDecorator.OnEnable();
            _inputDecorator.MentionRequested += ShowMentionsPanel;
            _inputDecorator.CharacterLimitExceededStatusChanged += OnCharacterLimitExceededChanged;
        }
        
        private void OnDisable()
        {
            _mentionButton.onClick.RemoveListener(ShowMentionsPanel);
            _hashtagButton.onClick.RemoveListener(OnHashtagButtonClicked);
            InputFieldAdapter.OnKeyboardHeightChanged -= OnKeyboardHeightChanged;
            _inputDecorator.OnDisable();
            _inputDecorator.MentionRequested -= ShowMentionsPanel;
            _inputDecorator.CharacterLimitExceededStatusChanged += OnCharacterLimitExceededChanged;
            _hashtagsPanel.Hide();
        }

        private void OnDestroy()
        {
            InputFieldAdapter.Dispose();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        public string GetParsedText()
        {
            var text = InputFieldAdapter.GetParsedText();
#if UNITY_IOS
            text = Regex.Replace(text, MENTION_REGEX, MENTION_ID_PATTERN);
            text = AdvancedInputFieldUtils.GetParsedText(text);
#endif
            return text;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private async void ShowMentionsPanel()
        {
            if (!InputFieldAdapter.IsFocused)
            {
                InputFieldAdapter.ActivateInputField();
            }
            
            var mentionsCount = Regex.Matches(InputFieldAdapter.Text, MENTION_REGEX).Count;
            if (mentionsCount >= 10)
            {
                _snackBarHelper.ShowInformationSnackBar(_localization.MentionsLimitReachedSnackbarMessage, 2);
                return;
            }

            if (_hashtagsPanel.IsActive)
            {
                _hashtagsPanel.ApplyLatestFilteringResult();
                _hashtagsPanel.Hide();

                // tag will be inserted with delay inside coroutine, so, needs to wait before opening Mentions panel
                await Task.Delay(25);
            }
            
#if ADVANCEDINPUTFIELD_TEXTMESHPRO 
            var text = InputFieldAdapter.Text.Insert(InputFieldAdapter.StringPosition, "@");
            InputFieldAdapter.Text = text;
            InputFieldAdapter.CaretPosition += 1;

            _descriptionInput.ShouldBlockDeselect = false;
#else 
            _mentionsPanel.Show();
#endif
        }

        private void OnHashtagButtonClicked()
        {
            if (!InputFieldAdapter.IsFocused)
            {
                InputFieldAdapter.ActivateInputField();
            }
#if ADVANCEDINPUTFIELD_TEXTMESHPRO 
            var text = InputFieldAdapter.Text.Insert(InputFieldAdapter.StringPosition, "#");
            InputFieldAdapter.Text = text;
            InputFieldAdapter.CaretPosition += 1;
#else               
            var text = InputFieldAdapter.Text.Insert(InputFieldAdapter.StringPosition, "#");
            InputFieldAdapter.SetTextWithoutNotify(text);
            InputFieldAdapter.StringPosition += 1;
            InputFieldAdapter.CaretPosition += 1;
            InputFieldAdapter.SendOnValueChanged();
#endif
        }

        private void OnKeyboardHeightChanged(int height)
        {
            _isKeyboardVisible = height > 0;

            if (!_isKeyboardVisible && EventSystem.current != null && !EventSystem.current.alreadySelecting)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        private void OnCharacterLimitExceededChanged(bool isExceeded)
        {
            CharacterLimitExceededStatusChanged?.Invoke(isExceeded);
        }
    }
}