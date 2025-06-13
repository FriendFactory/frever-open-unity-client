using System.Collections.Generic;

namespace UIManaging.Pages.Crews.Popups.CrewInvite
{
    public class CrewInviteFriendsSearchViewModel
    {
        public readonly long LocalUserGroupId;
        public List<long> MemberIds;
        public readonly long[] InvitedUsers;

        public CrewInviteFriendsSearchViewModel(long localUserGroupId, List<long> memberIds, long[] invitedUsers)
        {
            InvitedUsers = invitedUsers;
            MemberIds = memberIds;
            LocalUserGroupId = localUserGroupId;
        }
    }
}