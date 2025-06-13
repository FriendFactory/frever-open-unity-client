using BrunoMikoski.AnimationSequencer;
using JetBrains.Annotations;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking.Steps;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.TemplateSetup
{
    internal sealed class TemplateSetupProgressTrackerEventHandler: MonoBehaviour
    {
        [SerializeField] private AssetSelectionProgressStepType _stepType;
        [SerializeField] private GameObject _checkmark;
        [SerializeField] private AnimationSequencerController _animationSequencer;
        
        private TemplateSetupProgressTracker _progressTracker;
        
        public AssetSelectionProgressStepType StepType => _stepType;
        
        [Inject, UsedImplicitly]
        private void Construct(TemplateSetupProgressTracker progressTracker)
        {
            _progressTracker = progressTracker;
        }

        private void OnEnable()
        {
            var isStepFinished = _progressTracker.IsStepCompleted(_stepType);
            _checkmark.SetActive(isStepFinished);
            _animationSequencer.SetTime(isStepFinished? int.MaxValue : 0, false);
            
            _progressTracker.ProgressChanged += OnProgressChanged;
        }

        private void OnDisable()
        {
            _progressTracker.ProgressChanged -= OnProgressChanged;
        }
        
        private void OnProgressChanged(IProgressStep step)
        {
            if (step is SelectAssetProgressStep selectAssetStep && selectAssetStep.StepType != _stepType) return;
            
            _checkmark.SetActive(step.IsCompleted);
            
            if (step.IsCompleted)
            {
                _animationSequencer.PlayForward();
            }
            else
            {
                _animationSequencer.PlayBackwards();
            }
        }
    }
}