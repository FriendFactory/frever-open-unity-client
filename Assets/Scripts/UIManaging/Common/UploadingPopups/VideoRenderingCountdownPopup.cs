using System;
using System.Threading.Tasks;
using Common.ProgressBars;
using Extensions;
using Modules.InputHandling;
using TMPro;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Common.UploadingPopups
{
    /// <summary>
    /// Displays the progress of video rendering, first half of video making progress
    /// </summary>
    internal sealed class VideoRenderingCountdownPopup: BasePopup<VideoRenderingCountdownPopupConfiguration>
    {
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private ProgressBar _progressBar;
        [SerializeField] private TextMeshProUGUI _counter;
        [SerializeField] private ProgressSimulator _progressSimulator;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private GameObject _cancelWaitingIndicator;
        
        [Inject] private IBackButtonEventHandler _backButtonEventHandler;
        
        private float _progressWhenRenderingBegan;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        private IVideoRenderingState VideoRenderingState => Configs.VideoRenderingState;
        private float ProgressBarPieceForVideoCapturing => Configs.FillProgressBarForVideoCapturing;
        private string StartCaptureTitle => Configs.StartCaptureTitle;
        private string VideoRenderingTitle => Configs.RenderingVideoTitle;
        private Action OnCancelClicked => Configs.OnCancelClicked;
        private bool CanTriggerCancellation => VideoRenderingState.IsRendering;
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override void Show()
        {
            base.Show();
            VideoRenderingState.ProgressUpdated += OnVideoRenderingBegan;
            _title.text = StartCaptureTitle;
            _cancelButton.SetActive(true);
            _cancelButton.onClick.AddListener(OnCancelButtonClicked);
            _cancelWaitingIndicator.SetActive(false);
            SetProgressValue(0);
        }

        public override void Hide()
        {
            Cleanup();
            base.Hide();
        }

        public override void Hide(object result)
        {
            Cleanup();
            base.Hide(result);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnConfigure(VideoRenderingCountdownPopupConfiguration configuration)
        {
            IgnoreBackButtonPressing();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void UpdateProgressBar(float renderProgress)
        {
            var reservedForRendering = ProgressBarPieceForVideoCapturing - _progressWhenRenderingBegan;
            var progressBarValue = _progressWhenRenderingBegan + renderProgress * reservedForRendering;
            SetProgressValue(progressBarValue);
        }
        
        private void SetProgressValue(float value)
        {
            _counter.text = $"{(value * 100).ToString("F0")}%";
        }
        
        private void Cleanup()
        {
            VideoRenderingState.ProgressUpdated -= UpdateProgressBar;
            VideoRenderingState.ProgressUpdated -= OnVideoRenderingBegan;
            _cancelButton.onClick.RemoveListener(OnCancelButtonClicked);
        }

        private void OnVideoRenderingBegan(float progress)
        {
            VideoRenderingState.ProgressUpdated -= OnVideoRenderingBegan;
            VideoRenderingState.ProgressUpdated += UpdateProgressBar;
            _title.text = VideoRenderingTitle;
        }

        private async void OnCancelButtonClicked()
        {
            //workaround for cancelling video capture
            //while we don't have ability to cancel assets loading, we have to trigger that only during level preview/capturing
            if (!CanTriggerCancellation)
            {
                _cancelWaitingIndicator.SetActive(true);
                _cancelButton.SetActive(false);
                await WaitWhileCanBePossibleToCancelLoading();
                _cancelWaitingIndicator.SetActive(false);
            }

            OnCancelClicked?.Invoke();
        }

        private async Task WaitWhileCanBePossibleToCancelLoading()
        {
            while (!CanTriggerCancellation)
            {
                await Task.Delay(33);
            }
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
    }
}