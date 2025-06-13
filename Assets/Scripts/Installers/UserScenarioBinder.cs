using System;
using System.Collections.Generic;
using System.Linq;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.Common;
using Modules.UserScenarios.Implementation.LevelCreation;
using Modules.UserScenarios.Implementation.LevelCreation.Helpers;
using Modules.UserScenarios.Implementation.LevelCreation.Scenarios;
using Modules.UserScenarios.Implementation.LevelCreation.States;
using Modules.UserScenarios.Implementation.LevelCreation.Transitions;
using Zenject;

namespace Installers
{
    internal static class UserScenarioBinder
    {
        private static readonly Type SCENARIO_BASE_TYPE = typeof(IScenario);
        private static IEnumerable<Type> ScenarioTypes => typeof(CreateNewLevelScenario).Assembly.GetTypes()
           .Where(x => x.IsClass && !x.IsAbstract && SCENARIO_BASE_TYPE.IsAssignableFrom(x));
        
        public static void BindUserScenarioManaging(this DiContainer container)
        {
            container.BindInterfacesAndSelfTo<ScenarioManager>().AsSingle();
            container.BindInterfacesAndSelfTo<StateMachine>().AsSingle();
            
            foreach (var scenarioType in ScenarioTypes)
            {
                container.BindInterfacesAndSelfTo(scenarioType).AsSingle();
            }
            
            container.BindScenarioStates();
            container.BindScenarioTransitions();
            container.BindStateSetups();
            container.BindHelpers();
        }
    }
    
    internal static class ScenarioStatesBinder
    {
        private static readonly Type STATE_BASE_TYPE = typeof(IScenarioState);
        private static readonly Type RESOLVABLE_TYPE = typeof(IResolvable);
        private static IEnumerable<Type> StateTypes => typeof(LevelEditorState).Assembly.GetTypes()
           .Where(x => x.IsClass && !x.IsAbstract && STATE_BASE_TYPE.IsAssignableFrom(x) && RESOLVABLE_TYPE.IsAssignableFrom(x));

        public static void BindScenarioStates(this DiContainer container)
        {
            foreach (var stateType in StateTypes)
            {
                container.BindInterfacesAndSelfTo(stateType).AsTransient();
            }
        }
    }
    
    internal static class ScenarioTransitionsBinder
    {
        private static readonly Type TRANSITION_BASE_TYPE = typeof(ITransition);
        private static readonly Type RESOLVABLE_TYPE = typeof(IResolvable);
        private static IEnumerable<Type> TransitionTypes => typeof(EmptyTransition).Assembly.GetTypes()
           .Where(x => x.IsClass && !x.IsAbstract && TRANSITION_BASE_TYPE.IsAssignableFrom(x) && RESOLVABLE_TYPE.IsAssignableFrom(x));

        public static void BindScenarioTransitions(this DiContainer container)
        {
            foreach (var stateType in TransitionTypes)
            {
                container.BindInterfacesAndSelfTo(stateType).AsTransient();
            }
        }
    }

    internal static class ScenarioStateSetupsBinder
    {
        private static readonly Type TRANSITION_BASE_TYPE = typeof(StatesSetupBase);
        private static IEnumerable<Type> TransitionTypes => typeof(EmptyTransition).Assembly.GetTypes()
           .Where(x => x.IsClass && !x.IsAbstract && TRANSITION_BASE_TYPE.IsAssignableFrom(x));

        public static void BindStateSetups(this DiContainer container)
        {
            container.BindInterfacesAndSelfTo<LevelCreationStatesSetupProvider>().AsSingle();
            foreach (var stateType in TransitionTypes)
            {
                container.BindInterfacesAndSelfTo(stateType).AsTransient();
            }
        }
    }

    internal static class ScenarioHelpersBinder
    {
        public static void BindHelpers(this DiContainer container)
        {
            container.BindInterfacesAndSelfTo<AppCacheForceResetHelper>().AsSingle();
            container.BindInterfacesAndSelfTo<ExitToChatPageStateHelper>().AsSingle();
        }
    }
}