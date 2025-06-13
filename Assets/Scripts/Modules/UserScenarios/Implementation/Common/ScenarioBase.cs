using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Extensions;
using Modules.UserScenarios.Common;
using Zenject;

namespace Modules.UserScenarios.Implementation.Common
{
    internal abstract class ScenarioBase<TContext> : IScenario
    {
        protected readonly DiContainer DiContainer;

        public ITransition Entry { get; private set; }
        public IScenarioState[] States { get; private set; }
        public ITransition[] Transitions => States.SelectMany(x => x.Transitions).Distinct().Append(Entry).ToArray();

        protected TContext Context { get; private set; }

        protected ScenarioBase(DiContainer diContainer)
        {
            DiContainer = diContainer;
        }

        public async Task Setup()
        {
            await BeforeSetup();
            
            Context = await SetupContext();
            Entry = SetupEntryTransition();
            States = await SetupStates();
            
            SetupContext(States);
            SetupContext(Transitions);
        }
        
        public virtual void OnExit()
        {
        }

        protected virtual Task BeforeSetup()
        {
            return Task.CompletedTask;
        }
        
        protected abstract ITransition SetupEntryTransition();
        protected abstract Task<TContext> SetupContext();
        protected abstract Task<IScenarioState[]> SetupStates();

        protected TTransition ResolveTransition<TTransition>() where TTransition : ITransition, IResolvable
        {
            return DiContainer.Resolve<TTransition>();
        }

        protected TState ResolveState<TState>() where TState : IScenarioState, IResolvable
        {
            return DiContainer.Resolve<TState>();
        }

        protected TService Resolve<TService>()
        {
            return DiContainer.Resolve<TService>();
        }
        
        private void SetupContext<T>(ICollection<T> targets)
        {
            var contextDependantGenericType = typeof(IContextDependant<>);
            var contextDependant =
                targets.Where(x => x.GetType().IsAssignableToGenericType(contextDependantGenericType));

            foreach (var target in contextDependant)
            {
               var contextDependantInterface = target.GetType().GetInterfaces()
                      .First(x => x.IsGenericType && x.GetGenericTypeDefinition() == contextDependantGenericType);
               var requiredContextType = contextDependantInterface.GetGenericArguments().First();
               if (requiredContextType.IsInstanceOfType(Context))
               {
                   var setupMethod = contextDependantInterface.GetMethod(nameof(IContextDependant<TContext>.SetContext));
                   setupMethod.Invoke(target, new object[] { Context });
               }
               else
               {
                   throw new InvalidOperationException($"Can't resolve context for {target.GetType().Name}");
               }
            }
        }
    }

    internal abstract class ScenarioBase<TScenarioArgs, TContext> : ScenarioBase<TContext>, IScenario<TScenarioArgs> where TScenarioArgs: IScenarioArgs
    {
        protected TScenarioArgs InitialArgs { get; private set; }

        protected ScenarioBase(DiContainer diContainer) : base(diContainer)
        {
        }

        public void SetArgs(TScenarioArgs args)
        {
            InitialArgs = args;
        }
    }
}