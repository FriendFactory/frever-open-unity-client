using Modules.FollowRecommendations;
using UnityEngine;

namespace UIManaging.Pages.FollowersPage.Recommendations
{
    internal class FollowRecommendationsView : MonoBehaviour
    {
        [SerializeField] protected FollowRecommendationsList _recommendationsList;
        
        public bool IsInitialized { get; private set; }

        public void Initialize(IFollowRecommendationsListModel listModel)
        {
            if (IsInitialized) return;

            IsInitialized = true;

            _recommendationsList.Initialize(listModel);
        }

        private void OnDestroy()
        {
            _recommendationsList.CleanUp();
        }
    }
}