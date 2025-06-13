using System;
using System.Collections.Generic;
using Bridge.Models.ClientServer;
using Navigation.Core;

namespace Navigation.Args
{
    public sealed class UserSelectionPageArgs : PageArgs
    {
        public override PageId TargetPage => PageId.UserSelectionPage;
        public UsersFilter Filter { get; set; }
        public long? TargetProfileId { get; set; } = null;
        public ICollection<GroupShortInfo> SelectedProfiles { get; set; }
        public ICollection<GroupShortInfo> LockedProfiles { get; set; }
        public Action OnBackButton { get; set; }
        public Action<ICollection<GroupShortInfo>> OnSaveButton { get; set; }

        public enum UsersFilter
        {
            All = 0,
            Friends = 1,
            Followers = 2,
            Followed = 3
        }
    }
}
