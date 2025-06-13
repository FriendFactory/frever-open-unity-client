using System;
using Common.Abstract;
using Extensions;
using Modules.LevelManaging.Editing;
using UIManaging.PopupSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui
{
    public class AudioRecordingPanelToggle: BaseContextlessPanel 
    {
        [SerializeField] private Toggle _toggle;
        [SerializeField] private Image _voiceEnabledTargetGraphic;
        [SerializeField] private Image _voiceDisabledTargetGraphic;
        
        [Inject] private AudioRecordingStateController _stateController;
        [Inject] private PopupManagerHelper _popupManagerHelper;

        public Action<bool> ValueChanged;

        protected override void OnInitialized()
        {
            _toggle.onValueChanged.AddListener(OnToggleValueChanged);

            _stateController.TransitionStarted += OnTransitionStarted;
            _stateController.RecordingStateChanged += OnRecordingStateChanged;
        }

        protected override void BeforeCleanUp()
        {
            _toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
            
            _stateController.TransitionStarted -= OnTransitionStarted;
            _stateController.RecordingStateChanged -= OnRecordingStateChanged;
        }

        private void OnRecordingStateChanged(AudioRecordingState source, AudioRecordingState destination)
        {
            var isVoice = destination == AudioRecordingState.Voice;
            var isMusic = destination == AudioRecordingState.MusicActivated || destination == AudioRecordingState.MusicSelected;
            
            SetTargetGraphic(isVoice);
            
            _toggle.interactable = true;
            _toggle.SetIsOnWithoutNotify(isMusic);
        }
        
        private void OnTransitionStarted(AudioRecordingState source, AudioRecordingState destination)
        {
            _toggle.interactable = false;
        }

        private void OnToggleValueChanged(bool isEnabled)
        {
            if (!isEnabled && _stateController.State == AudioRecordingState.MusicSelected)
            {
                ShowDialogPopup();
            }
            else
            {
                ValueChanged?.Invoke(isEnabled);
            }
        }

        private void ShowDialogPopup()
        {
            var title = "Are you sure?";
            var description = "If you want to record with microphone we need to remove your selected sound";
            var noText = "Cancel";
            var yesText = "Yes";

            _popupManagerHelper.ShowDialogPopup(title, description, noText, OnCancel, yesText, OnYes, false);

            void OnYes()
            {
                _toggle.SetIsOnWithoutNotify(false);
                ValueChanged?.Invoke(false);
            }

            void OnCancel()
            {
                _toggle.SetIsOnWithoutNotify(true);
            }
        }
        
        private void SetTargetGraphic(bool isVoiceEnabled)
        {
            _voiceEnabledTargetGraphic.SetActive(isVoiceEnabled);
            _voiceDisabledTargetGraphic.SetActive(!isVoiceEnabled);

            _toggle.targetGraphic = isVoiceEnabled ? _voiceEnabledTargetGraphic : _voiceDisabledTargetGraphic;
        }
    }
}