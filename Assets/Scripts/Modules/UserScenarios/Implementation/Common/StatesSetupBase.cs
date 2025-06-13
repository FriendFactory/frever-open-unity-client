using System;
using Modules.UserScenarios.Common;
using Zenject;

namespace Modules.UserScenarios.Implementation.Common
{
    /// <summary>
    /// Prepared already setup for editor states and transitions most common scenario patterns/flows/paths
    /// </summary>
    internal abstract class StatesSetupBase
    {
        private IScenarioState[] _states;
        private readonly DiContainer _diContainer;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public IScenarioState[] States => _states ?? (_states = SetupStates());

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        protected StatesSetupBase(DiContainer diContainer)
        {
            _diContainer = diContainer;
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected abstract IScenarioState[] SetupStates();
        
        protected TTransition ResolveTransition<TTransition>() where TTransition : ITransition, IResolvable
        { 
            return _diContainer.Resolve<TTransition>();
        }

        protected TState ResolveState<TState>() where TState : IScenarioState, IResolvable
        {
            return _diContainer.Resolve<TState>();
        }
        
        protected TService ResolveService<TService>()
        {
            return _diContainer.Resolve<TService>();
        }
    }
}