using Modules.Amplitude.Events.Core;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking.Steps;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.Amplitude
{
    internal abstract class BaseSelectionButtonClickedAmplitudeEvent<TProgressStepType> : BaseAmplitudeEvent
    {
        private readonly TProgressStepType _stepType;
        
        public override string Name => GetName(_stepType);
        

        protected BaseSelectionButtonClickedAmplitudeEvent(TProgressStepType stepType)
        {
            _stepType = stepType;
        }
        
        protected abstract string GetName(TProgressStepType stepType);
    }
}