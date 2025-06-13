using System;

namespace Modules.DeepLinking
{
    public class InvitationLinkInfo
    {
        public readonly Guid InvitationGuid;
        
        public InvitationLinkInfo(string guid)
        {
            InvitationGuid = new Guid(guid);
        }
    }
}