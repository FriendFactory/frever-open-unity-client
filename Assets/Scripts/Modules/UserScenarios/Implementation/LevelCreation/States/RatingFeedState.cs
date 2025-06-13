using Extensions;
using Modules.Amplitude;
using JetBrains.Annotations;
using Models;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.UserScenarios.Common;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Pages.PublishPage;

namespace Modules.UserScenarios.Implementation.LevelCreation.States
{
    [UsedImplicitly]
    internal sealed class RatingFeedState: StateBase<ILevelCreationScenarioContext>, IResolvable
    {
        private readonly PageManager _pageManager;
        private readonly ILevelManager _levelManager;
        private readonly AmplitudeManager _amplitudeManager;
        private readonly IPublishVideoPopupManager _videoPublishingPopupManager;
        
        public override ScenarioState Type => ScenarioState.RatingFeed;
        
        public override ITransition[] Transitions => new[] { MoveNext, SkipRating}.RemoveNulls();
        
        public ITransition MoveNext;
        public ITransition SkipRating;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public RatingFeedState(PageManager pageManager, ILevelManager levelManager, AmplitudeManager amplitudeManager,
            IPublishVideoPopupManager videoPublishingPopupManager)
        {
            _pageManager = pageManager;
            _levelManager = levelManager;
            _amplitudeManager = amplitudeManager;
            _videoPublishingPopupManager = videoPublishingPopupManager;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public override void Run()
        {
            _levelManager.UnloadAllAssets();
            _pageManager.MoveNext(GetRatingFeedArgs(Context.LevelData));
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private RatingFeedPageArgs GetRatingFeedArgs(Level levelData)
        {
            return new RatingFeedPageArgs()
            {
                LevelData = levelData,
                MoveNextRequested = OnMoveNextRequested,
                SkipRatingRequested = OnSkipRatingRequested
            };
        }

        private async void OnMoveNextRequested()
        {
            await MoveNext.Run();
        }
        
        private async void OnSkipRatingRequested()
        {
            await SkipRating.Run();
        }
    }
}