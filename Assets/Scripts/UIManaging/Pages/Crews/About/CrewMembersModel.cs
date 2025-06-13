using System;
using System.Linq;
using Bridge.Models.ClientServer.Crews;
using Extensions;

namespace UIManaging.Pages.Crews
{
    internal sealed class CrewMembersModel
    {
        public readonly long CrewId;
        public readonly int MembersCount;
        public readonly CrewMember[] Members;
        
        public CrewMembersModel(CrewModel crewModel)
        {
            CrewId = crewModel.Id;
            MembersCount = crewModel.MembersCount;
            Members = crewModel.Members;
        }
        public CrewMembersListModel ToCrewListModel()
        {
            var model = new CrewMembersListModel
            {
                BlockedMembers = MembersCount - (Members?.Length ?? 0),
                MemberModels = {  }
            };

            if (Members.IsNullOrEmpty()) return model;
            
            foreach (var member in Members)
            {
                model.MemberModels.Add(new CrewMemberModel(member));
            }

            return model;
        }
    }
}