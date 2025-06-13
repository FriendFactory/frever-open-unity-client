using System;
using System.Collections;
using Common.ProgressBars;
using DG.Tweening;
using Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.VideoMessage.ImageGeneration
{
    public class GenerationProgressOverlay : MonoBehaviour
    {
        private const float DEFAULT_TIMEOUT = 30f;

        [SerializeField] private float _fadeInSpeed = 1f;
        [SerializeField] private float _fadeOutSpeed = 2.5f;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Loading")]
        [SerializeField] private ProgressBar _progressBar;
        [SerializeField] protected Button _cancelButton;
        [SerializeField] private ProgressSimulator _progressSimulator;

        private Action _cancelRequested;
        private Coroutine _timeoutCoroutine;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnEnable()
        {
            _cancelButton.onClick.AddListener(OnCancelRequested);
        }

        private void OnDisable()
        {
            _cancelButton.SetActive(false);
            _cancelButton.onClick.RemoveAllListeners();

            _canvasGroup.DOKill();
            _canvasGroup.alpha = 0f;

            if (_timeoutCoroutine != null) StopCoroutine(_timeoutCoroutine);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Show(Action onCancel)
        {
            gameObject.SetActive(true);

            _cancelRequested = onCancel;

            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.DOKill();
            _canvasGroup.DOFade(1f, _fadeInSpeed).SetUpdate(true);

            _progressBar.Value = 0f;
            _cancelButton.SetActive(false);

            _progressSimulator.ProgressUpdated += UpdateProgressBar;
            _progressSimulator.StartSimulation();

            _timeoutCoroutine = StartCoroutine(HandleTimeoutActions(DEFAULT_TIMEOUT));
        }

        public void Hide()
        {
            _cancelRequested = null;

            _canvasGroup.DOKill();
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.DOFade(0f, _fadeOutSpeed).SetUpdate(true)
                        .OnComplete(() => gameObject.SetActive(false));

            _progressBar.Value = 1f;

            _progressSimulator.ProgressUpdated -= UpdateProgressBar;
            _progressSimulator.StopSimulation();

            if (_timeoutCoroutine != null) StopCoroutine(_timeoutCoroutine);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private IEnumerator HandleTimeoutActions(float timeOut)
        {
            yield return new WaitForSecondsRealtime(timeOut);
            _cancelButton.SetActive(true);
        }

        private void UpdateProgressBar(float value)
        {
            _progressBar.Value = value;
        }

        private void OnCancelRequested()
        {
            _cancelRequested?.Invoke();
        }
    }
}