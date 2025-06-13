using System.Collections.Generic;
using System.Linq;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.DressUp;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking.Steps;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.TemplateSetup
{
    internal sealed class TemplateSetupPointingArrowController: BasePointingArrowController<TemplateSetupProgressTracker, SelectAssetProgressStep>
    {
        [SerializeField] private List<TemplateSetupProgressTrackerEventHandler> _eventHandlers;
        
        protected override Transform GetNextTransform(SelectAssetProgressStep step)
        {
            return _eventHandlers.FirstOrDefault(eventHandler => eventHandler.StepType == step.StepType)?.transform;
        }
    }
}