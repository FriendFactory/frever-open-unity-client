using Common.ProgressBars;
using Extensions;
using TMPro;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;

namespace UIManaging.Common.UploadingPopups
{
    /// <summary>
    /// Displays the progress of video uploading and processing by backend, second part of video making progress
    /// </summary>
    internal sealed class VideoUploadingCountdownPopup : SlideInLeftPopup<VideoUploadingCountdownPopupConfiguration>
    {
        [SerializeField] private MonoBehaviour _progressSimulator;
        [SerializeField] private ProgressBar _counter;
        [SerializeField] private GameObject _loadingCircle;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public override bool NonBlockingQueue => true;
        
        //---------------------------------------------------------------------
        // BasePopup
        //---------------------------------------------------------------------

        protected override void OnConfigure(VideoUploadingCountdownPopupConfiguration configuration)
        {
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override void Show()
        {
            base.Show();

            ShowProgressBar(true);
            SetProgressValue(Configs.InitialProgressValue);
            EnableProgressSimulator(true);
        }

        public override void Hide(object result)
        {
            EnableProgressSimulator(false);
            SetProgressValue(1f);
            ShowProgressBar(false);
            SetDoneImageActiveState(true);

            StartCoroutine(DelayedHideRoutine());
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        private void EnableProgressSimulator(bool enable)
        {
            _progressSimulator.enabled = enable;
        }

        private void ShowProgressBar(bool show)
        {            
            _loadingCircle.SetActive(show);
        }

        private void SetProgressValue(float value)
        {
            _counter.Value = value;
        }
    }
}