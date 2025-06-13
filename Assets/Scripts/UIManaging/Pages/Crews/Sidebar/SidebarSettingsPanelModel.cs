using System;
using Bridge.Models.ClientServer.Crews;
using Bridge.Models.Common;
using Bridge.Models.Common.Files;

namespace UIManaging.Pages.Crews.Sidebar
{
    internal sealed class SidebarSettingsPanelModel
    {
        public readonly IThumbnailOwner ThumbnailOwner;
        public readonly long CrewId;
        public readonly long ChatId;
        public readonly string CrewName;
        public readonly string CrewDescription;
        public readonly bool IsPublic;
        public readonly CrewMember[] Members;
        public readonly long LanguageId;

        public SidebarSettingsPanelModel(IThumbnailOwner thumbnailOwner, long crewId, long chatId, string name, 
            string description, CrewMember[] members, bool isPublic, long languageId)
        {
            ThumbnailOwner = thumbnailOwner;
            CrewId = crewId;
            ChatId = chatId;
            CrewName = name;
            CrewDescription = description;
            IsPublic = isPublic;
            Members = members;
            LanguageId = languageId;
        }
    }
}