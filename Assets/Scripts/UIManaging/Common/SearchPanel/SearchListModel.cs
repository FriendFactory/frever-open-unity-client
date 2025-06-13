using Bridge.Services.UserProfile;

namespace UIManaging.Common.SearchPanel
{
    public class SearchListModel 
    {
        public Profile[] Users { get; private set; }
        public bool IsSearchResult { get; }
        
        public SearchListModel(Profile[] followers, bool isSearchResult)
        {
            SetProfiles(followers);
            IsSearchResult = isSearchResult;
        }

        public void SetProfiles(Profile[] followers)
        {
            Users = followers;
        }
    }
}