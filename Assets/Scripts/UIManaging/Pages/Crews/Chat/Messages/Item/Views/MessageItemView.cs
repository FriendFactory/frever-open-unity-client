using System.Linq;
using System.Threading;
using Abstract;
using Bridge;
using Bridge.Models.ClientServer.Chat;
using Bridge.Models.VideoServer;
using Common.Hyperlinks;
using Common.UI;
using Extensions;
using JetBrains.Annotations;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.Common;
using UIManaging.Common.InputFields;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Crews
{
    internal abstract class MessageItemView<TModel> : BaseContextDataView<TModel> where TModel : MessageItemModel
    {
        //---------------------------------------------------------------------
        // Fields
        //---------------------------------------------------------------------

        private Video _video;
        
        [SerializeField] private TextMeshProUGUI _timeStampText;
        [Space]
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private LayoutElement _layoutElement;
        [Space]
        [FormerlySerializedAs("_commentText")]
        [SerializeField]
        protected TextMeshProUGUI _messageText;
        [Space]
        [SerializeField] private LongPressButton _replyLongPresButton;

        [Header("Media")]
        [SerializeField] private GameObject _mediaHolder;
        [Space]
        [SerializeField] private LongPressButton _photoButton;
        [SerializeField] private GameObject _photoHolder;
        [SerializeField] private RawImage _photoThumbnail;
        [SerializeField] private AspectRatioFitter _photoAspectFitter;
        [Space]
        [SerializeField] private LongPressButton _videoButton;
        [SerializeField] private GameObject _videoHolder;
        [SerializeField] private VideoThumbnail _videoThumbnail;

        [Header("Zenject")]
        [SerializeField] private ZenProjectContextInjecter _zenAutoInjecter;

        protected CancellationTokenSource CancellationSource;
        protected IBridge Bridge;
        protected PageManager PageManager;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        protected RectTransform RectTransform => _rectTransform;
        protected LayoutElement LayoutElement => _layoutElement;
        protected TextMeshProUGUI MessageText => _messageText;
        protected virtual string MentionStyle => HyperlinkParser.MENTION_STYLE_PINK;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject, UsedImplicitly]
        private void Construct(IBridge bridge, PageManager pageManager)
        {
            Bridge = bridge;
            PageManager = pageManager;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnDisable()
        {
            CancelLoading();
        }

        protected override void OnDestroy()
        {
            CancelLoading(true);
            base.OnDestroy();
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            _zenAutoInjecter.Awake();

            if (_replyLongPresButton)
            {
                _replyLongPresButton.onClick.AddListener(OnCommentTextClick);
                _replyLongPresButton.onLongPress.AddListener(ContextData.OpenContextMenu);
            }

            SetupCommentTextField();

            CancellationSource?.CancelAndDispose();
            CancellationSource = new CancellationTokenSource();

            SetupElapsedTimeText();

            UpdateAttachedMedia(ContextData.ChatMessage);
        }

        protected override void BeforeCleanup()
        {
            CancelLoading();

            Destroy(_photoThumbnail.texture);
            _photoThumbnail.texture = null;

            if (_replyLongPresButton)
            {
                _replyLongPresButton.onClick.RemoveListener(OnCommentTextClick);
                _replyLongPresButton.onLongPress.RemoveListener(ContextData.OpenContextMenu);
            }

            _photoButton.onClick.RemoveAllListeners();
            _photoButton.onLongPress.RemoveAllListeners();

            _videoButton.onClick.RemoveAllListeners();
            _videoButton.onLongPress.RemoveAllListeners();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void SetupCommentTextField()
        {
            var mentionsMapping = ContextData.ChatMessage.Mentions?.ToDictionary
            (
                mention => mention.Id,
                mention => AdvancedInputFieldUtils.GetParsedText(mention.Nickname)
            );

            var text = HyperlinkParser.ParseHyperlinks(ContextData.CommentText, mentionsMapping, null, MentionStyle) ?? string.Empty;

            _messageText.text = text;
            _layoutElement.enabled = false;

            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
        }

        private void SetupElapsedTimeText()
        {
            _timeStampText.text = ContextData.TimeStampText;
        }

        private void CancelLoading(bool dispose = false)
        {
            if (dispose)
            {
                CancellationSource?.CancelAndDispose();
                CancellationSource = null;
            }
            else
            {
                CancellationSource?.Cancel();
            }
        }

        private void UpdateAttachedMedia(ChatMessage message)
        {
            _mediaHolder.SetActive(false);

            _photoHolder.SetActive(false);
            _photoThumbnail.SetActive(false);

            _videoHolder.SetActive(false);
            _videoThumbnail.SetActive(false);

            UpdateAttachedPhoto(message);
            UpdateAttachedVideo(message);
        }

        private async void UpdateAttachedPhoto(ChatMessage message)
        {
            if (message.Files == null) return;

            _mediaHolder.SetActive(true);
            _photoHolder.SetActive(true);

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

            _photoButton.onClick.RemoveAllListeners();
            _photoButton.onClick.AddListener(() => OnPhotoThumbnailClicked(texture));
            _photoButton.onLongPress.AddListener(ContextData.OpenContextMenu);

            _photoAspectFitter.aspectRatio = texture.width / (float) texture.height;
            _photoThumbnail.texture = texture;
            _photoThumbnail.gameObject.SetActive(true);
        }

        private async void UpdateAttachedVideo(ChatMessage message)
        {
            if (message.VideoId == null) return;

            _mediaHolder.SetActive(true);
            _videoHolder.SetActive(true);

            _videoButton.onClick.RemoveAllListeners();
            _videoButton.onClick.AddListener(OnVideoThumbnailClicked);
            _videoButton.onLongPress.AddListener(ContextData.OpenContextMenu);

            var result = await Bridge.GetVideoAsync(message.VideoId.Value, CancellationSource.Token);
            if (result.IsRequestCanceled) return;
            if (result.IsError)
            {
                Debug.LogWarning($"Failed to resolve video URL for ID {message.VideoId}: {result.ErrorMessage}");
                return;
            }

            _video = result.ResultObject;
            _videoThumbnail.SetActive(true);
            _videoThumbnail.Initialize(new VideoThumbnailModel(result.ResultObject.ThumbnailUrl));
        }

        private void OnPhotoThumbnailClicked(Texture2D photo)
        {
            ContextData.OnPhotoThumbnailClicked(photo);
        }

        private void OnVideoThumbnailClicked()
        {
            if (_video == null) return;

            ContextData.OnVideoThumbnailClicked(_video);
        }

        protected virtual void OnCommentTextClick()
        {
            ContextData.OnReply();
        }
    }
}