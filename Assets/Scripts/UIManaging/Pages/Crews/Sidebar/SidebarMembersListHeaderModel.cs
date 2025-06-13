using System.Collections.Generic;

namespace UIManaging.Pages.Crews.Sidebar
{
    public sealed class SidebarMembersListHeaderModel : SidebarMembersListItemModel
    {
        public long UserRole;
        public readonly long CrewId;
        public readonly List<long> MemberIds;
        public readonly string MembersCountText;

        public SidebarMembersListHeaderModel(long roleId, long crewId, List<long> memberIds,int maxCount)
        {
            UserRole = roleId;
            MemberIds = memberIds;
            CrewId = crewId;
            MembersCountText = $"{memberIds.Count}/{maxCount}";
        }
    }
}