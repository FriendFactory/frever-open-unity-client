using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Navigation.Args;
using Navigation.Core;

namespace Modules.UserScenarios.Implementation.Common
{
    [UsedImplicitly]
    internal sealed class CrewPageExitState : ExitStateBase, IResolvable
    {
        private readonly PageManager _pageManager;

        public CrewPageExitState(PageManager pageManager)
        {
            _pageManager = pageManager;
        }
        
        public override ScenarioState Type => ScenarioState.CrewPageExit;
        
        public override void Run()
        {
            var inboxPageArgs = new CrewPageArgs();
            _pageManager.MoveNext(inboxPageArgs);
        }
    }
}