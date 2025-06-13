using DG.Tweening;
using Extensions;
using Modules.LevelManaging.Editing.LevelManagement;
using TMPro;
using UIManaging.Pages.LevelEditor.Ui.FeatureControls;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using PlayMode = Modules.LevelManaging.Editing.Players.EventPlaying.PlayMode;

namespace UIManaging.Pages.LevelEditor.Ui.RecordingButton
{
    internal sealed class LevelDurationProgressUI : MonoBehaviour
    {
        [SerializeField] private Image _levelDurationFillImage;
        [SerializeField] private Image _lastEventFillImage;
        [SerializeField] private Image _paleEventsFillImage;
        [SerializeField] private Image _stopIcon;
        [SerializeField] private Image _recIcon;
        [SerializeField] private TextMeshProUGUI _durationText;
        [SerializeField] private GameObject _uploadGalleryVideoUiParent;
        [SerializeField] private GameObject _uiWithEventContainer;
        [SerializeField] private EventIndicatorSpawner _eventIndicatorSpawner;

        [Inject] private ILevelManager _levelManager;
        [Inject] private INonLevelVideoUploadFeatureControl _levelVideoUploadFeature;
        private Sequence _fadeLastEventFillImage;

        private float LevelDurationFillAmount
        {
            get => _levelDurationFillImage.fillAmount;
            set => _levelDurationFillImage.fillAmount = value;
        }

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        private float MaxDuration => _levelManager.MaxLevelDurationSec;
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Setup()
        {
            _levelManager.RecordingStarted += OnStartRecording;
            _levelManager.RecordingEnded += OnRecordingEnded;
            _levelManager.RecordingCancelled += OnRecordingCancelled;
            _levelManager.EventStarted += OnEventStarted;
            _levelManager.EventSaved += OnEventSaved;
        }

        public void SetProgress(float fillAmount)
        {
            LevelDurationFillAmount = fillAmount;
            SetDurationText(fillAmount);
        }

        public void SetupEventIndicators()
        {
            _eventIndicatorSpawner.DisposeActiveIndicators();

            var eventEndTime = 0f;
            foreach (var ev in _levelManager.CurrentLevel.Event)
            {
                eventEndTime += ev.Length.ToSeconds();
                _eventIndicatorSpawner.PlaceIndicator(eventEndTime / MaxDuration, ev.LevelSequence);
            }
        }

        public void PlayDeleteLastEventAnimation()
        {
            PrepareForDeleteLastEventAnimation();

            const float startFadeValue = 1f;
            const float endFadeValue = 0.3f;
            const float fadeDuration = 0;
            const float fadeIntervalTime = 0.8f;
            const int timesToLoop = 3;

            _fadeLastEventFillImage?.Kill();
            _fadeLastEventFillImage = DOTween.Sequence();
            _fadeLastEventFillImage.SetLoops(timesToLoop, LoopType.Restart);
            _fadeLastEventFillImage.OnKill(OnAnimationKill);
            _fadeLastEventFillImage.Append(_lastEventFillImage.DOFade(startFadeValue, fadeDuration));
            _fadeLastEventFillImage.AppendInterval(fadeIntervalTime);
            _fadeLastEventFillImage.Append(_lastEventFillImage.DOFade(endFadeValue, fadeDuration));
            _fadeLastEventFillImage.AppendInterval(fadeIntervalTime);
        }

        public void StopDeleteLastEventAnimation()
        {
            _fadeLastEventFillImage?.Kill();
        }

        public void RemoveEventIndicator(int eventSequence)
        {
            _eventIndicatorSpawner.RemoveIndicator(eventSequence);
        }

        public void PaleAllEvents()
        {
            _paleEventsFillImage.fillAmount = _levelManager.LevelDurationSec / MaxDuration;
        }

        public void UnPaleAllEvents()
        {
            _paleEventsFillImage.fillAmount = 0;
        }
        
                
        public void UnsubscribeEvents()
        {
            _levelManager.RecordingStarted -= OnStartRecording;
            _levelManager.RecordingEnded -= OnRecordingEnded;
            _levelManager.RecordingCancelled -= OnRecordingCancelled;
            _levelManager.EventStarted -= OnEventStarted;
            _levelManager.EventSaved -= OnEventSaved;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void SetDurationText(float amount)
        {
            var duration = amount * MaxDuration;
            _durationText.text = duration.ToString("F1") + "s";
        }

        private void OnStartRecording()
        {
            _recIcon.gameObject.SetActive(false);
            _stopIcon.gameObject.SetActive(true);
            _uiWithEventContainer.SetActive(true);
            _uploadGalleryVideoUiParent.SetActive(false);
        }

        private void OnRecordingEnded()
        {
            _recIcon.gameObject.SetActive(true);
            _stopIcon.gameObject.SetActive(false);
        }
        
        private void OnRecordingCancelled()
        {
            _recIcon.gameObject.SetActive(true);
            _stopIcon.gameObject.SetActive(false);
            _uiWithEventContainer.SetActive(!_levelManager.IsLevelEmpty);
            _uploadGalleryVideoUiParent.SetActive(ShouldShowUploadGalleryVideoUI());
        }

        private void OnEventStarted()
        {
            if (_levelManager.CurrentPlayMode == PlayMode.Preview) return;

            _uiWithEventContainer.SetActive(!_levelManager.IsLevelEmpty);
            _uploadGalleryVideoUiParent.SetActive(ShouldShowUploadGalleryVideoUI());
        }

        private void OnEventSaved()
        {
            _eventIndicatorSpawner.PlaceIndicator(LevelDurationFillAmount, _levelManager.GetLastEvent().LevelSequence);
        }

        private bool ShouldShowUploadGalleryVideoUI()
        {
            return _levelManager.IsLevelEmpty && _levelVideoUploadFeature.IsFeatureEnabled;
        }

        private void PrepareForDeleteLastEventAnimation()
        {
            _lastEventFillImage.gameObject.SetActive(true);
            var lastEventSequence = _levelManager.GetLastEvent().LevelSequence;
            var levelDurationBeforeLastEvent =
                _levelManager.CurrentLevel.GetEventsDurationSecBeforeEvent(lastEventSequence, false);
            var fillAmountWithoutLastEvent = levelDurationBeforeLastEvent / MaxDuration;
            var originalImageFillAmount = LevelDurationFillAmount;
            LevelDurationFillAmount= fillAmountWithoutLastEvent;
            _lastEventFillImage.fillAmount = originalImageFillAmount;
        }

        private void OnAnimationKill()
        {
            _lastEventFillImage.DOFade(1, 0);
            _lastEventFillImage.gameObject.SetActive(false);
            var currentFillAmount = _levelManager.LevelDurationSeconds / MaxDuration;
            LevelDurationFillAmount = currentFillAmount;
        }
    }
}