using Bridge.Models.ClientServer.Invitation;
using Navigation.Core;

namespace UIManaging.Pages.DiscoverPeoplePage.StarCreator.AcceptedInvitations
{
    public class StarCreatorAcceptedInvitationsPageArgs: PageArgs
    {
        public override PageId TargetPage => PageId.StarCreatorAcceptedInvitations;
        public InviteGroup[] Invitations;
    }
}