using System;
using System.Linq;
using System.Threading.Tasks;

namespace Modules.UserScenarios.Common
{
    internal interface ITransition
    {
        event Action<ITransition> Finished;

        ScenarioState To { get; }
        Task Run();
        
        /// <summary>
        /// Without notifying state machine.
        /// Necessary when we use the transition as a part of another transition
        /// </summary>
        Task RunSilently();
    }

    internal abstract class TransitionBase: ITransition
    {
        public event Action<ITransition> Finished;
        public abstract ScenarioState To { get; }

        public abstract Task Run();
        public abstract Task RunSilently();

        protected void OnFinished()
        {
            Finished?.Invoke(this);
        }
    }

    internal abstract class TransitionBase<TContext> : TransitionBase, IContextDependant<TContext>
    {
        protected TContext Context { get; private set; }

        public virtual void SetContext(TContext context)
        {
            Context = context;
        }

        public sealed override async Task Run()
        {
            await RunSilently();
            OnFinished();
        }

        public sealed override async Task RunSilently()
        {
            await UpdateContext();
            await OnRunning();
        }

        protected abstract Task UpdateContext();

        protected virtual Task OnRunning()
        {
            return Task.CompletedTask;
        }
    }

    internal abstract class SwitchTransitionBase<TContext> : TransitionBase<TContext>
    {
        private readonly ITransition[] _transitions;
        
        protected ScenarioState Destination;
        public override ScenarioState To => Destination;
        protected ITransition TargetTransition => _transitions.First(x => x.To == To);

        protected SwitchTransitionBase(ITransition[] subTransitions)
        {
            _transitions = subTransitions;
        }
        
        public override void SetContext(TContext context)
        {
            base.SetContext(context);
            foreach (var transition in _transitions.Where(x=>x is IContextDependant<TContext>).Cast<IContextDependant<TContext>>())
            {
                transition.SetContext(context);
            }
        }
        
        protected override Task OnRunning()
        {
            return TargetTransition.RunSilently();
        }
    }
}