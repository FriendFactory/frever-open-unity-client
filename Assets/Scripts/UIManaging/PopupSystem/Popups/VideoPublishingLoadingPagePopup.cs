using System.Threading.Tasks;
using Common.ProgressBars;
using Extensions;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.LevelViewPort;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.PopupSystem.Popups
{
    internal sealed class VideoPublishingLoadingPagePopup : BasePageLoadingPopup<VideoPublishingLoadingPagePopupConfiguration>
    {
        [SerializeField] private ProgressSimulator _progressSimulator;
        [Header("Preview")]
        [SerializeField] private LevelViewPort _levelView;
        [SerializeField] private AspectRatioFitter _levelViewContainer;
        [SerializeField] private float _defaultRatio = 0.5625f;

        [Inject] private ILevelManager _levelManager;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private bool CanTriggerCancellation => Configs.VideoRenderingState.IsRendering;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected override void OnEnable()
        {
            base.OnEnable();
            _levelViewContainer.aspectRatio = _defaultRatio;
            CancelButton.SetActive(true);
        }

        protected override void OnDisable()
        {
            _levelManager.PlayingEventSwitched -= UpdateRenderTexture;
            _levelView.SetActive(false);
            base.OnDisable();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override void Show()
        {
            base.Show();

            if (_levelManager == null)
            {
                Debug.LogError("Level manager is null");
                return;
            }

            _levelManager.PlayingEventSwitched -= UpdateRenderTexture;
            _levelManager.PlayingEventSwitched += UpdateRenderTexture;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnFadeInAnimationStarted()
        {
            Configs.OnProgressUpdated += UpdateProgressBar;
        }

        protected override void CleanUp()
        {
            if (!_progressSimulator.enabled)
            {
                Configs.OnProgressUpdated -= UpdateProgressBar;
                return;
            }

            _progressSimulator.ProgressUpdated -= UpdateProgressBar;
        }

        protected override void UpdateProgressBar(float value)
        {
            var initial = Configs.InitialValue;
            var max = Configs.MaxValue;
            var calculatedValue = Mathf.Clamp(initial + value * max, 0.0f, initial + max);
            _progressBar.Value = calculatedValue;
        }

        protected override async void RequestCancelAction()
        {
            if (!CanTriggerCancellation)
            {
                CancelButton.SetActive(false);
                await WaitWhileCanBePossibleToCancelLoading();
            }

            base.RequestCancelAction();
        }

        private async Task WaitWhileCanBePossibleToCancelLoading()
        {
            const int delay = 50;
            var timeout = 15000f;

            while (timeout > 0 && !CanTriggerCancellation)
            {
                await Task.Delay(delay);
                timeout -= delay;
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void UpdateRenderTexture()
        {
            var activeCamera = _levelManager.GetActiveCamera();
            if (activeCamera == null)
            {
                Debug.LogError("Active camera is null");
                return;
            }

            if (activeCamera.targetTexture == _levelView.RenderTexture) return;

            activeCamera.targetTexture = _levelView.RenderTexture;
            activeCamera.ApplyAspectRatioFromRenderTextureImmediate();
        }
    }
}