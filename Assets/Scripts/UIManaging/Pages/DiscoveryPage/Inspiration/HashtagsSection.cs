using System;
using Bridge;
using Bridge.Models.VideoServer;
using Extensions;
using Navigation.Args;
using UIManaging.Common.Hashtags;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.DiscoveryPage
{
    internal sealed class HashtagsSection : VerticalDiscoverySection<HashtagItemView>
    {
        [Header("Search")]
        [SerializeField] private DiscoverySearchView _discoverySearchView;

        [Inject] private IBridge _bridge;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action<HashtagInfo> HashtagClicked;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public async void Init()
        {
            var result = await _bridge.GetHashtags(null, 0, _itemsCount);

            if (result.IsSuccess)
            {
                var hashtagsCount = result.Models.Length;
                for (var i = 0; i < _itemsCount; i++)
                {
                    if (i < hashtagsCount)
                    {
                        var hashtagInfo = result.Models[i];
                        Items[i].Initialize(hashtagInfo);
                        Items[i].ItemClicked += OpenHashtagFeed;
                        Items[i].SetActive(true);
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
        // Protected
        //---------------------------------------------------------------------

        protected override void OnMoreButtonClicked()
        {
            _discoverySearchView.Show(DiscoverySearchState.Hashtags);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OpenHashtagFeed(HashtagInfo hashtagInfo)
        {
            HashtagClicked?.Invoke(hashtagInfo);
        }
    }
}