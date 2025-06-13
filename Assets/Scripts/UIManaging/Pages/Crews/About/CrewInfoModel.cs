using System;
using Bridge.Models.ClientServer.Crews;

namespace UIManaging.Pages.Crews
{
    internal sealed class CrewInfoModel
    {
        public Action LeaveCrewActionRequested;
        
        public readonly string CrewName;
        public readonly string Description;
        public readonly int MembersCount;
        public readonly int MaxMembersCount;
        public readonly int TrophyScore;
        public readonly long? LanguageId;

        public CrewInfoModel(CrewModel crewModel)
        {
            CrewName = crewModel.Name;
            Description = crewModel.Description;
            MembersCount = crewModel.MembersCount;
            TrophyScore = crewModel.Competition.TrophyScore;
            LanguageId = crewModel.LanguageId;
            
            MaxMembersCount = 20;
        }
        
        public CrewInfoModel(CrewShortInfo crewModel)
        {
            CrewName = crewModel.Name;
            Description = crewModel.Description;
            MembersCount = crewModel.MembersCount;
            MaxMembersCount = 20;
        }
    }
}