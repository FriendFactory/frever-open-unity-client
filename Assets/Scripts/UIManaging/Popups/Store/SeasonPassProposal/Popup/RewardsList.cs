using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Gamification;
using UnityEngine;

namespace UIManaging.Popups.Store.SeasonPassProposal.Popup
{
    internal sealed class RewardsList: RewardsListBase
    {
        [SerializeField] private Transform _container;
        
        public void Setup(ICollection<SeasonReward> premiumRewards)
        {
            DestroyAllViews();

            var viewModels = CreateModels(premiumRewards);
            for (var i = 0; i < MaxItemToShow && i < viewModels.Count; i++)
            {
                var model = viewModels.ElementAt(i);
                var prefab = GetPrefab(model);
                if (prefab == null) continue;
                var view = Instantiate(prefab, _container);
                view.Setup(model);
                ItemViews.Add(view);
            }
        }
    }
}