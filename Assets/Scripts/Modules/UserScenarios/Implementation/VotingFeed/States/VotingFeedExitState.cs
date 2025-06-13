using System.Linq;
using Common;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.UserScenarios.Common;
using Navigation.Core;
using UIManaging.Pages.Common.VideoManagement;

namespace Modules.UserScenarios.Implementation.VotingFeed.States
{
    [UsedImplicitly]
    internal sealed class VotingFeedExitState : ExitStateBase<VotingFeedContext>, IResolvable
    {
        private readonly ILevelManager _levelManager;
        private readonly PageManager _pageManager;

        public override ScenarioState Type => ScenarioState.PreviousPageExit;
    
        public VotingFeedExitState(ILevelManager levelManager, PageManager pageManager)
        {
            _levelManager = levelManager;
            _pageManager = pageManager;
        }

        public override void Run()
        {
            if (Context.AllBattleData != null)
            {
                foreach (var battleData in Context.AllBattleData
                            .SelectMany(battle => battle.BattleVideos.Select(battleVideo => battleVideo.VideoModel)))
                {
                    battleData.CleanUp();
                }
            }
            
            _levelManager.UnloadAllAssets();

            if (_pageManager.CurrentPage.Id == Context.OpenedFromPage) 
            {
                return; //do not reload the page
            }
        
            _pageManager.MoveBackTo(Context.OpenedFromPage);
        }
    }
}