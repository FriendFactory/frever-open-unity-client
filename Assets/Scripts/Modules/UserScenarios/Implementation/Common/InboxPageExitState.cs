using System;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Navigation.Args;
using Navigation.Core;

namespace Modules.UserScenarios.Implementation.Common
{
    [UsedImplicitly]
    internal sealed class InboxPageExitState : IScenarioState, IResolvable
    {
        private readonly PageManager _pageManager;

        public ScenarioState Type => ScenarioState.InboxPageExit;
        public bool IsExitState => true;
        public ITransition[] Transitions => Array.Empty<ITransition>();
        
        public InboxPageExitState(PageManager pageManager)
        {
            _pageManager = pageManager;
        }

        public void Run()
        {
            var inboxPageArgs = new InboxPageArgs();
            _pageManager.MoveNext(inboxPageArgs);
        }
    }
}