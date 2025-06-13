using System;
using System.Collections;
using System.Linq;
using Common.Abstract;
using Extensions;
using JetBrains.Annotations;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking.Steps;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.DressUp
{
    internal abstract class BasePointingArrowController<TProgressTracker, TProgressStep> : BaseContextlessPanel
        where TProgressTracker : BaseEditingStepProgressTracker<TProgressStep> where TProgressStep : IProgressStep
    {
        [SerializeField] private PointingArrow _pointingArrow;
        
        private TProgressTracker _progressTracker;

        [Inject, UsedImplicitly]
        private void Construct(TProgressTracker progressTracker)
        {
            _progressTracker = progressTracker;
        }
        
        public void MoveToNextAvailablePosition() => StartCoroutine(MoveToNextPositionRoutine());

        protected override void OnInitialized()
        {
            _progressTracker.ProgressChanged += MoveNext;
        }

        protected override void BeforeCleanUp()
        {
            _progressTracker.ProgressChanged -= MoveNext;
        }
        
        private void MoveNext(IProgressStep _) => MoveToNextPosition();

        private IEnumerator MoveToNextPositionRoutine()
        {
            // need to wait a bit until layout group will be updated
            yield return new WaitForEndOfFrame();
            
            MoveToNextPosition();
        }

        private void MoveToNextPosition()
        {
            _pointingArrow.SetActive(true);
            
            var next = _progressTracker.Steps.FirstOrDefault(step => !step.IsCompleted);
            
            if (next == null)
            {
                _pointingArrow.SetActive(false);
                return;
            }

            var nextButton = GetNextTransform(next);
            
            if (nextButton == null)
            {
                _pointingArrow.SetActive(false);
                return;
            }
            
            var x = nextButton.transform.position.x;
            
            _pointingArrow.transform.SetPositionX(x);
        }
        
        protected abstract Transform GetNextTransform(TProgressStep step);
    }
}