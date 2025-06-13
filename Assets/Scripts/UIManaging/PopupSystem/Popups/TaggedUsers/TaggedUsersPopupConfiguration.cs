using System.Collections.Generic;
using Bridge.Models.VideoServer;
using UIManaging.PopupSystem.Configurations;

namespace UIManaging.PopupSystem.Popups.TaggedUsers
{
    public sealed class TaggedUsersPopupConfiguration : PopupConfiguration
    {
        public TaggedGroup[] TaggedGroups { get; }

        public TaggedUsersPopupConfiguration(TaggedGroup[] taggedGroups) : base(PopupType.TaggedUsers, null, string.Empty)
        {
            TaggedGroups = taggedGroups;
        }
    }
}