using System.Threading.Tasks;
using Bridge;
using Bridge.Services.UserProfile;
using JetBrains.Annotations;
using Zenject;

namespace UIManaging.Pages.Common.FollowersManagement.UserLists
{
    [UsedImplicitly]
    public sealed class LocalUserFollowerList: BaseUserList
    {
        [Inject] private IBridge _bridge;
        
        protected override Task<ProfilesResult<Profile>> GetProfiles()
        {
            return _bridge.GetMyFollowers(REQUEST_USER_COUNT, 0);
        }
        
        public override Task<ProfilesResult<Profile>> GetNextProfilesAsync(int take = 20, int skip = 0)
        {
            return _bridge.GetMyFollowers(take, skip);
        }
    }
}