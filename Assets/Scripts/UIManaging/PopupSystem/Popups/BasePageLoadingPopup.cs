using System;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.ProgressBars;
using Extensions;
using Models;
using Sirenix.OdinInspector;
using TMPro;
using UIManaging.Animated.Behaviours;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups.Views;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.PopupSystem.Popups
{
    public abstract class BasePageLoadingPopup<TConfig> : BasePopup<TConfig>
        where TConfig : BasePageLoadingPopupConfiguration
    {
        private const int TIPS_SWAP_INTERVAL = 3;

        [SerializeField] private TMP_Text _headerText;
        [SerializeField] private TMP_Text _tipsText;
        [SerializeField] private FadeInOutBehaviour _tipsFadeInOut;
        [SerializeField] protected ProgressBar _progressBar;
        [SerializeField] private TMP_Text _progressBarText;

        [FormerlySerializedAs("_cancelButton")] [SerializeField]
        protected Button CancelButton;

        [Space]
        [SerializeField] private RandomBackground _randomBackground;

        [SerializeField] protected FadeInOutBehaviour _fadeInOut;
        [SerializeField] private SlideInOutBehaviour _slideIn;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Space] [SerializeField] private TipsBank _tipsBank;

        [Inject] protected PopupManager PopupManager;
        [Inject] private IExceptionCatcher _exceptionCatcher;

        private TimeSpan _tipsTimeSpan;
        private CancellationTokenSource _cancellationTokenSource;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected virtual void OnEnable()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _tipsTimeSpan = TimeSpan.FromSeconds(TIPS_SWAP_INTERVAL);

            if (Configs == null) return;

            CancelButton.onClick.AddListener(RequestCancelAction);
            if (_cancellationTokenSource.IsCancellationRequested) return;
            if (_randomBackground != null) _randomBackground.Play(_cancellationTokenSource.Token);
            PlayFadeInAnimation();
            if (_cancellationTokenSource.IsCancellationRequested) return;
            TipsRoutine(_cancellationTokenSource.Token);
            _exceptionCatcher.ExceptionCaught += OnExceptionCaught;
            
            UpdateProgressBar(0f);
        }

        protected virtual void OnDisable()
        {
            _cancellationTokenSource.CancelAndDispose();
            _exceptionCatcher.ExceptionCaught -= OnExceptionCaught;
            CancelButton.onClick.RemoveAllListeners();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnConfigure(TConfig configuration)
        {
            _headerText.text = configuration.Header;
            _progressBarText.text = configuration.ProgressBarText;
            configuration.HideActionRequested += FadeOutAndHide;
        }

        protected override void OnHidden()
        {
            base.OnHidden();

            CancelButton.SetActive(false);
            CancelButton.onClick.RemoveAllListeners();

            CleanUp();

            _progressBar.Value = 0.0f;
            Configs = null;
        }

        protected abstract void OnFadeInAnimationStarted();

        protected abstract void CleanUp();

        protected virtual void UpdateProgressBar(float value)
        {
            _progressBar.Value = value;

            if (value >= 1.0f)
            {
                if (Configs.HideActionRequested != null) return;

                FadeOutAndHide();
            }
        }

        protected virtual void RequestCancelAction()
        {
            Configs.CancelActionRequested?.Invoke();

            PopupManager.ClosePopupByType(PopupType.AlertWithTitlePopup);
            FadeOutAndHide();
        }

        //---------------------------------------------------------------------
        // Private
        //---------------------------------------------------------------------

        [Button]
        private void PlayFadeInAnimation()
        {
            _canvasGroup.alpha = 1.0f;
            _canvasGroup.blocksRaycasts = true;
            _slideIn.InitSequence(new Vector3(0, 120), new Vector3(0, -265));
            _slideIn.SlideIn();
            Configs.FadeInCompleted?.Invoke();
            OnFadeInAnimationStarted();
            
            if (_cancellationTokenSource.IsCancellationRequested) return;
        }

        [Button]
        protected void PlayFadeOutAnimation()
        {
            _canvasGroup.alpha = 1.0f;
            _canvasGroup.blocksRaycasts = false;
            _fadeInOut.CleanUp();
            _fadeInOut.FadeOut();

            _slideIn.SlideOut();
        }

        private async void TipsRoutine(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                _tipsFadeInOut.CleanUp();
                _tipsFadeInOut.OnAnimationCompleted += UpdateTip;
                _tipsFadeInOut.FadeOut();
                await Task.Delay(_tipsTimeSpan);
            }

            void UpdateTip()
            {
                _tipsText.text = _tipsBank.GetRandomTip();
                if (token.IsCancellationRequested) return;

                _tipsFadeInOut.CleanUp();
                _tipsFadeInOut.FadeIn();
            }
        }

        private void OnExceptionCaught()
        {
            CancelButton.SetActive(true);

            var config = new AlertPopupConfiguration
            {
                PopupType = PopupType.AlertWithTitlePopup,
                ConfirmButtonText = "Go back",
                OnConfirm = RequestCancelAction,
                Description = "Oops looks like something is wrong, we can't load load necessary assets",
                Title = "Error",
            };

            PopupManager.SetupPopup(config);
            PopupManager.ShowPopup(config.PopupType, true);
        }

        private void FadeOutAndHide()
        {
            _fadeInOut.OnAnimationCompleted = null;
            _canvasGroup.alpha = 0.0f;
            _canvasGroup.blocksRaycasts = false;
            OnFadeOutComplete();
        }

        private void OnFadeOutComplete()
        {
            Hide(null);
        }
    }
}