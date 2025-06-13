using System.Threading.Tasks;
using Bridge;
using Bridge.Services.UserProfile;
using JetBrains.Annotations;
using Zenject;

namespace UIManaging.Pages.Common.FollowersManagement.UserLists
{
    [UsedImplicitly]
    public sealed class RemoteUserFollowedList: BaseUserList
    {
        [Inject] private IBridge _bridge;
        
        public readonly long UserGroupId;

        public RemoteUserFollowedList(long userGroupId)
        {
            UserGroupId = userGroupId;
        }
        
        protected override Task<ProfilesResult<Profile>> GetProfiles()
        {
            return _bridge.GetFollowedBy(UserGroupId, REQUEST_USER_COUNT, 0);
        }

        public override Task<ProfilesResult<Profile>> GetNextProfilesAsync(int take = 20, int skip = 0)
        {
            return _bridge.GetFollowedBy(UserGroupId, take, skip);
        }
        
        public class Factory: PlaceholderFactory<long, RemoteUserFollowedList> { }
    }
}