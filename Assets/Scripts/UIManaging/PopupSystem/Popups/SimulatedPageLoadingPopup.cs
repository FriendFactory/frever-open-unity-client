using System.Collections;
using Common.ProgressBars;
using Extensions;
using UIManaging.Animated.Behaviours;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups
{
    internal sealed class SimulatedPageLoadingPopup : BasePageLoadingPopup<SimulatedPageLoadingPopupConfiguration>
    {
        private const float DEFAULT_TIMEOUT = 30f;
        
        [SerializeField] private AnimatedLoadingBarGradientBehaviour _animatedLoadingBarGradient;
        [SerializeField] private ProgressSimulator _progressSimulator;
        
        private Coroutine _timeoutCoroutine;

        protected override void OnFadeInAnimationStarted()
        {
            _progressSimulator.ProgressUpdated += UpdateProgressBar;
            _progressSimulator.StartSimulation();
            
            _timeoutCoroutine = StartCoroutine(ShowTimeoutPopup(DEFAULT_TIMEOUT));
        }

        protected override void CleanUp()
        {
            _progressSimulator.ProgressUpdated -= UpdateProgressBar;
            _progressSimulator.StopSimulation();
            
            if (_timeoutCoroutine != null) StopCoroutine(_timeoutCoroutine);
        }
        

        private IEnumerator ShowTimeoutPopup(float timeOut)
        {
            yield return new WaitForSecondsRealtime(timeOut);

            var timeoutPopupConfiguration = new InformationPopupConfiguration()
            {
                PopupType = PopupType.SlowConnection
            };

            PopupManager.SetupPopup(timeoutPopupConfiguration);
            PopupManager.ShowPopup(timeoutPopupConfiguration.PopupType);
        
            CancelButton.SetActive(true);           
        }
    }
}