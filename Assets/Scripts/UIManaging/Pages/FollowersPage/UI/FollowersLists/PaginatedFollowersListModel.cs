using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge.Services.UserProfile;
using UIManaging.Pages.Common.FollowersManagement.UserLists;

namespace UIManaging.Pages.FollowersPage.UI.FollowersLists
{
    public class PaginatedFollowersListModel
    {
        public bool IsLocalUser { get;}
        public int TabIndex { get;}
        private BaseUserList UserList { get; }
        public List<Profile> Profiles => UserList.Profiles;

        public PaginatedFollowersListModel(BaseUserList userList, bool isLocalUser, int tabIndex)
        {
            UserList = userList;
            IsLocalUser = isLocalUser;
            TabIndex = tabIndex;
        }

        public async Task<FollowerViewModel[]> GetNextFollowerViewModels(int take = 20, int skip = 0)
        {
            var profiles = await UserList.GetNextProfilesAsync(take, skip);
            var followerViews = profiles.Profiles.Select(profile => new FollowerViewModel(profile)).ToArray();
            return followerViews;
        }
    }
}