using System.Linq;
using Abstract;
using Bridge.Models.ClientServer.Chat;
using UnityEngine;

namespace UIManaging.Pages.Crews
{
    public class ChatView: BaseContextDataView<IChatModel>
    {
        [SerializeField] private MessagesListHandler _messagesListHandler;
        [SerializeField] private ChatTabInputHandler _chatInputHandler;
        [SerializeField] private ChatIntroHandler _chatIntroHandler;
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            _messagesListHandler.Initialize(ContextData);
            _messagesListHandler.ContextData.ReplyRequested += OnReply;

            _chatInputHandler.Initialize(ContextData.ChatId, ContextData.Members.ToArray());
            _chatInputHandler.HeightChanged += OnHeightChanged;

            if (ContextData.ChatType != ChatType.Crew)
            {
                _chatIntroHandler.Initialize(ContextData);
            }
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();

            if (_messagesListHandler.ContextData != null)
            {
                _messagesListHandler.ContextData.ReplyRequested -= OnReply;
            }

            _messagesListHandler.CleanUp();
            
            _chatInputHandler.HeightChanged -= OnHeightChanged;
            _chatInputHandler.Cleanup();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnReply(ChatMessage message)
        {
            _chatInputHandler.ShowReplyToPanel(message);
        }

        private void OnHeightChanged(float height)
        {
            _messagesListHandler.OnInputHeightChanged(height);
        }
    }
}