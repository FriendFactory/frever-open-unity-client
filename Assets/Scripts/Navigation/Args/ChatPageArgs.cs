using Bridge.Models.ClientServer.Chat;
using Navigation.Core;

namespace Navigation.Args
{
    public sealed class ChatPageArgs : PageArgs
    {
        public ChatInfo ChatInfo { get; }
        public bool OpenInboxPageOnExit { get; }

        public ChatPageArgs(ChatInfo chatInfo, bool openInboxPageOnExit = false)
        {
            ChatInfo = chatInfo;
            OpenInboxPageOnExit = openInboxPageOnExit;
        }
        
        public override PageId TargetPage => PageId.ChatPage;
    }
}