using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Abstract;
using Bridge;
using Bridge.Models.ClientServer.Chat;
using Bridge.Models.VideoServer;
using Navigation.Args.Feed;
using Navigation.Core;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.Common.VideoManagement;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Crews
{
    internal sealed class MessagesListView : BaseContextDataView<IChatModel>
    {
        //---------------------------------------------------------------------
        // Fields
        //---------------------------------------------------------------------

        [SerializeField] private GameObject _noCommentsPlaceholder;
        [SerializeField] private MessagesListAdapter _messagesListAdapter;
        [SerializeField] private PhotoPreviewPanel _photoPreviewPanel;

        [Inject] private LocalUserDataHolder _localUser;
        [Inject] private IBridge _bridge;
        [Inject] private PageManager _pageManager;
        [Inject] private VideoManager _videoManager;
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] private PopupManager _popupManager;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public async Task<bool> CheckForNewMessages(bool bringToLastMessage = false)
        {
            var result = (ContextData.Messages.Count == 0)
                ? await ContextData.DownloadPrevPage()
                : await ContextData.DownloadNextPage();

            if (result && bringToLastMessage)
            {
                Canvas.ForceUpdateCanvases();
                _messagesListAdapter.ResetItems(_messagesListAdapter.Data.Count);
                _messagesListAdapter.SmoothScrollTo(_messagesListAdapter.Data.Count - 1, 0.3f, 0.5f);
            }

            return result;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            ContextData.StartPageLoaded += StartPageLoaded;
            ContextData.EndPageLoaded += EndPageLoaded;

            ContextData.NewPagePrepended += FirstPageLoaded;
            ContextData.NewPageAppended += FirstPageLoaded;

            ContextData.DownloadFirstPage();

            StartCoroutine(InitCoroutine());
        }

        private IEnumerator InitCoroutine()
        {
            yield return new WaitForEndOfFrame();

            _messagesListAdapter.Refresh();
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();

            _messagesListAdapter.ScrolledToTop -= OnScrolledToTop;
            _messagesListAdapter.ScrolledToBottom -= OnScrolledToBottom;

            if (ContextData == null) return;

            ContextData.StartPageLoaded -= StartPageLoaded;
            ContextData.EndPageLoaded -= EndPageLoaded;

            ContextData.NewPagePrepended -= FirstPageLoaded;
            ContextData.NewPagePrepended -= OnNewPageAppended;

            ContextData.NewPageAppended -= FirstPageLoaded;
            ContextData.NewPageAppended -= OnNewPagePrepended;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private async void OnScrolledToTop()
        {
            if (!ContextData.IsAwaitingData) await ContextData.DownloadPrevPage();
        }

        private async void OnScrolledToBottom()
        {
            if (!ContextData.IsAwaitingData) await ContextData.DownloadNextPage();
        }

        private void StartPageLoaded()
        {
            _noCommentsPlaceholder.gameObject.SetActive(ContextData.Messages.Count == 0);
            _messagesListAdapter.ScrolledToTop -= OnScrolledToTop;
        }

        private void EndPageLoaded()
        {
            _noCommentsPlaceholder.gameObject.SetActive(ContextData.Messages.Count == 0);
            _messagesListAdapter.ScrolledToBottom -= OnScrolledToBottom;
        }

        private void FirstPageLoaded(ChatMessage[] messages)
        {
            _noCommentsPlaceholder.gameObject.SetActive(messages.Length == 0);

            ContextData.NewPagePrepended -= FirstPageLoaded;
            ContextData.NewPagePrepended += OnNewPagePrepended;

            ContextData.NewPageAppended -= FirstPageLoaded;
            ContextData.NewPageAppended += OnNewPageAppended;

            var models = messages.Reverse().Select(ConvertToModel).ToList();

            _messagesListAdapter.ResetItems(models);
            _messagesListAdapter.BringToView(models.Count - 1);

            _messagesListAdapter.ScrolledToTop += OnScrolledToTop;
            _messagesListAdapter.ScrolledToBottom += OnScrolledToBottom;
        }

        private void OnNewPageAppended(ChatMessage[] messages)
        {
            var models = messages.Reverse().Select(ConvertToModel).ToList();

            _messagesListAdapter.InsertItemsAtEnd(models);
        }

        private void OnNewPagePrepended(ChatMessage[] messages)
        {
            var models = messages.Reverse().Select(ConvertToModel).ToList();

            _messagesListAdapter.InsertItemsAtStart(models, true);
        }

        private MessageItemModel ConvertToModel(ChatMessage message)
        {
            if (message.IsMessageType(MessageType.User))
            {
                if (message.Group?.Id == _localUser.GroupId)
                {
                    var username = _localUser.NickName;
                    var model = new OwnMessageModel(ContextData.ChatId, ContextData.ChatType, message, username, OnReply, ShowPhotoPreview, OpenVideoInFeed);
                    model.OnLongPress += ShowContextMenu;
                    return model;
                }
                else
                {
                    var username = message.Group?.Nickname ?? $"Unknown";
                    var model = new UserMessageModel(ContextData.ChatId, ContextData.ChatType, message, username, OnReply, ShowPhotoPreview, OpenVideoInFeed);
                    model.OnLongPress += ShowContextMenu;
                    return model;
                }
            }

            if (message.IsMessageType(MessageType.System))
            {
                return new SystemMessageModel(ContextData.ChatId, ContextData.ChatType, message, ContextData.ChatName, OnReply, ShowPhotoPreview, OpenVideoInFeed);
            }

            if (message.IsMessageType(MessageType.Bot))
            {
                return new BotMessageModel(ContextData.ChatId, ContextData.ChatType, message, ContextData.ChatName, OnReply, ShowPhotoPreview, OpenVideoInFeed);
            }

            return null;
        }

        private void OnReply(ChatMessage message)
        {
            ContextData?.Reply(message);
        }

        private void OpenVideoInFeed(Video video)
        {
            _pageManager.MoveNext(PageId.Feed, new ChatVideoFeedArgs(_videoManager, _pageManager, video));
        }

        private void ShowPhotoPreview(Texture2D photo)
        {
            _photoPreviewPanel.Show(photo);
        }

        private void ShowContextMenu(MessageItemModel messageModel)
        {
            if (messageModel.GroupId == _localUser.GroupId)
            {
                ShowOwnContextMenu(messageModel);
            }
            else
            {
                ShowUserContextMenu(messageModel);
            }
        }

        private void ShowUserContextMenu(MessageItemModel messageModel)
        {
            _popupManagerHelper.ShowChatUserMessageActions(OnReportRequested);

            void OnReportRequested()
            {
                var cfg = new ReportMessagePopupConfiguration(messageModel.ChatId, messageModel.ChatMessage.Id, null);
                _popupManager.SetupPopup(cfg);
                _popupManager.ShowPopup(cfg.PopupType);
            }
        }

        private void ShowOwnContextMenu(MessageItemModel messageModel)
        {
            _popupManagerHelper.ShowChatOwnMessageActions(() => OnMessageDeletionRequested(messageModel));
        }

        private void OnMessageDeletionRequested(MessageItemModel messageModel)
        {
            _popupManagerHelper.ShowDialogPopup(
                "Are you sure?",
                "Once you delete your message it cannot be undone. Do you want to continue?",
                "Cancel", () => _popupManager.ClosePopupByType(PopupType.DialogDarkV3),
                "Delete", () => DeleteMessage(messageModel), true
            );
        }

        private async void DeleteMessage(MessageItemModel messageModel)
        {
            _popupManager.ClosePopupByType(PopupType.DialogDarkV3);

            var result = await _bridge.DeleteMessage(messageModel.ChatId, messageModel.ChatMessage.Id);

            if (result.IsSuccess)
            {
                var data = _messagesListAdapter.Data;
                var index = data.List.FindIndex(model => model.ChatMessage.Id == messageModel.ChatMessage.Id);
                if (index >= 0) data.RemoveOne(index);
            }
            else if (result.IsError)
            {
                Debug.LogError($"Unable to delete message. Reason: {result.ErrorMessage}");
            }
        }
    }
}