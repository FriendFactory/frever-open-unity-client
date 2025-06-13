using System.Threading.Tasks;
using Bridge;
using Modules.Amplitude;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.UserScenarios.Common;
using UIManaging.Pages.PublishPage;
using UnityEngine;

namespace Modules.UserScenarios.Implementation.LevelCreation.Transitions
{
    [UsedImplicitly]
    internal sealed class ExitTaskScenarioFromPublish : ExitScenarioFromPublishBase
    {
        public ExitTaskScenarioFromPublish(ILevelManager levelManager, PublishVideoHelper publishHelper, IPublishVideoPopupManager popupManager, AmplitudeManager amplitudeManager, IBridge bridge) : base(levelManager, publishHelper, popupManager, amplitudeManager, bridge)
        {
        }

        protected override async Task<bool> SetNextState()
        {
            var taskId = Context.TaskId ?? 0;
            var result = await Bridge.GetTaskFullInfoAsync(taskId);
            if (result.IsSuccess)
            {
                Context.Task = result.Model;
                DestinationState = ScenarioState.TasksExit;
                return true;
            }
            
            if (result.IsError)
            {
                Debug.LogWarning($"Couldn't retrieve information about task {taskId}");
                DestinationState = ScenarioState.PreviousPageExit;
                
            }
            return false;
        }
    }
}