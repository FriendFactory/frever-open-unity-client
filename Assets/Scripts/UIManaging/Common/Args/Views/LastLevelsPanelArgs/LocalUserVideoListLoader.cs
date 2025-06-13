using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge.Models.VideoServer;
using Bridge;
using Common.BridgeAdapter;
using Common.Publishers;
using Navigation.Args;
using Navigation.Args.Feed;
using Navigation.Core;
using UIManaging.Common.Args.Views.LevelPreviews;
using UIManaging.Pages.Common.VideoManagement;
using System;

namespace UIManaging.Common.Args.Views.LastLevelsPanelArgs
{
    public sealed class LocalUserVideoListLoader : LocalUserVideoListLoaderBase
    {
        private const int DRAFTS_TAKE_COUNT = 21;
        
        private readonly IPublishVideoHelper _publishVideoHelper;
        private readonly ILevelService _levelService;
        private BaseLevelItemArgs[] _draftLevelArgs;
        private bool _draftsLoaded;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        protected override int PrependDataIndex => _draftLevelArgs.Length == 0 ? 0 : 1;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public LocalUserVideoListLoader(IPublishVideoHelper publishVideoHelper, VideoManager videoManager,
            PageManager pageManager, IBridge bridge, ILevelService levelService)
            : base(videoManager, pageManager, bridge, bridge.Profile.GroupId)
        {
            _publishVideoHelper = publishVideoHelper;
            _levelService = levelService;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override async Task<Video[]> DownloadModelsInternal(object targetVideo, int takeNext, int takePrevious = 0, CancellationToken token = default)
        {
            if (!_draftsLoaded)
            {
                _draftsLoaded = true;
                await DownloadDraftModels();
                if (_draftLevelArgs.Length > 0)
                {
                    LevelPreviewArgs.Insert(0, new LevelDraftParentPreviewItemArgs(_draftLevelArgs[0].Level, true, OnDraftsPreviewClicked));
                    --takeNext;
                } 
            }

            var results = await VideoManager.GetVideosForLocalUser((long?)targetVideo, takeNext, takePrevious, token);
            return results.Video.ToArray();
        }
        
        protected override void OnVideoPreviewClicked(BaseLevelItemArgs args)
        {
            PageManager.MoveNext(PageId.Feed, new LocalUserFeedArgs(VideoManager, args.Video.Id));
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnDraftsPreviewClicked(BaseLevelItemArgs args)
        {
            PageManager.MoveNext(PageId.DraftsPage, new DraftsPageArgs(_draftLevelArgs));
        }
        
        private async Task DownloadDraftModels()
        {
            var result = await _levelService.GetLevelDraftsAsync(DRAFTS_TAKE_COUNT, 0);

            if (result.IsSuccess)
            {
                var drafts = result.Levels
                    .Select(level => new LevelDraftPreviewItemArgs(level, null));

                if (_publishVideoHelper.IsPublishing)
                {
                    drafts = drafts.Where(level => level.Level.Id != _publishVideoHelper.PublishedLevelId);
                }
                
                _draftLevelArgs = drafts.ToArray<BaseLevelItemArgs>();
            }
            else if (!result.IsCancelled)
            {
                Debug.LogError($"Failed to download level models. Reason: {result.ErrorMessage}");
            }
        }
    }
}