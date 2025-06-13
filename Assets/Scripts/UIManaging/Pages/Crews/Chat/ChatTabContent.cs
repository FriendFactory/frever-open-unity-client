using System.Linq;
using Bridge;
using Bridge.Models.ClientServer.Chat;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Crews
{
    internal sealed class ChatTabContent : CrewTabContent
    {
        [SerializeField] private ChatView _chatView;
        [SerializeField] private MOTDHandler _motdHandler;
        
        [Inject] private IBridge _bridge;
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            _chatView.Initialize(new MessagesListLoader(_bridge, ContextData.ChatId, ContextData.Name, ChatType.Crew, ContextData.Members.Select(m=>m.Group)));
            _motdHandler.Initialize(ContextData);
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();

            _chatView.CleanUp();
            _motdHandler.CleanUp();
        }
    }
}