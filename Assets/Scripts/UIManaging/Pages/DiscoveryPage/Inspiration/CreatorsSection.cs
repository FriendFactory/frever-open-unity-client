using Bridge;
using Extensions;
using Navigation.Args;
using UIManaging.Pages.FollowersPage.UI.Search;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.DiscoveryPage
{
    internal sealed class CreatorsSection : VerticalDiscoverySection<SearchUserFollowItemView>
    {
        [Header("Search")]
        [SerializeField] private DiscoverySearchView _discoverySearchView;

        [Inject] private IBridge _bridge;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public async void Init()
        {
            var result = await _bridge.GetProfiles(_itemsCount, 0);

            if (result.IsSuccess)
            {
                var profilesLength = result.Profiles.Length;
                for (var i = 0; i < _itemsCount; i++)
                {
                    if (i < profilesLength)
                    {
                        var profile = result.Profiles[i];
                        Items[i].SetActive(true);
                        Items[i].Initialize(profile);
                    }
                    else
                    {
                        Items[i].SetActive(false);
                    }
                }
            }
            else if (result.IsError)
            {
                Debug.LogWarning($"Cannot get hashtags: {result.ErrorMessage}");
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        protected override void OnMoreButtonClicked()
        {
            _discoverySearchView.Show(DiscoverySearchState.Users);
        }
    }
}