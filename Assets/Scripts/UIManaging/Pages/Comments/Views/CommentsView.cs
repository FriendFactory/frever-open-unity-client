using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using AdvancedInputFieldPlugin;
using Bridge;
using Bridge.VideoServer;
using Common;
using Modules.Amplitude;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UIManaging.Common.InputFields;
using UIManaging.Common.Ui;
using UIManaging.Localization;
using UIManaging.Pages.Comments.Views;
using UIManaging.Pages.Common.Helpers;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;
using static UIManaging.Common.Ui.MentionsPanel;
using CommentInfo = Bridge.NotificationServer.CommentInfo;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.Comments
{
    public class CommentsView : MonoBehaviour, IInputFieldEventProvider
    {
        private const int MENTIONS_LIMIT = 5;
        
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action OnCommentPublished;
        public event Action OnOpen;
        public event Action InputFieldActivated;
        public event Action InputFieldDeactivated;
        public event Action InputFieldSlidedDown;

        //---------------------------------------------------------------------
        // Fields
        //---------------------------------------------------------------------

        [SerializeField] private CommentsListView _commentListView;
        [SerializeField] private Button _outsideViewButton;
        [SerializeField] private Button _outsideInputFieldButton;
        [SerializeField] private Button _publishCommentButton;
        [SerializeField] private Button _mentionButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private RectTransform _rectToIgnoreTransform;
        [SerializeField] private RectTransform _inputPanelRect;
        [SerializeField] private HorizontalLayoutGroup _inputPanelLayoutGroup;
        [SerializeField] private RawImage _userAvatar;
        [SerializeField] private CommentsViewAnimator _commentsViewAnimator;
        [SerializeField] private MentionsPanel _mentionsPanel;
        [SerializeField] private CharacterLimitFilter _characterLimitFilter;

        [Inject] private CommentsLocalization _localization;
        
        #if ADVANCEDINPUTFIELD_TEXTMESHPRO
        [SerializeField] private IgnoredDeselectableAreaAdvancedInputField _commentInputField;
        #else
        [SerializeField] private CommentInputField _commentInputField;
        #endif

        private IBridge _bridge;
        private CharacterThumbnailsDownloader _characterThumbnailsDownloader;
        private PopupManager _popupManager;
        private SnackBarHelper _snackBarHelper;
        private CommentInputDecorator _commentInputDecorator;

        private IInputFieldAdapter _inputFieldAdapter;
        private InputFieldMaxHeightResizer _inputFieldMaxHeightResizer;
        private CommentListModel _commentListModel;
        private CancellationTokenSource _cancellationTokenSource;
        
        private long _videoId;
        private long? _replyToCommentId;

        private bool _reloadOnOpen;
        private bool _isOpen;
        private string _commentPrependedOnTopKey;
        private float _defaultCommentPanelPaddingTop;
        private float _defaultCommentPanelHeight;
        private float _lastResizeMaxHeight;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        [Inject]
        [UsedImplicitly]
        private void Construct(IBridge bridge, PopupManager popupManager, SnackBarHelper snackBarHelper, InputFieldAdapterFactory inputFieldAdapterFactory, CharacterThumbnailsDownloader characterThumbnailsDownloader)
        {
            _bridge = bridge;
            _snackBarHelper = snackBarHelper;
            _popupManager = popupManager;

            _commentsViewAnimator.InputFieldSlidingUpStart += OnInputFieldSlidingUpStart;
            _commentsViewAnimator.InputFieldSlidingUpFinish += OnInputFieldSlidingUpFinish;
            
            _outsideViewButton.onClick.AddListener(Close);
            _closeButton.onClick.AddListener(Close);
            _outsideInputFieldButton.onClick.AddListener(OnKeyboardLostFocus);
            _publishCommentButton.onClick.AddListener(OnClickPublishButton);
            _mentionButton.onClick.AddListener(ShowMentionsPanel);

            _characterThumbnailsDownloader = characterThumbnailsDownloader;
            
            _inputFieldAdapter = inputFieldAdapterFactory.CreateInstance(_commentInputField, true);
             var inputFieldScrollArea = _commentInputField.GetComponentInChildren<ScrollArea>();
            _inputFieldMaxHeightResizer = new InputFieldMaxHeightResizer(this, _inputFieldAdapter, inputFieldScrollArea);
            _mentionsPanel.Init(_inputFieldAdapter);
#if !ADVANCEDINPUTFIELD_TEXTMESHPRO
            _commentInputDecorator = new CommentInputDecorator(_inputFieldAdapter);
#endif
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            LoadUserProfileImage();

            _defaultCommentPanelPaddingTop = _inputPanelLayoutGroup.padding.top;
            _defaultCommentPanelHeight = _inputPanelRect.rect.height;
        }

        private void OnEnable()
        {
            EnableInputFieldAdapter();

#if !ADVANCEDINPUTFIELD_TEXTMESHPRO
            _commentInputDecorator.MentionRequested += ShowMentionsPanel;
            _commentInputDecorator.OnEnable();
#endif
        }

        private void OnDisable()
        {
            Cancel();
            
            DisableInputFieldAdapter();
            
#if !ADVANCEDINPUTFIELD_TEXTMESHPRO
            _commentInputDecorator.MentionRequested -= ShowMentionsPanel;
            _commentInputDecorator.OnDisable();
#endif
        }
        
        private void OnDestroy()
        {
            _inputFieldAdapter.Dispose();
            _inputFieldMaxHeightResizer.Dispose();
            _outsideViewButton.onClick.RemoveListener(Close);
            _closeButton.onClick.RemoveListener(Close);
            _outsideInputFieldButton.onClick.RemoveListener(OnKeyboardLostFocus);
            _publishCommentButton.onClick.RemoveListener(OnClickPublishButton);
            _mentionButton.onClick.RemoveListener(ShowMentionsPanel);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public long VideoId
        {
            set
            {
                if (_videoId == value) return;
                _reloadOnOpen = true;
                _videoId = value;
            }
        }

        public void Open(bool openKeyboard = false)
        {
            if (_reloadOnOpen)
            {
                ClearInputFieldText();
                SetupCommentListView();
            }
            _commentsViewAnimator.SlideUpAnimation();
            _isOpen = true;

            if (!openKeyboard) return;
            _inputFieldAdapter.ActivateInputField();
        }

        public void OpenWithCommentOnTop(CommentInfo commentInfo)
        {
            _commentListView.OnFirstPageLoaded += LoadAndAppendOnTop;
            _reloadOnOpen = true;
            Open();

            void LoadAndAppendOnTop()
            {
                _commentListView.OnFirstPageLoaded -= LoadAndAppendOnTop;
                _commentPrependedOnTopKey = commentInfo.Key.Split('.')[0];
                    
                //load root comment
                if (commentInfo.ReplyTo == null)
                {
                    //Check if comment was loaded on first page
                    _commentListModel.LoadOnTop(_commentPrependedOnTopKey, null, null);
                    return;
                }
                
                //if reply to root - load root and target reply
                if (commentInfo.ReplyTo.Key == _commentPrependedOnTopKey)
                {
                    _commentListModel.LoadOnTop(_commentPrependedOnTopKey, null, commentInfo.Key);
                    return;
                }
                
                //if reply to reply - load root, recipient reply and target reply
                var recipientKey = commentInfo.ReplyTo.Key;
                _commentListModel.LoadOnTop(_commentPrependedOnTopKey, recipientKey, commentInfo.Key);
            }
        }
        
        //---------------------------------------------------------------------
        // Callbacks and Helpers 
        //---------------------------------------------------------------------

        private void ShowMentionsPanel()
        {
            if (!_inputFieldAdapter.IsFocused)
            {
                _inputFieldAdapter.ActivateInputField();
            }
            
            var mentionsCount = Regex.Matches(_inputFieldAdapter.Text, MENTION_REGEX).Count;
            if (mentionsCount >= MENTIONS_LIMIT)
            {
                _snackBarHelper.ShowInformationSnackBar(string.Format(_localization.ReachedMentionLimitSnackbarMessage, MENTIONS_LIMIT));
                return;
            }
#if ADVANCEDINPUTFIELD_TEXTMESHPRO
            var text = _inputFieldAdapter.Text.Insert(_inputFieldAdapter.StringPosition, "@");

            if (_inputFieldAdapter.GetParsedTextLength(_characterLimitFilter.CharacterLimit).Item1 > _characterLimitFilter.CharacterLimit)
            {
                _snackBarHelper.ShowInformationSnackBar(_localization.ReachedTextLimitSnackbarMessage, 2);
                return;
            }
            
            _inputFieldAdapter.Text = text;
            _inputFieldAdapter.CaretPosition += 1;
            
            _commentInputField.ShouldBlockDeselect = false;
#else 
            _mentionsPanel.Show();
#endif
        }

        private void LoadUserProfileImage()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _characterThumbnailsDownloader.GetCharacterThumbnailByUserGroupId(_bridge.Profile.GroupId, Resolution._128x128, OnThumbnailDownloaded, cancellationToken: _cancellationTokenSource.Token);

            void OnThumbnailDownloaded(Texture2D texture)
            {
                _userAvatar.texture = texture;
            }
        }

        private void EnableInputFieldAdapter()
        {
            _inputFieldAdapter.OnValueChanged += OnInputValueChanged;
            _inputFieldAdapter.OnKeyboardStatusChanged += OnKeyboardStatusChanged;
            _inputFieldAdapter.OnKeyboardHeightChanged += OnKeyboardHeightChanged;
            
            _commentInputField.onSelect.AddListener(OnInputSelect);
            _commentInputField.OnSizeChanged.AddListener(OnInputFieldSizeChanged);

            _commentInputField.AddIgnoreDeselectOnRect(_rectToIgnoreTransform);
        }

        private void DisableInputFieldAdapter()
        {
            _inputFieldAdapter.OnValueChanged -= OnInputValueChanged;
            _inputFieldAdapter.OnKeyboardStatusChanged -= OnKeyboardStatusChanged;
            _inputFieldAdapter.OnKeyboardHeightChanged -= OnKeyboardHeightChanged;
            _commentInputField.onSelect.RemoveListener(OnInputSelect);
            _commentInputField.OnSizeChanged.RemoveListener(OnInputFieldSizeChanged);
        }

        private void OnInputFieldSlidingUpStart()
        {
#if !ADVANCEDINPUTFIELD_TEXTMESHPRO
            _outsideInputFieldButton.interactable = false;
            _commentInputField.AddIgnoreDeselectOnRect(_outsideInputFieldButton.GetComponent<RectTransform>());
#endif
        }
        
        private void OnInputFieldSlidingUpFinish()
        {
#if !ADVANCEDINPUTFIELD_TEXTMESHPRO
            _outsideInputFieldButton.interactable = true;
            _commentInputField.RemoveIgnoreDeselectOnRect(_outsideInputFieldButton.GetComponent<RectTransform>());
#endif
        }
        
        private void OnKeyboardStatusChanged(KeyboardStatus keyboardStatus)
        {
            if (_commentsViewAnimator.InputFieldSlidingDown) return;
            switch (keyboardStatus)
            {
                case KeyboardStatus.Canceled:
                    DeactivateInputField();
                    ClearInputFieldText();
                    _commentListView.ReturnToPositionFromReply();
                    break;
                case KeyboardStatus.Done:
                    DeactivateInputField();
                    break;
            }       
        }

        private void OnKeyboardHeightChanged(int height)
        {
            if (height <= 0) return;

        }
        
        private void OnInputSelect(string arg0)
        {
            if (_replyToCommentId.HasValue)
            {
                _commentListView.ScrollToCommentForReply(_replyToCommentId.Value);
            }
            _commentsViewAnimator.InitInputFieldSequence();
            _commentsViewAnimator.SlideUpInputField();
            
            _outsideInputFieldButton.gameObject.SetActive(true);
            
            InputFieldActivated?.Invoke();
        }

        private void Close()
        {
            _commentsViewAnimator.SlideDownAnimation();
            InputFieldSlidedDown?.Invoke();
            _isOpen = false;
        }

        private async void PublishComment(string text)
        {
            var commentReq = new AddCommentRequest { Text = text, ReplyToCommentId = _replyToCommentId };
            _replyToCommentId = null;
            var addCommentRequest = await _bridge.AddComment(_videoId, commentReq);

            if (addCommentRequest.IsSuccess)
            {
                ShowPublishedSnackbar();
                OnCommentPublished?.Invoke();
                _inputFieldAdapter.DeactivateInputField();
            }
            else if (addCommentRequest.IsError)
            {
                var error = JsonConvert.DeserializeObject<ServerError>(addCommentRequest.ErrorMessage);
                ShowInformationSnackBar(error.ErrorCode);
                Close();
            }

            if(!_isOpen) return;
            
            ClearInputFieldText();
            var commentModel = addCommentRequest.ResultObject;
            _commentListModel.InsertComment(commentModel, true);
        }
        
        private string RemoveEmptyLines(string text)
        {
            text = Regex.Replace(text, @"\v", "\n");
            text = Regex.Replace(text, @" {2,}", " ");
            text = Regex.Replace(text, @"^ +", string.Empty, RegexOptions.Multiline);
            text = Regex.Replace(text, @"^\s*$\n|\r", string.Empty, RegexOptions.Multiline);
            return text.TrimEnd();
        }

        private string GetParsedText()
        {
            var text = _inputFieldAdapter.GetParsedText();
            text = RemoveEmptyLines(text);
            
            return text;
        }

        private void ShowPublishedSnackbar()
        {
            _snackBarHelper.ShowSuccessSnackBar(_localization.CommentAddedSnackbarMessage, 2);
        }
        
        private void SetupCommentListView()
        {
            _commentListModel = new CommentListModel(_bridge, _videoId, OnReplyButton, OpenContextMenu, LikeComment, OnMovingToProfileStarted, OnMovingToProfileFinished);
            _commentListView.Initialize(_commentListModel);
            _commentPrependedOnTopKey = null;
            _reloadOnOpen = false;
            OnOpen?.Invoke();
        }

        private void LikeComment(CommentItemModel commentItemModel)
        {
            if (commentItemModel.CommentInfo.IsLikedByCurrentUser)
            {
                _bridge.LikeCommentAsync(commentItemModel.CommentInfo.VideoId, commentItemModel.Id);
            }
            else
            {
                _bridge.UnlikeCommentAsync(commentItemModel.CommentInfo.VideoId, commentItemModel.Id);
            }
        }

        private void OpenContextMenu(CommentItemModel commentItemModel)
        {
            var variants = new List<KeyValuePair<string, Action>>
            {
                new KeyValuePair<string, Action>(_localization.CommentsReplyOption, () => { OnReplyButton(commentItemModel); }),
                new KeyValuePair<string, Action>(_localization.CommentsCopyOption, () => { CopyCommentToClipboard(commentItemModel.CommentText); })
            };
            
            var config = new DarkActionPopupConfiguration
            {
                PopupType = PopupType.DarkActionPopup,
                Variants = variants
            };
            
            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(config.PopupType);
        }

        private void CopyCommentToClipboard(string commentText)
        {
            ShowInformationSnackBar(_localization.CommentCopiedSnackbarMessage);
            GUIUtility.systemCopyBuffer = commentText;
        }

        private void OnReplyButton(CommentItemModel commentItemModel)
        {
            if (_replyToCommentId != commentItemModel.Id)
            {
                ClearInputFieldText();
            }
            
            #if UNITY_EDITOR
            _commentsViewAnimator.InitInputFieldSequence();
            #endif
            
            _replyToCommentId = commentItemModel.Id;
            _inputFieldAdapter.Select();
            _inputFieldAdapter.PlaceholderText = string.Format(_localization.ReplyInputPlaceholderFormat, commentItemModel.NickName); 
        }

        private void ClearInputFieldText()
        {
            _replyToCommentId = null;
            _inputFieldAdapter.PlaceholderText = _localization.DefaultInputPlaceholder;
            _inputFieldAdapter.Text = "";
            _inputFieldAdapter.CaretPosition = 0;
            _commentsViewAnimator.HidePublishCommentButton();
        }

        private void OnClickPublishButton()
        {
            var text = GetParsedText();
            if (string.IsNullOrEmpty(text)) return;
            _commentsViewAnimator.HidePublishCommentButton();
            PublishComment(text);
            DeactivateInputField();
        }

        private void OnEmojiSelected(string emojiName)
        {
            _inputFieldAdapter.InsertEmoji(emojiName);
        }

        private void OnInputValueChanged(string text)
        {
            var isNullEmptyOrWhitespace = string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text);
            if (isNullEmptyOrWhitespace)
            {
                _commentsViewAnimator.HidePublishCommentButton();
            }
            else
            {
                _commentsViewAnimator.ShowPublishCommentButton();
            }
        }

        private void OnKeyboardLostFocus()
        {
            _commentListView.ReturnToPositionFromReply();
            DeactivateInputField();
        }
        
        private void DeactivateInputField()
        {
            _inputFieldAdapter.DeactivateInputField();
            EventSystem.current.SetSelectedGameObject(null);
            _commentsViewAnimator.SlideDownInputField();
            _outsideInputFieldButton.gameObject.SetActive(false);
            if (string.IsNullOrEmpty(_inputFieldAdapter.Text))
            {
                ClearInputFieldText();
            }
            
            InputFieldDeactivated?.Invoke();
        }

        private void OnMovingToProfileStarted()
        {
            _inputFieldAdapter.Interactable = false;
        }

        private void OnMovingToProfileFinished()
        {
            _inputFieldAdapter.Interactable = true;
        }
        
        private void ShowInformationSnackBar(string message)
        {
            _snackBarHelper.ShowInformationSnackBar(message, 2);
        }

        private void Cancel()
        {
            _cancellationTokenSource?.Cancel();
        }

        private void OnInputFieldSizeChanged(Vector2 value)
        {
            var size = _inputPanelRect.rect.size;
            var sizeDifferenceY = value.y - size.y;
            var sizeDelta = _inputPanelRect.sizeDelta;
            var sizeDeltaY = sizeDelta.y + sizeDifferenceY + 2f * _defaultCommentPanelPaddingTop;
            sizeDeltaY = Mathf.Max(sizeDeltaY, _defaultCommentPanelHeight);

            if (sizeDifferenceY == 0f) return;

            sizeDelta.y = sizeDeltaY;
            _inputPanelRect.sizeDelta = sizeDelta;
                
            LayoutRebuilder.ForceRebuildLayoutImmediate(_inputPanelRect);
        }
    }
}