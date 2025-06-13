using System;

namespace UIManaging.PopupSystem.Configurations
{
    public sealed class ManageCrewMemberPopupConfiguration : PopupConfiguration
    {
        public ManageCrewMemberPopupConfiguration() : base(PopupType.ManageCrewMember, null)
        {
            
        }
        public long MemberRoleId { get; set; }
        public long MemberGroupId { get; set; }
        public long CrewId { get; set; }
        public string NickName { get; set; }
        public string LastLogin { get; set; }
        public string Joined { get; set; }
    }
}