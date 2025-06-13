using System.Collections.Generic;

namespace UIManaging.Pages.Crews
{
    internal sealed class CrewMembersListModel
    {
        public int BlockedMembers;
        public List<CrewMemberModel> MemberModels { get; } = new List<CrewMemberModel>();
    }
}