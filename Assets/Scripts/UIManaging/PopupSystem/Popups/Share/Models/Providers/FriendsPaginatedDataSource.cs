using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Services.UserProfile;

namespace UIManaging.PopupSystem.Popups.Share
{
    internal sealed class FriendsPaginatedDataSource: PaginatedDataSource<Profile>
    {
        public FriendsPaginatedDataSource(IBridge bridge, int pageSize) : base(bridge, pageSize) { }
        
        protected override List<Profile> GetFilteredModels(string searchQuery)
        {
            return Models.Where(model => model.NickName.Contains(searchQuery)).ToList();
        }

        protected override async Task<PageResult<Profile>> GetModelsInternal(int take, int skip, CancellationToken token)
        {
            var result = await Bridge.GetMyFriends(take, skip, null, true, false, token);
            
            if (result.IsError) return PageResult<Profile>.Error(result.ErrorMessage);

            if (result.IsRequestCanceled) return PageResult<Profile>.Cancelled();
            
            return PageResult<Profile>.Success(result.Profiles);
        }
    }
}