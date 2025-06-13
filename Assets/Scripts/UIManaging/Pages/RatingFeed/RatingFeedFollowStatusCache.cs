using Common.Collections;

namespace UIManaging.Pages.RatingFeed
{
    internal enum UserFollowStatus
    {
        NotFollowed = 0,
        Follower = 1,
        Friend = 2,
    }
    
    internal sealed class RatingFeedFollowStatusCache: DictionaryCache<long, UserFollowStatus> { }
}