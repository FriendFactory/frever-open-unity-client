using System;
using System.Collections.Generic;
using Bridge.Models.ClientServer;
using Bridge.Models.VideoServer;
using UIManaging.PopupSystem.Popups;

namespace UIManaging.PopupSystem.Configurations
{
    public sealed class PrivacyPopupConfiguration : BasePrivacyPopupConfiguration<VideoAccess>
    {
        public readonly List<GroupShortInfo> SelectedUsers;
        public readonly List<GroupShortInfo> TaggedUsers;
        public readonly Action<PrivacyPopupResult> SelectedCallback;
        
        public bool ReopenOnSave { get; }

        public PrivacyPopupConfiguration(VideoAccess access, List<GroupShortInfo> selectedUsers, 
            List<GroupShortInfo> taggedUsers, bool reopenOnSave = true, Action<PrivacyPopupResult> onSelected = null) :
            base(access, PopupType.PrivacyPopup)
        {
            ReopenOnSave = reopenOnSave;
            SelectedUsers = selectedUsers;
            TaggedUsers = taggedUsers;
            SelectedCallback = onSelected;
        }
    }
}