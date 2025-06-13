using System.Collections.Generic;

namespace UIManaging.Pages.Common.VideoManagement
{
    public class FeedVideoLikesService
    {
        private readonly Dictionary<long, int> _localLikesCount = new();

        public int GetLocalGivenLikesCount(long groupId) => _localLikesCount.GetValueOrDefault(groupId, 0);
        
        public int AddLocalGivenLikesCount(long groupId, bool isLiked)
            => _localLikesCount[groupId] = _localLikesCount.GetValueOrDefault(groupId, 0)
                                         + (isLiked ? 1 : -1);
    }
}