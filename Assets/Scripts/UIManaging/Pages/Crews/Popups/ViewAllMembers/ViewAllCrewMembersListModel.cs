namespace UIManaging.Pages.Crews.Popups
{
    internal sealed class ViewAllCrewMembersListModel
    {
        public readonly long CrewId;
        public readonly int BlockedMembers;
        public readonly long LocalUserGroupId;
        public int MembersCount;

        public ViewAllCrewMembersListModel(long crewId, long localUserGroupId, int membersCount, int blockedMembersCount)
        {
            CrewId = crewId;
            LocalUserGroupId = localUserGroupId;
            BlockedMembers = blockedMembersCount;
            MembersCount = membersCount;
        }
    }
}