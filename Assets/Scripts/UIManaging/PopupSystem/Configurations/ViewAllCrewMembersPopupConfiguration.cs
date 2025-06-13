namespace UIManaging.PopupSystem.Configurations
{
    public class ViewAllCrewMembersPopupConfiguration : PopupConfiguration
    {
        public readonly long CrewId;
        public readonly long LocalUserGroupId;
        public readonly int BlockedMembers;
        public readonly int MembersCount;
        
        public ViewAllCrewMembersPopupConfiguration(long crewId, long localUserGroupId, int membersCount, int blockedMembers) 
            : base(PopupType.ViewAllCrewMembers, null)
        {
            CrewId = crewId;
            LocalUserGroupId = localUserGroupId;
            BlockedMembers = blockedMembers;
            MembersCount = membersCount;
        }
    }
}