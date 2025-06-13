using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Crews;
using Common;
using Extensions;
using UIManaging.Localization;

namespace UIManaging.Pages.Crews.Sidebar
{
    internal sealed class SidebarMembersModel
    {
        public readonly long CrewId;
        public readonly int TotalMemberCount;
        public readonly int BlockedMembersCount;
        public readonly List<SidebarMembersListItemModel> Models = new List<SidebarMembersListItemModel>();

        private readonly long UserRole;
        private readonly int MaxUserCount;
        private readonly CrewPageLocalization _localization;
        
        public SidebarMembersModel(long userRole, long crewId, int totalMemberCount, int blockedMembersCount, CrewMember[] crewMembers, int maxUserCount, CrewPageLocalization localization)
        {
            CrewId = crewId;
            MaxUserCount = maxUserCount;
            _localization = localization;
            UserRole = userRole;
            TotalMemberCount = totalMemberCount;
            BlockedMembersCount = blockedMembersCount;
            SetupMembersListItemModels(crewMembers);
        }

        public void UpdateMembersList(IReadOnlyCollection<CrewMember> members)
        {
            SetupMembersListItemModels(members);
        }

        private void SetupMembersListItemModels(IReadOnlyCollection<CrewMember> members)
        {
            Models.Clear();
            var memberIds = members.Select(m => m.Group.Id).ToList();
            Models.Add(new SidebarMembersListHeaderModel(UserRole, CrewId, memberIds, MaxUserCount));

            var roles = members.DistinctBy(m => m.RoleId)
                                   .Select(m => m.RoleId)
                                   .OrderBy(id => id)
                                   .ToList();

            var localUserCanEdit = UserRole  <= Constants.Crew.COORDINATOR_ROLE_ID;
            foreach (var roleId in roles)
            {
                var groupMembers = members.Where(m => m.RoleId == roleId);
                Models.Add(new SidebarMembersListRoleModel( roleId, _localization.GetRankLocalized(roleId), groupMembers.Count()));
                groupMembers.ForEach(m => Models.Add(new SidebarMembersListMemberModel(m, CrewId, localUserCanEdit)));
            }

            if (BlockedMembersCount != 0)
            {
                Models.Add(new SidebarMembersListRoleModel(0,_localization.BlockedUsersTitle, BlockedMembersCount));
            }
        }
    }
}