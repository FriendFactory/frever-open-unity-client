using Common;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.LevelCreation;
using Navigation.Args;
using Navigation.Core;

namespace Modules.UserScenarios.Implementation.Common
{
    [UsedImplicitly]
    internal sealed class TasksExitState : ExitStateBase<ITasksExitContext>, IResolvable
    {
        private readonly ILevelManager _levelManager;
        private readonly PageManager _pageManager;

        public override ScenarioState Type => ScenarioState.TasksExit;
        
        public TasksExitState(ILevelManager levelManager, PageManager pageManager)
        {
            _levelManager = levelManager;
            _pageManager = pageManager;
        }

        public override void Run()
        {
            _levelManager.UnloadAllAssets();
            _pageManager.MoveNext(new TasksPageArgs(Context.Task, Constants.TasksPageTabs.IN_VOTING), false);
        }
    }
}