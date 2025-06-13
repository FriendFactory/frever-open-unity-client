using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Modules.UserScenarios.Common
{
    [UsedImplicitly]
    internal sealed class StateMachine
    {
        internal event Action<IScenarioState> StateRun;
        
        private IScenario _scenario;

        private ITransition[] Transitions => _scenario.Transitions;
        private IScenarioState[] States => _scenario.States;

        public async void Run(IScenario scenario)
        {
            _scenario = scenario;
            await scenario.Setup();
            Init();
            ExecuteEntryTransition();
        }

        private void Init()
        {
            foreach (var transition in Transitions)
            {
                transition.Finished += RunState;
            }
        }
        
        private void ExecuteEntryTransition()
        {
            _scenario.Entry.Run();
        }

        private void RunState(ITransition transition)
        {
            var nextStateType = transition.To;
            var nextState = States.FirstOrDefault(x => x.Type == nextStateType);
            if (nextState == null)
            {
                Debug.LogError($"The scenario {_scenario.GetType()} has missed state {transition.To}");
                return;
            }
            nextState.Run();
            StateRun?.Invoke(nextState);
            if (nextState.IsExitState)
            {
                _scenario.OnExit();
            }
        }
    }
}