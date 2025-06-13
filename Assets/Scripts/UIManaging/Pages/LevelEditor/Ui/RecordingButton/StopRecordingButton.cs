using System.Collections;
using DG.Tweening;
using Modules.LevelManaging.Editing;
using UIManaging.Localization;
using UIManaging.Pages.LevelEditor.Ui.Permissions;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.RecordingButton
{
    internal sealed class StopRecordingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private const float GRACE_PERIOD_TIME = 0.2f;
        private const float BREATHING_INTERVAL = 1.5f;
        private const float INTERVAL_BETWEEN_RECS_MIN = 0.5f;
        private static readonly IEnumerator LOCK_INTERVAL = new WaitForSecondsRealtime(INTERVAL_BETWEEN_RECS_MIN);

        [SerializeField] private RectTransform _recordCircleRect;

        [Inject] private EventRecordingService _eventRecordingService;
        [Inject] private AudioRecordingStateController _audioRecordingStateController;
        [Inject] private MicrophonePermissionHelper _microphonePermissionHelper;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private LevelEditorCameraSettingsLocalization _localization;
        
        private readonly Vector3 _growScale = new(1.18f, 1.18f, 1f);
        private Sequence _breathSequence;
        private Vector3 _originalScale;

        private int _currentPointerId;
        private bool _isPointerDown;
        private bool _isGracePeriodRunning;
        private bool _isRecordingLocked;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void OnEnable()
        {
            _eventRecordingService.RecordingStarted += PlayBreathingTween;
            _eventRecordingService.RecordingCancelled += OnRecordingEnded;
            _eventRecordingService.RecordingEnded += OnRecordingEnded;
        }

        private void OnDisable()
        {
            _eventRecordingService.RecordingStarted -= PlayBreathingTween;
            _eventRecordingService.RecordingCancelled -= OnRecordingEnded;
            _eventRecordingService.RecordingEnded -= OnRecordingEnded;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_isRecordingLocked) return;
            _currentPointerId = eventData.pointerId;
            _isPointerDown = true;

            TryStartHoldingRecording();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_currentPointerId != eventData.pointerId) return;
            _isPointerDown = false;

            if (_eventRecordingService.IsRecording)
            {
                _eventRecordingService.StopRecording();
                return;
            }

            StopGracePeriodIfRunning();

            TryStartRecording();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void TryStartHoldingRecording()
        {
            if (!_eventRecordingService.IsRecordingAllowed)
            {
                DisplayTimeLimitReachedMessage();
                return;
            }

            StartCoroutine(HoldGracePeriod());
        }

        private void TryStartRecording()
        {
            if (!_eventRecordingService.IsRecordingAllowed || _eventRecordingService.IsSavingLastRecording || _isRecordingLocked) return;

            var requestPermission = _audioRecordingStateController.State == AudioRecordingState.Voice &&
                                    !_microphonePermissionHelper.IsPermissionGranted;
            if (requestPermission)
            {
                _microphonePermissionHelper.RequestPermission();
                return;
            }

            _eventRecordingService.StartRecording();
        }

        private IEnumerator HoldGracePeriod()
        {
            _isGracePeriodRunning = true;
            yield return new WaitForSeconds(GRACE_PERIOD_TIME);

            if (_isPointerDown) TryStartRecording();
            _isGracePeriodRunning = false;
        }

        private void StopGracePeriodIfRunning()
        {
            if (!_isGracePeriodRunning) return;

            StopCoroutine(HoldGracePeriod());
            _isGracePeriodRunning = false;
        }

        private void DisplayTimeLimitReachedMessage()
        {
            _snackBarHelper.ShowInformationSnackBar(_localization.TimeLimitReachedSnackbarMessage, 2);
        }
        
        private void PlayBreathingTween()
        {
            _originalScale = _recordCircleRect.localScale;
            
            _breathSequence?.Kill();
            _breathSequence = DOTween.Sequence();
            _breathSequence.Append(_recordCircleRect.DOScale(_growScale, BREATHING_INTERVAL));
            _breathSequence.Append(_recordCircleRect.DOScale(_originalScale, BREATHING_INTERVAL));
            _breathSequence.SetLoops(-1);
        }

        private void StopBreathingTween()
        {
            _breathSequence?.Kill();
            _recordCircleRect.localScale = Vector3.one;
        }

        private IEnumerator LockCoroutine()
        {
            _isRecordingLocked = true;
            yield return LOCK_INTERVAL;
            _isRecordingLocked = false;
        }

        private void OnRecordingEnded()
        {
            StopBreathingTween();
            StartCoroutine(LockCoroutine());
        }
    }
}