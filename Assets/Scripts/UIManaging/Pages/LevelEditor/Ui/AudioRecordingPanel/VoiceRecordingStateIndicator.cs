using Common.Abstract;
using Common.Permissions;
using Extensions;
using Modules.LevelManaging.Editing;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.LevelEditor.Ui.Permissions;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui
{
    internal sealed class VoiceRecordingStateIndicator: BaseContextlessPanel 
    {
        [SerializeField] private Image _microphoneIcon;
        [SerializeField] private Color32 _enabledColor = Color.white;
        [SerializeField] private Color32 _disabledColor = Color.gray;
        
        [Inject] private ILevelManager _levelManager;
        [Inject] private MicrophonePermissionHelper _microphonePermissionHelper;
        [Inject] private AudioRecordingStateController _audioRecordingStateController;

        private void Awake()
        {
            _microphoneIcon.SetActive(false);
        }

        protected override void OnInitialized()
        {
            UpdateIconState();
            UpdateIconColor(_microphonePermissionHelper.IsPermissionGranted);

            _levelManager.TargetCharacterSequenceNumberChanged += OnTargetCharacterSequenceNumberChanged;
            _levelManager.EditingCharacterSequenceNumberChanged += OnTargetCharacterSequenceNumberChanged;
            _microphonePermissionHelper.PermissionStatusChanged += OnPermissionStatusChanged; 
            _audioRecordingStateController.RecordingStateChanged += OnAudioRecordingStateChanged;
        }

        protected override void BeforeCleanUp()
        {
            _levelManager.TargetCharacterSequenceNumberChanged -= OnTargetCharacterSequenceNumberChanged;
            _levelManager.EditingCharacterSequenceNumberChanged -= OnTargetCharacterSequenceNumberChanged;
            _microphonePermissionHelper.PermissionStatusChanged -= OnPermissionStatusChanged;
            _audioRecordingStateController.RecordingStateChanged -= OnAudioRecordingStateChanged;
        }

        private void OnTargetCharacterSequenceNumberChanged()
        {
            UpdateIconState();
        }

        private void OnAudioRecordingStateChanged(AudioRecordingState source, AudioRecordingState destination)
        {
            UpdateIconState();
        }
        
        private void OnPermissionStatusChanged(PermissionStatus status)
        {
            UpdateIconColor(status.IsGranted());
        }

        private void UpdateIconState()
        {
            var charactersCount = _levelManager.TargetEvent?.CharacterController?.Count ?? 0;
            var audioRecordingState = _audioRecordingStateController.State;
            
            _microphoneIcon.SetActive(charactersCount <= 1 && audioRecordingState == AudioRecordingState.Voice);
        }

        private void UpdateIconColor(bool isEnabled)
        {
            _microphoneIcon.color = isEnabled ? _enabledColor : _disabledColor;
        }
    }
}