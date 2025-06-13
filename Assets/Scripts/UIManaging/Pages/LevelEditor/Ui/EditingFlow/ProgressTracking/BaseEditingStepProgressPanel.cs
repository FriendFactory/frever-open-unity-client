using Common.Abstract;
using Extensions;
using JetBrains.Annotations;
using TMPro;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking.Steps;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking
{
    internal abstract class BaseEditingStepProgressPanel<TProgressStep> : BaseContextPanel<IEditingFlowStepProgress<TProgressStep>> where TProgressStep : IProgressStep
    {
        [SerializeField] private TMP_Text _stepDescription;
        [SerializeField] private GameObject _nextButtonGroup;
        
        protected EditingFlowLocalization _localization;

        [Inject, UsedImplicitly]
        private void Construct(EditingFlowLocalization localization)
        {
            _localization = localization;
        }

        protected override void OnInitialized()
        {
            ContextData.ProgressChanged += OnStepCompleted;
            
            Toggle(false);
            
            OnStepCompleted();
        }

        protected override void BeforeCleanUp()
        {
            ContextData.ProgressChanged -= OnStepCompleted;
        }
        
        private void OnStepCompleted()
        {
            var stepDescription = GetStepDescription();
            _stepDescription.text = stepDescription;

            if (ContextData.StepsCount != ContextData.CompletedStepsCount) return;
            
            Toggle(true);
            
            ContextData.ProgressChanged -= OnStepCompleted;
        }

        protected abstract string GetStepDescription();
        
        private void Toggle(bool isCompleted)
        {
            _stepDescription.SetActive(!isCompleted);
            _nextButtonGroup.SetActive(isCompleted);
        }
    }
}