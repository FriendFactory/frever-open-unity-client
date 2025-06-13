using System.Collections.Generic;
using System.Linq;
using Bridge.Services.UserProfile;
using UIManaging.Common.SearchPanel;

namespace UIManaging.PopupSystem.Popups.StartChat
{
    public class AddMembersSearchHandler: SearchHandler
    {
        public IReadOnlyList<long> ExcludedIds { get; set; }

        protected override SearchListModel CreateSearchListModel(ICollection<Profile> profiles, bool isSearchResult)
        {
            return base.CreateSearchListModel(profiles.Where(profile => ExcludedIds == null || !ExcludedIds.Contains(profile.MainGroupId)).ToArray(), isSearchResult);
        }
    }
}