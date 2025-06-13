using Models;
using Modules.UserScenarios.Common;
using UIManaging.Common.InputFields;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Common.Buttons
{
    [RequireComponent(typeof(Button))]
    internal sealed class LevelEditButton: LevelLoadButton
    {
        [Inject] private IScenarioManager _scenarioManager;
        
        protected override void LoadLevel(Level level)
        {
            var isTaskDraft = level.SchoolTaskId.HasValue && !level.RemixedFromLevelId.HasValue;
            if (isTaskDraft)
            {
                _scenarioManager.ExecuteTaskDraftEditing(level);
            }
            else
            {
                AdvancedInputFieldUtils.AddBindingsFromText(level.Description);
                _scenarioManager.ExecuteDraftEditing(level);
            }
        }
    }
}