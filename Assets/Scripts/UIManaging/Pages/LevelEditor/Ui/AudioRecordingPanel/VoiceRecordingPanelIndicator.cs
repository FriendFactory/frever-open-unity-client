using Common.Abstract;
using Common.Permissions;
using Modules.LevelManaging.Editing;
using UIManaging.Pages.LevelEditor.Ui.Permissions;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui
{
    public class VoiceRecordingPanelIndicator: BaseContextlessPanel
    {
        [SerializeField] private AudioRecordingStateBasedColorSwitcher _enabledIconStateColorSwitcher;
        [SerializeField] private AudioRecordingStateBasedColorSwitcher _disabledIconStateColorSwitcher;
        [SerializeField] private Image _microphoneIcon;
        [SerializeField] private Color _enabledColor = Color.white;
        [SerializeField] private Color _disabledColor = Color.gray;

        [Inject] private MicrophonePermissionHelper _permissionHelper;
        [Inject] private AudioRecordingStateController _stateController;

        protected override void OnInitialized()
        {
            _enabledIconStateColorSwitcher.Initialize();
            _disabledIconStateColorSwitcher.Initialize();
            
            UpdateIconColor();

            _permissionHelper.PermissionStatusChanged += OnPermissionStatusChanged;
            _stateController.RecordingStateChanged += OnRecordingStateChanged;
        }

        protected override void BeforeCleanUp()
        {
            _enabledIconStateColorSwitcher.CleanUp();
            _disabledIconStateColorSwitcher.CleanUp();
            
            _permissionHelper.PermissionStatusChanged -= OnPermissionStatusChanged;
            _stateController.RecordingStateChanged -= OnRecordingStateChanged;
        }

        private void OnPermissionStatusChanged(PermissionStatus status) => UpdateIconColor();

        private void OnRecordingStateChanged(AudioRecordingState source, AudioRecordingState destination)
        {
            if (destination != AudioRecordingState.Voice) return;
            
            UpdateIconColor();
        }

        private void UpdateIconColor()
        {
            _microphoneIcon.color = _permissionHelper.IsPermissionGranted ? _enabledColor : _disabledColor;
        }
    }
}