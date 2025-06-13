using System;
using System.Collections.Generic;

namespace UIManaging.PopupSystem.Configurations
{
    public class CrewInviteFriendPopupConfiguration : PopupConfiguration
    {
        public readonly long CrewId;
        public readonly List<long> MemberIds;
        public readonly long LocalUserGroupId;

        public CrewInviteFriendPopupConfiguration(long crewId, List<long> memberIds, long localUserGroupId, Action<object> onClose) : base(PopupType.CrewInviteFriends, onClose)
        {
            CrewId = crewId;
            MemberIds = memberIds;
            LocalUserGroupId = localUserGroupId;
        }
    }
}
