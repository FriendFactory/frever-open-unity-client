using System.Collections.Generic;
using System.Linq;
using Bridge.Models.VideoServer;
using Bridge;
using UIManaging.Pages.Common.VideoManagement;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Common.Args.Views.LevelPreviews;
using UIManaging.Common.Loaders;

namespace UIManaging.Common.Args.Views.LastLevelsPanelArgs
{
    public abstract class BaseVideoListLoader : GenericPaginationLoader<Video>
    {
        private const int DEFAULT_PAGE_SIZE = 18;
        
        public readonly List<BaseLevelItemArgs> LevelPreviewArgs = new List<BaseLevelItemArgs>();
        
        protected readonly IBridge Bridge;
        protected readonly PageManager PageManager;
        protected readonly VideoManager VideoManager;
        protected readonly long UserGroupId;
        
        protected string LastLoadedItemKey => Models?.Count > 0 ? Models[Models.Count-1].Key : null;
        
        protected override int DefaultPageSize => DEFAULT_PAGE_SIZE;

        protected BaseVideoListLoader(VideoManager videoManager, PageManager pageManager, IBridge bridge, long userGroupId)
        {
            VideoManager = videoManager;
            PageManager = pageManager;
            Bridge = bridge;
            UserGroupId = userGroupId;
        }

        public virtual void AdjustToRowSize(int rowSize) { }
        
        public virtual void OnTaskClicked() { }

        protected override void OnFirstPageLoaded(Video[] page)
        {
            LevelPreviewArgs.InsertRange(PrependDataIndex, page.Select(VideoToLevelPreviewArgs));
        }

        protected override void OnNextPageLoaded(Video[] page)
        {
            LevelPreviewArgs.AddRange(page.Select(VideoToLevelPreviewArgs));
        }

        protected virtual BaseLevelItemArgs VideoToLevelPreviewArgs(Video video)
        {
            return new LevelPreviewItemArgs(video, OnVideoPreviewClicked);
        }

        protected abstract void OnVideoPreviewClicked(BaseLevelItemArgs args);
    }
}