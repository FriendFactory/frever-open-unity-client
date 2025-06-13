using Common;
using Modules.Amplitude;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.UserScenarios.Common;
using Navigation.Args.Feed;
using Navigation.Core;
using UIManaging.Pages.Common.VideoManagement;

namespace Modules.UserScenarios.Implementation.Common
{
    [UsedImplicitly]
    internal sealed class FeedExitState : ExitStateBase<IExitContext>, IResolvable
    {
        private static bool IsForMe => AmplitudeManager.IsForMeFeatureEnabled();
        
        private readonly ILevelManager _levelManager;
        private readonly VideoManager _videoManager;
        private readonly PageManager _pageManager;
        
        public override ScenarioState Type => ScenarioState.FeedExit;

        public FeedExitState(ILevelManager levelManager, VideoManager videoManager, PageManager pageManager)
        {
            _levelManager = levelManager;
            _videoManager = videoManager;
            _pageManager = pageManager;
        }

        public override void Run()
        {
            _levelManager.UnloadAllAssets();
            _pageManager.MoveNext(GetFeedArgs(), false);
        }
        
        private GeneralFeedArgs GetFeedArgs()
        {
            var lastFeedArgs = TryGetLastOpenedArgs();
            var type = IsForMe ? VideoListType.ForMe : VideoListType.New;
            return lastFeedArgs ?? new GeneralFeedArgs(_videoManager) { VideoListType = type };
        }

        private GeneralFeedArgs TryGetLastOpenedArgs()
        {
            if (!_pageManager.HistoryContains(PageId.Feed))
            {
                return null;
            }
            
            var lastFeedArgs = _pageManager.GetLastArgsForPage(PageId.Feed) as GeneralFeedArgs;
            var type = IsForMe ? VideoListType.ForMe : VideoListType.New;
            
            return lastFeedArgs == null || lastFeedArgs.VideoListType != type ? null : lastFeedArgs;
        }
    }
}