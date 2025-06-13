using System.Collections.Generic;
using System.Linq;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking.Steps;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.Outfits;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.DressUp
{
    internal sealed class DressUpPointingArrowController: BasePointingArrowController<DressUpStepProgressTracker, SelectWardrobeProgressStep>
    {
        [SerializeField] private List<CreateNewOutfitButton> _wardrobeButtons;
        
        protected override Transform GetNextTransform(SelectWardrobeProgressStep step)
        {
            return _wardrobeButtons.FirstOrDefault(button => button.ProgressStepType == step.StepType)?.transform;
        }
    }
}