using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Gamification;
using Extensions;
using UnityEngine;

namespace UIManaging.Popups.Store.SeasonPassProposal
{
    internal sealed class RewardsList : RewardsListBase
    {
        [SerializeField] private ExploreMorePremiumRewardsView _exploreMoreView;
        
        public void Setup(ICollection<SeasonReward> rewards)
        {
            DestroyAllViews();

            var viewModels = CreateModels(rewards);
            var canShowAllItems = viewModels.Count <= MaxItemToShow;
            var itemsToShowCount = canShowAllItems 
                ? Mathf.Clamp(viewModels.Count, 0, MaxItemToShow) 
                : MaxItemToShow - 1; //reserve 1 place for "more" button

            for (var i = 0; i < itemsToShowCount; i++)
            {
                var model = viewModels.ElementAt(i);
                var prefab = GetPrefab(model);
                if (prefab == null) continue;
                var view = Instantiate(prefab, transform);
                view.Setup(model);
                ItemViews.Add(view);
            }
            
            _exploreMoreView.SetActive(!canShowAllItems);
            if (!canShowAllItems)
            {
                var notShownCount = viewModels.Count - itemsToShowCount;
                _exploreMoreView.Setup(notShownCount);
                _exploreMoreView.transform.SetSiblingIndex(itemsToShowCount);
            }
        }
    }
}