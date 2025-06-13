using Common;
using Extensions;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Navigation.Args;
using Navigation.Core;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;

namespace Modules.UserScenarios.Implementation.VotingFeed.States
{
    [UsedImplicitly]
    internal sealed class VotingFeedState : StateBase<VotingFeedContext>, IResolvable
    {
        private readonly PageManager _pageManager;
        private readonly PopupManager _popupManager;
        
        public ITransition MoveNext;
        public ITransition MoveBack;
        public override ScenarioState Type => ScenarioState.VotingFeed;
        public override ITransition[] Transitions => new[] { MoveNext, MoveBack }.RemoveNulls();

        public VotingFeedState(PageManager pageManager, PopupManager popupManager)
        {
            _pageManager = pageManager;
            _popupManager = popupManager;
        }

        public override void Run()
        {
            var args = new VotingFeedPageArgs
            {
                AllBattleData = Context.AllBattleData,
                TaskId = Context.Task.Id,
                BattleName = Context.Task.Name,
                ShowHintsOnDisplay = false,
                MoveNext = OnMoveNext,
                MoveBack = OnExitRequested
            };
            
            _pageManager.MoveNext(args);
        }

        private async void OnMoveNext()
        {
            await MoveNext.Run();
        }

        private void OnExitRequested()
        {
            var config = new ExitVotingPopupConfiguration
            {
                ExitConfirmed = OnMoveBack
            };
            _popupManager.SetupPopup(config);    
            _popupManager.ShowPopup(config.PopupType);
        }
        
        private async void OnMoveBack()
        {
            await MoveBack.Run();
        }
    }
}