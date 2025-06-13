using System;
using System.Collections.Generic;
using Common;
using Extensions;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Navigation.Core;
using TipsManagment;
using UIManaging.Pages.VotingFeed;

namespace Modules.UserScenarios.Implementation.VotingFeed.States
{
    [UsedImplicitly]
    internal sealed class StyleBattleStartState : StateBase<VotingFeedContext>, IResolvable
    {
        private readonly PageManager _pageManager;
        
        public ITransition MoveNext;
        public ITransition MoveBack;
        public override ScenarioState Type => ScenarioState.StyleBattleStart;
        public override ITransition[] Transitions => new[] { MoveNext, MoveBack }.RemoveNulls();

        public StyleBattleStartState(PageManager pageManager)
        {
            _pageManager = pageManager;
        }

        public override void Run()
        {
            var args = new StyleBattleStartPageArgs
            {
                Task = Context.Task,
                DressCodes = new List<string>(),
                MoveNext = OnMoveNext,
                MoveBack = OnMoveBack
            };
            
            _pageManager.MoveNext(args);
        }

        private async void OnMoveNext()
        {
            await MoveNext.Run();
        }

        private async void OnMoveBack()
        {
            await MoveBack.Run();
        }
    }
}