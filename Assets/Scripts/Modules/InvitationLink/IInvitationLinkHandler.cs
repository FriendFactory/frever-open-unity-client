using System;
using System.Collections.Generic;

namespace Modules.DeepLinking
{
    public interface IInvitationLinkHandler
    {
        event Action<Dictionary<string, string>> InviteLinkRequested;
        event Action<string> OnLinkGenerated;
        InvitationLinkInfo InvitationLinkInfo { get; }
        
        void Initialize(InvitationLinkInfo info);
        void Clear();
        void GenerateLink(long userGroupId, bool isStartCreator);
        void HandleGeneratedInviteLink(string link);
    }
}