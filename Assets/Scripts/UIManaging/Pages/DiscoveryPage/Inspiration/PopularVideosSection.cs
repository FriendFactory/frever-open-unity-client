using Bridge;
using Extensions;
using JetBrains.Annotations;
using Navigation.Args;
using Navigation.Args.Feed;
using Navigation.Core;
using UIManaging.Common.Args.Views.LevelPreviews;
using UIManaging.Pages.Common.VideoManagement;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.DiscoveryPage
{
    internal sealed class PopularVideosSection : ItemsDiscoverySection<DiscoveryVideoItem>
    {
        private IBridge _bridge;
        private PageManager _pageManager;
        private VideoManager _videoManager;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject, UsedImplicitly]
        private void Construct(IBridge bridge, PageManager pageManager, VideoManager videoManager)
        {
            _bridge = bridge;
            _pageManager = pageManager;
            _videoManager = videoManager;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public async void Init()
        {
            var result = await _bridge.GetTrendingVideoListAsync(null, _itemsCount);

            if (result.IsSuccess)
            {
                var videosCount = result.Models.Length;
                for (var i = 0; i < _itemsCount; i++)
                {
                    if (i < videosCount)
                    {
                        var video = result.Models[i];
                        var previewItem = new LevelPreviewItemArgs(video, OnVideoPreviewClicked);
                        Items[i].SetActive(true);
                        Items[i].Initialize(previewItem);
                    }
                    else
                    {
                        Items[i].SetActive(false);
                    }
                }
            }
            else if (result.IsError)
            {
                Debug.LogWarning($"Cannot get trending videos: {result.ErrorMessage}");
            }
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnMoreButtonClicked()
        {
            _pageManager.MoveNext(PageId.Feed, new TrendingFeedArgs(_videoManager, null));
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnVideoPreviewClicked(BaseLevelItemArgs args)
        {
            _pageManager.MoveNext(PageId.Feed, new TrendingFeedArgs(_videoManager, args.Video.Id));
        }
    }
}