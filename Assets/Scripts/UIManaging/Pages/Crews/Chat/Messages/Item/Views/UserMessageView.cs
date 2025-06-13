using System.Linq;
using Bridge.Models.ClientServer.Chat;
using Common.Hyperlinks;
using Extensions;
using Modules.VideoStreaming.Feed;
using Navigation.Args;
using TMPro;
using UIManaging.Common;
using UIManaging.Localization;
using UIManaging.Common.InputFields;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Crews
{
    internal class UserMessageView : PortraitMessageView
    {
        [Header("User Message")]
        [SerializeField] private GameObject _messageTextHolder;

        [Header("Username")]
        [SerializeField] private RectTransform _usernameHolder;
        [SerializeField] private TMP_Text _usernameText;

        [Header("Reply")]
        [SerializeField] private RectTransform _replyView;
        [SerializeField] private RectTransform _replyTreeLine;
        [SerializeField] private LayoutElement _replyTextElement;
        [SerializeField] private float _replyTextHeightShort = 50f;
        [SerializeField] private float _replyTextHeightLong = 100f;
        [SerializeField] private RawImage _replyUserIcon;
        [SerializeField] private int _replyTextIndent = 55;
        [SerializeField] private TMP_Text _replyText;

        [Header("Reply Media")]
        [SerializeField] private GameObject _replyMediaHolder;
        [Space]
        [SerializeField] private GameObject _replyPhotoHolder;
        [SerializeField] private RawImage _replyPhotoThumbnail;
        [SerializeField] private AspectRatioFitter _replyPhotoAspectFitter;
        [Space]
        [SerializeField] private GameObject _replyVideoHolder;
        [SerializeField] private VideoThumbnail _replyVideoThumbnail;

        [Header("Likes")]
        [SerializeField] private TextMeshProUGUI _likeCounter;
        [SerializeField] private FeedLikeToggle _likeToggle;
        [SerializeField] private TMP_Text _likeText;
        [SerializeField] private Button _likeButton;

        [Inject] private ChatLocalization _localization;
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            base.OnInitialized();

            UpdateUsername();
            UpdateLikesUI();
            UpdateReplyToView();
            HideMessageWhenEmpty();
        }

        protected override void BeforeCleanup()
        {
            _replyUserIcon.texture = DefaultUserIcon;

            DestroyImmediate(_replyPhotoThumbnail.texture, true);
            _replyPhotoThumbnail.texture = null;

            _likeButton.onClick.RemoveListener(OnLikeButtonClicked);
            _likeToggle.Toggle.onValueChanged.RemoveListener(OnLikeToggleValueChanged);

            base.BeforeCleanup();
        }

        protected virtual void UpdateUsername()
        {
            if (ContextData.ChatType != ChatType.Private)
            {
                _usernameText.text = ContextData.Username;
                _usernameHolder.SetActive(true);
                _replyTreeLine.SetBottom(-60f);
                ThumbnailHolder.SetAnchoredY(-75f);
            }
            else
            {
                _usernameHolder.SetActive(false);
                _replyTreeLine.SetBottom(10f);
                ThumbnailHolder.SetAnchoredY(0f);
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void UpdateReplyToView()
        {
            var hasReplyTo = ContextData.ChatMessage.ReplyToMessage != null;
            
            _replyView.SetActive(hasReplyTo);
            
            if (!hasReplyTo) return;

            var replyMessage = ContextData.ChatMessage.ReplyToMessage;

            UpdateReplyThumbnail(replyMessage);

            var username = replyMessage.Group?.Nickname ?? replyMessage.Group?.Id.ToString() ?? "Unknown";
            
            var mentionsMapping = replyMessage.Mentions?.ToDictionary
            (
                mention => mention.Id,
                mention => AdvancedInputFieldUtils.GetParsedText(mention.Nickname)
            );

            var text = HyperlinkParser.ParseHyperlinks(replyMessage.Text, mentionsMapping, null);
            
            var replyText = $"<indent={_replyTextIndent}></indent><b>{username}</b>: {text}";

            var preferredTextWidth = _replyText.GetPreferredValues(replyText).x;
            var currentTextWidth = _replyView.rect.size.x;

            _replyTextElement.preferredHeight = preferredTextWidth > currentTextWidth
                ? _replyTextHeightLong
                : _replyTextHeightShort;

            _replyText.text = replyText;

            UpdateReplyMedia(replyMessage);
        }

        private async void UpdateReplyThumbnail(ChatMessage replyMessage)
        {
            _replyUserIcon.texture = await DownloadUserProfileThumbnail(replyMessage.Group, CancellationSource.Token);
        }

        private void UpdateReplyMedia(ChatMessage message)
        {
            _replyMediaHolder.SetActive(false);

            _replyPhotoHolder.SetActive(false);
            _replyPhotoThumbnail.SetActive(false);

            _replyVideoHolder.SetActive(false);
            _replyVideoThumbnail.SetActive(false);

            UpdateAttachedPhoto(message);
            UpdateAttachedVideo(message);
        }

        private async void UpdateAttachedPhoto(ChatMessage message)
        {
            if (message.Files == null) return;

            _replyMediaHolder.SetActive(true);
            _replyPhotoHolder.SetActive(true);

            var result = await Bridge.GetMessageFiles(message, CancellationSource.Token);
            if (result.IsRequestCanceled) return;
            if (result.IsError)
            {
                Debug.LogError($"Failed to fetch message media: {result.ErrorMessage}");
                return;
            }

            var texture = result.Images.FirstOrDefault();
            if (texture == null)
            {
                Debug.LogError($"Failed to fetch message media: no textures available.");
                return;
            }

            _replyPhotoAspectFitter.aspectRatio = texture.width / (float) texture.height;
            _replyPhotoThumbnail.texture = texture;
            _replyPhotoThumbnail.gameObject.SetActive(true);
        }

        private async void UpdateAttachedVideo(ChatMessage message)
        {
            if (message.VideoId == null) return;

            _replyMediaHolder.SetActive(true);
            _replyVideoHolder.SetActive(true);

            var result = await Bridge.GetVideoAsync(message.VideoId.Value, CancellationSource.Token);
            if (result.IsRequestCanceled) return;
            if (result.IsError)
            {
                Debug.LogError($"Failed to resolve video URL for ID {message.VideoId}: {result.ErrorMessage}");
                return;
            }

            _replyVideoThumbnail.SetActive(true);
            _replyVideoThumbnail.Initialize(new VideoThumbnailModel(result.ResultObject.ThumbnailUrl));
        }

        private void UpdateLikesUI()
        {
            _likeButton.onClick.AddListener(OnLikeButtonClicked);

            var isLikedByCurrentUser = ContextData.IsLikedByCurrentUser;
            UpdateLikeToggle(isLikedByCurrentUser);
            UpdateLikeText(isLikedByCurrentUser);
            UpdateLikeCounter();
        }

        private void OnLikeButtonClicked()
        {
            _likeToggle.Toggle.isOn = !ContextData.IsLikedByCurrentUser;
        }

        private void OnLikeToggleValueChanged(bool isLikedByCurrentUser)
        {
            UpdateLikesModel();

            _likeToggle.RefreshUI(isLikedByCurrentUser);
            _likeToggle.SetActive(ContextData.LikeCount > 0);

            UpdateLikeText(isLikedByCurrentUser);
            UpdateLikeCounter();
        }

        private void UpdateLikeToggle(bool isLikedByCurrentUser)
        {
            _likeToggle.SetActive(ContextData.LikeCount > 0);
            _likeToggle.EnableAnimation(false);
            _likeToggle.Toggle.isOn = isLikedByCurrentUser;
            _likeToggle.RefreshUI(_likeToggle.Toggle.isOn);
            _likeToggle.EnableAnimation(true);
            _likeToggle.Toggle.onValueChanged.AddListener(OnLikeToggleValueChanged);
        }

        private void UpdateLikeText(bool isLikedByCurrentUser)
        {
            _likeText.text = isLikedByCurrentUser ? _localization.UnlikeButton : _localization.LikeButton;
        }

        private void UpdateLikeCounter()
        {
            _likeCounter.text = ContextData.LikeCount > 0
                ? ContextData.LikeCount.ToShortenedString()
                : string.Empty;
        }

        private async void UpdateLikesModel()
        {
            ContextData.IsLikedByCurrentUser = !ContextData.IsLikedByCurrentUser;

            var chatId = ContextData.ChatId;
            var messageId = ContextData.ChatMessage.Id;

            if (ContextData.IsLikedByCurrentUser)
            {
                ContextData.LikeCount += 1;

                var result = await Bridge.LikeMessage(chatId, messageId);
                if (result.IsError)
                {
                    Debug.LogError($"Failed to like message: {result.ErrorMessage}");
                }
            }
            else
            {
                ContextData.LikeCount -= 1;

                var result = await Bridge.UnlikeMessage(chatId, messageId);
                if (result.IsError)
                {
                    Debug.LogError($"Failed to unlike message: {result.ErrorMessage}");
                }
            }
        }

        protected override void OnCommentTextClick()
        {
            if (_messageText.textInfo != null)
            {
                var linkIndex = TMP_TextUtilities.FindIntersectingLink(_messageText, Input.mousePosition, Camera.main);
            
                if( linkIndex >= 0) 
                {
                    var linkInfo = _messageText.textInfo.linkInfo[linkIndex];
                    ParseHyperlink(linkInfo.GetLinkID());
                    return;
                }
            }
            
            if (_replyText.textInfo != null)
            {
                var linkIndex = TMP_TextUtilities.FindIntersectingLink(_replyText, Input.mousePosition, Camera.main);
            
                if( linkIndex >= 0) 
                {
                    var linkInfo = _replyText.textInfo.linkInfo[linkIndex];
                    ParseHyperlink(linkInfo.GetLinkID());
                    return;
                }
            }

            base.OnCommentTextClick();
        }
        
        private void ParseHyperlink(string linkId)
        {
            var split = linkId.Split(':');

            if (split.Length == 0) return;

            switch (split[0])
            {
                case "mention":
                    if (!long.TryParse(split[1], out var groupId)) return;
                    PrefetchDataForUser(groupId);
                    break;
                
                case "profile":
                    PrefetchUser();
                    break;
            }
        }

        private void HideMessageWhenEmpty()
        {
            var hasText = !string.IsNullOrEmpty(ContextData.CommentText);
            _messageTextHolder.SetActive(hasText);
        }
    }
}