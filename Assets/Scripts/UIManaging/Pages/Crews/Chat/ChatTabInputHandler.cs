using System;
using System.Text.RegularExpressions;
using AdvancedInputFieldPlugin;
using Bridge.Models.ClientServer;
using Bridge.Models.ClientServer.Chat;
using Bridge.Models.Common.Files;
using Bridge.Models.VideoServer;
using Common;
using Extensions;
using Modules.Crew;
using JetBrains.Annotations;
using Modules.Chat;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Animated;
using UIManaging.Common.InputFields;
using UIManaging.Common.Ui;
using UIManaging.Localization;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;
using static UIManaging.Animated.KeyboardAnchoredUiAnimator.State;

namespace UIManaging.Pages.Crews
{
    public class ChatTabInputHandler : MonoBehaviour, IInputFieldEventProvider
    {
        private const string PRIVATE_VIDEO_WARNING = "You are sharing a non-public video. Users in the chat may not be able to see it.";
        private const string DEFAULT_ERROR_MESSAGE = "Something went wrong. Try sending again";
        private const string NOT_MEMBER_ERROR_CODE = "UserNotMember";
        private const int MENTIONS_LIMIT = 5;

        [SerializeField] private IgnoredDeselectableAreaAdvancedInputField _inputField;
        [SerializeField] private RectTransform _inputPanel;
        [SerializeField] private HorizontalLayoutGroup _inputPanelLayoutGroup;
        [SerializeField] private KeyboardAnchoredUiAnimator _inputAnimator;
        [SerializeField] private Button _outsideInputFieldButton;
        [SerializeField] private CrewMediaButtonsHandler _mediaButtonsHandler;
        [SerializeField] private Button _sendButton;
        [Space]
        [SerializeField] private RectTransform _inputHeader;
        [SerializeField] private CrewChatThumbnailsPanel _thumbnailsPanel;
        [SerializeField] private ReplyToPanel _replyToPanel;
        [SerializeField] private Button _replyCancelButton;
        [SerializeField] private MentionsPanel _mentionsPanel;
        [SerializeField] private CharacterLimitFilter _characterLimitFilter;
        [SerializeField] private Button _mentionsButton;

        [Inject] private CrewService _crewService;
        [Inject] private IChatService _chatService;
        [Inject] SnackBarHelper _snackBarHelper;
        [Inject] private PageManager _pageManager;
        [Inject] private InputFieldAdapterFactory _inputFieldAdapterFactory;
        [Inject] private CrewPageLocalization _localization;
        
        private RectTransform _replyToPanelTransform;
        private RectTransform _thumbnailsPanelTransform;
        private long _chatId;

        private ChatMessage _replyToMessage;
        private Texture2D _attachedPhoto;
        private Video _attachedVideo;

        private float _defaultHeight;

        private IInputFieldAdapter _inputFieldAdapter;
        private InputFieldMaxHeightResizer _inputFieldMaxHeightResizer;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action InputFieldActivated;
        public event Action InputFieldDeactivated;
        public event Action InputFieldSlidedDown;

        internal event Action<float> HeightChanged;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _replyToPanelTransform = _replyToPanel.GetComponent<RectTransform>();
            _thumbnailsPanelTransform = _thumbnailsPanel.GetComponent<RectTransform>();
            _defaultHeight = _inputPanel.GetHeight();
            
            #if UNITY_IOS && !UNITY_EDITOR
            if (DeviceInformationHelper.IsUnknownDeviceGeneration())
            {
                _inputAnimator.KeyboardHeightOffset = 75;
            }
            #endif
            
           _inputFieldAdapter = _inputFieldAdapterFactory.CreateInstance(_inputField, true);
           var inputFieldScrollArea = _inputField.GetComponentInChildren<ScrollArea>();
           _inputFieldMaxHeightResizer = new InputFieldMaxHeightResizer(this, _inputFieldAdapter, inputFieldScrollArea);
           _mentionsPanel.Init(_inputFieldAdapter);

           _mentionsButton.onClick.AddListener(ShowMentionsPanel);
        }

        private void OnEnable()
        {
            EnableInputField();

            _mediaButtonsHandler.MediaSelectionStarted += OnMediaSelectionStarted;
            _mediaButtonsHandler.PhotoSelected += AttachPhoto;
            _mediaButtonsHandler.VideoSelected += AttachVideo;

            _thumbnailsPanel.PanelShown += OnHeaderShown;
            _replyToPanel.PanelShown += OnHeaderShown;
            _thumbnailsPanel.PhotoThumbnailRemoved += OnPhotoDetached;
            _thumbnailsPanel.VideoThumbnailRemoved += OnVideoDetached;

            NativeKeyboardManager.AddKeyboardHeightChangedListener(KeyboardHeightChanged);
            
            _crewService.SidebarActivated += OnSidebarActivated;
            _crewService.SidebarDisabled += OnSidebarDisabled;
        }

        private void OnDisable()
        {
            DisableInputField();

            _mediaButtonsHandler.MediaSelectionStarted -= OnMediaSelectionStarted;
            _mediaButtonsHandler.PhotoSelected -= AttachPhoto;
            _mediaButtonsHandler.VideoSelected -= AttachVideo;
            
            _mediaButtonsHandler.HideVideoPanel();

            _replyToPanel.PanelShown -= OnHeaderShown;
            HideReplyToPanel();

            _thumbnailsPanel.PanelShown -= OnHeaderShown;
            _thumbnailsPanel.PhotoThumbnailRemoved -= OnPhotoDetached;
            _thumbnailsPanel.VideoThumbnailRemoved -= OnVideoDetached;
            _thumbnailsPanel.RemoveAllThumbnails();

            NativeKeyboardManager.RemoveKeyboardHeightChangedListener(KeyboardHeightChanged);
            
            _crewService.SidebarActivated -= OnSidebarActivated;
            _crewService.SidebarDisabled -= OnSidebarDisabled;
        }

        private void OnDestroy()
        {
            _inputFieldAdapter.Dispose();
            _inputFieldMaxHeightResizer.Dispose();
        }

        //---------------------------------------------------------------------
        // Internal
        //---------------------------------------------------------------------

        public void Initialize(long chatId, GroupShortInfo[] members)
        {
            _chatId = chatId;
            _mentionsPanel.SetUserList(members);
        }

        public void Cleanup()
        {
        }

        public void ShowReplyToPanel(ChatMessage message)
        {
            _replyToPanel.Show(message);
            _replyToMessage = message;
            _inputField.ManualSelect();
        }

        //---------------------------------------------------------------------
        // Helpers - Thumbnails
        //---------------------------------------------------------------------

        private void HideReplyToPanel()
        {
            _replyToPanel.Hide();
            _replyToMessage = null;
        }

        private void AttachPhoto(string path, Texture2D thumbnail)
        {
            _thumbnailsPanel.AddPhotoThumbnail(thumbnail);
            _attachedPhoto = thumbnail;
        }

        private void AttachVideo(Video video, Texture2D thumbnail)
        {
            if (video.Access != VideoAccess.Public)
            {
                _snackBarHelper.ShowInformationSnackBar(PRIVATE_VIDEO_WARNING);
            }

            _thumbnailsPanel.AddVideoThumbnail(thumbnail);
            _attachedVideo = video;
        }

        private void OnPhotoDetached()
        {
            _attachedPhoto = null;
        }

        private void OnVideoDetached()
        {
            _attachedVideo = null;
        }

        private void OnHeaderShown(bool isShown)
        {
            OnHeightChanged();
        }

        private void OnHeightChanged()
        {
            CoroutineSource.Instance.ExecuteWithFramesDelay(3, () =>
            {
                var inputHeight = _inputPanel.GetHeight() - _defaultHeight;
                var headerHeight = _inputHeader.GetHeight();

                HeightChanged?.Invoke(inputHeight + headerHeight);
            });
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void EnableInputField()
        {
            _inputField.onSelect.AddListener(OnInputSelect);
            _inputField.OnEndEdit.AddListener(OnEndEdit);
            _inputField.OnSizeChanged.AddListener(OnInputFieldSizeChanged);
            _inputField.AddIgnoreDeselectOnRect(_inputPanel, _replyToPanelTransform, _thumbnailsPanelTransform);

            _outsideInputFieldButton.onClick.AddListener(DeactivateInputField);
            _sendButton.onClick.AddListener(SendChatMessage);
            _replyCancelButton.onClick.AddListener(HideReplyToPanel);
        }

        private void DisableInputField()
        {
            _inputField.onSelect.RemoveListener(OnInputSelect);
            _inputField.OnEndEdit.RemoveListener(OnEndEdit);
            _inputField.OnSizeChanged.RemoveListener(OnInputFieldSizeChanged);
            _inputField.RemoveIgnoreDeselectOnRect(_inputPanel, _replyToPanelTransform, _thumbnailsPanelTransform);

            _outsideInputFieldButton.onClick.RemoveListener(DeactivateInputField);
            _sendButton.onClick.RemoveListener(SendChatMessage);
            _replyCancelButton.onClick.RemoveListener(HideReplyToPanel);
        }

        private void DeactivateInputField()
        {
            _inputAnimator.SlideDownInputField(() => InputFieldSlidedDown?.Invoke());
            _outsideInputFieldButton.SetActive(false);

            EventSystem.current.SetSelectedGameObject(null);

            _inputField.ShouldBlockDeselect = false;
            _inputField.ManualDeselect();

            _mediaButtonsHandler.ShowMediaButtons();
            
            InputFieldDeactivated?.Invoke();
        }

        private async void SendChatMessage()
        {
            var message = RemoveEmptyLines(_inputFieldAdapter.GetParsedText());
            
            if (string.IsNullOrEmpty(message) && _attachedPhoto == null && _attachedVideo == null)
            {
                return;
            }

            NativeKeyboardManager.HideKeyboard();
            DeactivateInputField();

            _sendButton.interactable = false;

            var messageModel = PrepareMessageModel(message);
            var result = await _chatService.PostMessage(_chatId, messageModel);

            if (result.IsError)
            {
                var redirectToHome = false;
                var errorMessage = JsonUtility.FromJson<ErrorMapping>(result.ErrorMessage).ErrorCode;
                if (errorMessage.Contains(NOT_MEMBER_ERROR_CODE))
                {
                    errorMessage = _localization.NotAMemberSnackbarMessage;
                    redirectToHome = true;
                }
                else if (string.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = DEFAULT_ERROR_MESSAGE;
                }
                _snackBarHelper.ShowFailSnackBar(errorMessage);
                _sendButton.interactable = true;

                if (redirectToHome)
                {
                    _pageManager.MoveNext(new HomePageArgs(), false);
                }
                
                return;
            }

            _sendButton.interactable = true;

            _thumbnailsPanel.RemoveAllThumbnails();
            HideReplyToPanel();

            ClearInputFieldText();
        }

        [UsedImplicitly]
        private class ErrorMapping
        {
            public string ErrorCode;
        }

        private string RemoveEmptyLines(string text)
        {
            text = Regex.Replace(text, @"\v", "\n");
            text = Regex.Replace(text, @" {2,}", " ");
            text = Regex.Replace(text, @"^ +", string.Empty, RegexOptions.Multiline);
            text = Regex.Replace(text, @"^\s*$\n|\r", string.Empty, RegexOptions.Multiline);
            return text.TrimEnd();
        }

        private AddMessageModel PrepareMessageModel(string message)
        {
            var messageModel = new AddMessageModel()
            {
                Text = message
            };

            if (_replyToMessage != null)
            {
                messageModel.ReplyToMessageId = _replyToMessage.Id;
            }

            if (_attachedPhoto != null)
            {
                messageModel.AttachFile(_attachedPhoto, FileExtension.Jpg);
            }

            if (_attachedVideo != null)
            {
                messageModel.VideoId = _attachedVideo.Id;
            }

            return messageModel;
        }

        private void OnEndEdit(string value, EndEditReason reason)
        {
            if (_inputAnimator.CurrentState == SlidingDown) return;

            switch (reason)
            {
                case EndEditReason.KEYBOARD_CANCEL:
                case EndEditReason.KEYBOARD_DONE:
                    DeactivateInputField();
                    break;
            }
        }

        private void OnInputFieldSizeChanged(Vector2 value)
        {
            var panelSize = _inputPanel.rect.size.y;
            var panelPadding = _inputPanelLayoutGroup.padding.vertical;
            var newPanelSize = value.y + panelPadding;

            if (Math.Abs(panelSize - newPanelSize) < 0.1f) return;

            _inputPanel.SetSizeY(newPanelSize);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_inputPanel);

            OnHeightChanged();
        }

        private void KeyboardHeightChanged(int height)
        {
            if (height == 0) return;

            if (_inputAnimator.CurrentState != Up)
            {
                SlideUpInputField();
            }
        }

        private void OnInputSelect(string arg0)
        { 
            SlideUpInputField();
            _mediaButtonsHandler.HideMediaButtons();
            
            InputFieldActivated?.Invoke();
        }
        
        private void SlideUpInputField()
        {
            _inputAnimator.SlideUpInputField();
            _outsideInputFieldButton.gameObject.SetActive(true);
        }

        private void OnMediaSelectionStarted()
        {
            NativeKeyboardManager.HideKeyboard();
            DeactivateInputField();
        }

        private void ClearInputFieldText()
        {
            _inputField.Text = string.Empty;
            _inputField.CaretPosition = 0;
        }

        private void OnSidebarActivated()
        {
            NativeKeyboardManager.RemoveKeyboardHeightChangedListener(KeyboardHeightChanged);
        }

        private void OnSidebarDisabled()
        {
            NativeKeyboardManager.AddKeyboardHeightChangedListener(KeyboardHeightChanged);
        }
        
        private void ShowMentionsPanel()
        {
            if (!_inputFieldAdapter.IsFocused)
            {
                _inputFieldAdapter.ActivateInputField();
            }
            
            var mentionsCount = Regex.Matches(_inputFieldAdapter.Text, MentionsPanel.MENTION_REGEX).Count;
            if (mentionsCount >= MENTIONS_LIMIT)
            {
                _snackBarHelper.ShowInformationSnackBar($"You can mention up to {MENTIONS_LIMIT} users", 2);
                return;
            }
            var text = _inputFieldAdapter.Text.Insert(_inputFieldAdapter.StringPosition, "@");

            if (_inputFieldAdapter.GetParsedTextLength(_characterLimitFilter.CharacterLimit).Item1 > _characterLimitFilter.CharacterLimit)
            {
                _snackBarHelper.ShowInformationSnackBar("Reached text limitation", 2);
                return;
            }
            
            _inputFieldAdapter.Text = text;
            _inputFieldAdapter.CaretPosition += 1;
            
            _inputField.ShouldBlockDeselect = false;
        }
    }
}