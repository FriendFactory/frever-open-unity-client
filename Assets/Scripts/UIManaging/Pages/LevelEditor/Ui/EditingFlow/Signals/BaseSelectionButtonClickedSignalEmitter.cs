using JetBrains.Annotations;
using UIManaging.Common.Buttons;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.Signals
{
    internal abstract class BaseSelectionButtonClickedSignalEmitter<TProgressStepType, TSignal>: BaseButton
    {
        [SerializeField] private TProgressStepType _stepType;
        
        private SignalBus _signalBus;
        
        [Inject, UsedImplicitly]
        private void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        protected override void OnClickHandler()
        {
            _signalBus.Fire(GetSignal(_stepType));
        }
        
        protected abstract TSignal GetSignal(TProgressStepType stepType);
    }
}