using System.Threading.Tasks;
using Modules.UserScenarios.Common;

namespace Modules.UserScenarios.Implementation.Common
{
    internal abstract class EntryTransitionBase<TContext> : TransitionBase<TContext>
        where TContext: class, IExitContext
    {
        protected readonly ScenarioArgsBase ScenarioArgs;

        protected EntryTransitionBase(ScenarioArgsBase scenarioArgs)
        {
            ScenarioArgs = scenarioArgs;
        }

        protected override Task UpdateContext()
        {
            Context.OpenedFromPage = ScenarioArgs.ExecutedFrom;
            return Task.CompletedTask;
        }
    }
}