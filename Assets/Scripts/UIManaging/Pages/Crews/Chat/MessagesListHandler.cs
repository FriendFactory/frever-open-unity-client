using System.Threading.Tasks;
using Abstract;
using Extensions;
using Modules.Chat;
using UIManaging.Common;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Crews
{
    public class MessagesListHandler : BaseContextDataView<IChatModel>
    {
        [SerializeField] private PeriodicUpdater _updater;
        [SerializeField] private MessagesListView _messagesListView;
        [SerializeField] private RectTransform _contentTransform;
        [Inject] private IChatService _chatService;
        
        private bool _bringToLastMessage;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            _messagesListView.Initialize(ContextData);
            _updater.CallUpdateFunc = ExecuteUpdate;
            _updater.IsRefreshing = () => ContextData != null && !ContextData.IsAwaitingData;
            _chatService.MessagePosted += OnMessageSent;
        }

        protected override void BeforeCleanup()
        {
            _updater.CallUpdateFunc = null;
            _updater.IsRefreshing = null;
            _messagesListView.CleanUp();
            _chatService.MessagePosted -= OnMessageSent;
            
            base.BeforeCleanup();
        }

        private void OnMessageSent()
        {
            _updater.ResetRefreshingTime();
            _bringToLastMessage = true;
        }

        public void OnInputHeightChanged(float height)
        {
            _contentTransform.SetTop(-height);
            _contentTransform.SetBottom(height);
        }

        private async Task<bool> ExecuteUpdate()
        {
            var result = await _messagesListView.CheckForNewMessages(_bringToLastMessage);
            _bringToLastMessage = false;
            return result;
        }
    }
}