using Modules.FollowRecommendations;

namespace UIManaging.Pages.FollowersPage.Recommendations
{
    public sealed class FollowersSearchListModel
    {
        public IFollowRecommendationsListModel FollowRecommendationsModel { get; }
        public IFollowRecommendationsListModel FollowBackRecommendationsModel { get; }
        public bool IsLocalUser { get; }
        public int InitialTabIndex { get; }
        
        public FollowersSearchListModel(IFollowRecommendationsListModel followRecommendationsModel, IFollowRecommendationsListModel followBackRecommendationsModel,
            bool isLocalUser, int initialTabIndex)
        {
            FollowRecommendationsModel = followRecommendationsModel;
            FollowBackRecommendationsModel = followBackRecommendationsModel;
            IsLocalUser = isLocalUser;
            InitialTabIndex = initialTabIndex;
        }
    }
}