using System;
using Bridge.Models.Common;
using Bridge.Models.Common.Files;

namespace UIManaging.PopupSystem.Configurations
{
    public class EditCrewPopupConfiguration : PopupConfiguration
    {
        public readonly bool IsPublic;
        public IThumbnailOwner ThumbnailOwner;
        public string CrewName { get; private set; }
        public string CrewDescription { get; private set; }
        public long LanguageId { get; private set; }

        public EditCrewPopupConfiguration(IThumbnailOwner thumbnailOwner, string name, string description, bool isPublic, 
            long languageId, Action<object> onClose)
            : base(PopupType.EditCrew, onClose)
        {
            ThumbnailOwner = thumbnailOwner;
            CrewName = name;
            CrewDescription = description;
            IsPublic = isPublic;
            LanguageId = languageId;
        }

        public void UpdateCrewName(string crewName)
        {
            CrewName = crewName;
        }

        public void UpdateCrewDescription(string description)
        {
            CrewDescription = description;
        }

        public void UpdateCrewLanguage(long id)
        {
            LanguageId = id;
        }
    }
}