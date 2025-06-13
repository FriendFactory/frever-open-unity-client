using System;
using System.Linq;
using System.Threading;
using Bridge;
using Bridge.Models.ClientServer.Chat;
using Common.Hyperlinks;
using Extensions;
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
    internal class ReplyToPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text _usernameText;
        [SerializeField] private TMP_Text _messageText;
        [Header("Media")]
        [SerializeField] private GameObject _replyPhotoHolder;
        [SerializeField] private RawImage _replyPhotoThumbnail;
        [SerializeField] private AspectRatioFitter _replyPhotoAspectFitter;
        [Space]
        [SerializeField] private GameObject _replyVideoHolder;
        [SerializeField] private VideoThumbnail _replyVideoThumbnail;

        [Inject] private IBridge _bridge;
        [Inject] private ChatLocalization _localization;

        private CancellationTokenSource CancellationSource;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        internal Action<bool> PanelShown;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnDisable()
        {
            CancelLoading();
        }

        private void OnDestroy()
        {
            CancelLoading(true);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Show(ChatMessage message)
        {
            CancellationSource?.CancelAndDispose();
            CancellationSource = new CancellationTokenSource();

            _usernameText.text = string.Format(_localization.ReplyToTitleFormat, message.Group.Nickname);
            _messageText.text = message.Text ?? string.Empty;

            var mentionsMapping = message.Mentions?.ToDictionary
            (
                mention => mention.Id,
                mention => AdvancedInputFieldUtils.GetParsedText(mention.Nickname)
            );

            var text = HyperlinkParser.ParseHyperlinks(_messageText.text, mentionsMapping, null);
            _messageText.text = AdvancedInputFieldUtils.GetParsedText(text);
            
            UpdateReplyMedia(message);

            this.SetActive(true);
            PanelShown?.Invoke(true);
        }

        public void Hide()
        {
            this.SetActive(false);
            PanelShown?.Invoke(false);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void UpdateReplyMedia(ChatMessage message)
        {
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

            _replyPhotoHolder.SetActive(true);

            var result = await _bridge.GetMessageFiles(message, CancellationSource.Token);
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

            _replyVideoHolder.SetActive(true);

            var result = await _bridge.GetVideoAsync(message.VideoId.Value, CancellationSource.Token);
            if (result.IsRequestCanceled) return;
            if (result.IsError)
            {
                Debug.LogError($"Failed to resolve video URL for ID {message.VideoId}: {result.ErrorMessage}");
                return;
            }

            _replyVideoThumbnail.SetActive(true);
            _replyVideoThumbnail.Initialize(new VideoThumbnailModel(result.ResultObject.ThumbnailUrl));
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
    }
}