using System.Threading.Tasks;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.UserScenarios.Common;

namespace Modules.UserScenarios.Implementation.LevelCreation.Transitions
{
    [UsedImplicitly]
    internal sealed class ExitFromSaveToDrafts: TransitionBase<ILevelCreationScenarioContext>, IResolvable
    {
        private readonly ILevelManager _levelManager;

        public override ScenarioState To
        {
            get
            {
                if (Context.OpenedFromChat != null) return ScenarioState.ChatPageExit;
                return ScenarioState.ProfileExit;
            }
        }

        public ExitFromSaveToDrafts(ILevelManager levelManager)
        {
            _levelManager = levelManager;
        }

        protected override Task UpdateContext()
        {
            return Task.CompletedTask;
        }

        protected override Task OnRunning()
        {
            _levelManager.UnloadAllAssets();
            return base.OnRunning();
        }
    }
}