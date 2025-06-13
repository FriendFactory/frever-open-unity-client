using System;
using System.Collections.Generic;
using Bridge.Models.ClientServer.Crews;
using Bridge.Models.Common;
using Bridge.Models.Common.Files;

namespace UIManaging.Pages.Crews.Sidebar
{
    internal sealed class CrewManagementViewModel
    {
        public readonly IThumbnailOwner ThumbnailOwner;
        public readonly long CrewId;
        public readonly long ChatId;
        public readonly string CrewName;
        public readonly string CrewDescription;
        public readonly CrewMember[] Members;
        public readonly int MaxCount;
        public readonly bool IsPublic;
        public List<FileInfo> Files;
        public string MessageOfDay { get; set; }

        public long LanguageId;
        public bool OpenRequestsTab;

        public CrewManagementViewModel(CrewModel crewModel, bool openRequestsTab = false)
        {
            ThumbnailOwner = crewModel;
            CrewId = crewModel.Id;
            ChatId = crewModel.ChatId;
            CrewName = crewModel.Name;
            CrewDescription = crewModel.Description;

            Members = crewModel.Members;
            MaxCount = crewModel.TotalMembersCount;
            IsPublic = crewModel.IsPublic;
            MessageOfDay = crewModel.MessageOfDay;
            Files = crewModel.Files;
            LanguageId = crewModel.LanguageId ?? -1;
            OpenRequestsTab = openRequestsTab;
        }
    }
}