using Common.ProgressBars;
using Modules.InputHandling;
using TMPro;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using Zenject;

namespace UIManaging.PopupSystem.Popups
{
    internal sealed class ContentSavingPopup : BasePopup<ContentSavingPopupConfiguration>
    {
        [SerializeField] private TextMeshProUGUI _progressText;
        [SerializeField] private ProgressSimulator _progressSimulator;

        [Inject] private IBackButtonEventHandler _backButtonEventHandler;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        private void OnEnable()
        {
            _progressSimulator.ProgressUpdated += OnProgressUpdated;
        }

        private void OnDisable()
        {
            _progressSimulator.ProgressUpdated -= OnProgressUpdated;
        }

        //---------------------------------------------------------------------
        // PopupBase
        //---------------------------------------------------------------------
        protected override void OnConfigure(ContentSavingPopupConfiguration configuration)
        {
            IgnoreBackButtonPressing();            
            _progressSimulator.StartSimulation();
        }

        private void IgnoreBackButtonPressing()
        {
            _backButtonEventHandler.ProcessEvents(false);

            OnClose += OnClosed;
            
            void OnClosed(object obj)
            {
                OnClose -= OnClosed;
                
                _backButtonEventHandler.ProcessEvents(true);
            }
        }

        private void OnProgressUpdated(float progress) 
        {
            if (_progressText == null || Configs == null) return;
            _progressText.text = $"{Configs.ProgressMessage} {Mathf.FloorToInt(progress * 100)} %";
        }
    }
}
