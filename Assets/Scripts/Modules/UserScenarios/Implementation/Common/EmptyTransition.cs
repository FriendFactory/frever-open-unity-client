using System;
using System.Threading.Tasks;
using Modules.UserScenarios.Common;

namespace Modules.UserScenarios.Implementation.Common
{
    internal sealed class EmptyTransition : TransitionBase
    {
        private Func<Task> _onRunning;
        public override ScenarioState To { get; }
      
        public EmptyTransition(ScenarioState to)
        {
            To = to;
        }

        public EmptyTransition(IScenarioState to)
        {
            To = to.Type;
        }

        public void SetOnRunningBehaviour(Func<Task> onRunning)
        {
            _onRunning = onRunning;
        }
        
        public override async Task Run()
        {
            await RunSilently();
            OnFinished();
        }

        public override async Task RunSilently()
        {
            if (_onRunning != null)
            {
                await _onRunning();
            }
        }
    }
}