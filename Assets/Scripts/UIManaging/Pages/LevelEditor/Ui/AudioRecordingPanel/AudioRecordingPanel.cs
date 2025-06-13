using System;
using System.Collections;
using Bridge.Models.Common;
using Common.Abstract;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.Common.SongOption;
using UIManaging.Pages.Common.SongOption.Common;
using UIManaging.Pages.LevelEditor.Ui.Permissions;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui
{
    internal sealed class AudioRecordingPanel: BaseContextlessPanel
    {
        [SerializeField] private MusicSelectionPage _musicSelectionPage;
        [SerializeField] private AudioRecordingPanelAnimator _panelAnimator;
        [SerializeField] private MusicRecordingPanel _musicRecordingPanel;
        [SerializeField] private AudioRecordingPanelToggle _toggle;
        [SerializeField]private VoiceRecordingPanelIndicator _voiceRecordingPanelIndicator;

        private SongSelectionController _songSelectionController;
        private AudioRecordingStateController _stateController;
        private ILevelManager _levelManager;
        private MicrophonePermissionHelper _microphonePermissionHelper;
        private LevelEditorPageModel _pageModel;

        [Inject, UsedImplicitly]
        public void Construct(SongSelectionController songSelectionController,
            AudioRecordingStateController stateController,
            ILevelManager levelManager, MicrophonePermissionHelper microphonePermissionHelper,
            LevelEditorPageModel pageModel)
        {
            _songSelectionController = songSelectionController;
            _stateController = stateController;
            _levelManager = levelManager;
            _microphonePermissionHelper = microphonePermissionHelper;
            _pageModel = pageModel;
        }

        protected override void OnInitialized()
        {
            _panelAnimator.Initialize();
            _musicRecordingPanel.Initialize();
            _toggle.Initialize();
            _voiceRecordingPanelIndicator.Initialize();
            
            _toggle.ValueChanged += OnToggleValueChanged;

            _stateController.RecordingStateChanged += OnRecordingStateChanged;
            _songSelectionController.SongApplied += OnSongApplied;

            _levelManager.RecordingStarted += EnablePreviewMode;
            _levelManager.RecordingEnded += DisablePreviewMode;
            _levelManager.RecordingCancelled += DisablePreviewMode;
            _levelManager.LevelPreviewStarted += EnablePreviewMode;
            _levelManager.LevelPreviewCompleted += DisablePreviewMode;
        }

        protected override void BeforeCleanUp()
        {
            _panelAnimator.CleanUp();
            _musicRecordingPanel.CleanUp();
            _toggle.CleanUp();
            _voiceRecordingPanelIndicator.CleanUp();
            
            _toggle.ValueChanged = OnToggleValueChanged;
            
            _stateController.RecordingStateChanged -= OnRecordingStateChanged;
            _songSelectionController.SongApplied -= OnSongApplied;
            
            _levelManager.RecordingStarted -= EnablePreviewMode;
            _levelManager.RecordingEnded -= DisablePreviewMode;
            _levelManager.RecordingCancelled -= DisablePreviewMode;
            _levelManager.LevelPreviewStarted -= EnablePreviewMode;
            _levelManager.LevelPreviewCompleted -= DisablePreviewMode;
        }

        private void EnablePreviewMode()
        {
            if (_stateController.State != AudioRecordingState.MusicSelected) return;
            
            _stateController.FireAsync(AudioRecordingTrigger.StartMusicPreview);
        }

        private void DisablePreviewMode()
        {
            if (_stateController.State != AudioRecordingState.MusicPreviewed) return;
            
            _stateController.FireAsync(AudioRecordingTrigger.StopMusicPreview);
        }

        private void OnRecordingStateChanged(AudioRecordingState source, AudioRecordingState destination)
        {
            var openMusicPage = source == AudioRecordingState.Voice && destination == AudioRecordingState.MusicActivated;
            if (openMusicPage)
            {
                _musicSelectionPage.Display(new MusicSelectionPageArgs
                {
                    SelectionPurpose = SelectionPurpose.ForRecordingNewEvent
                }, null, null);
                return;
            }

            var requestPermission = (source != AudioRecordingState.None && destination == AudioRecordingState.Voice) &&
                                    !_microphonePermissionHelper.IsPermissionGranted;
            if (requestPermission)
            {
                StartCoroutine(RequestMicrophonePermissionRoutine());
            }
        }

        private void OnToggleValueChanged(bool isEnabled)
        {
            var trigger = isEnabled ? AudioRecordingTrigger.ActivateMusic : AudioRecordingTrigger.ActiveVoice;

            _stateController.FireAsync(trigger);
            
            // we need to reset the song selection, but only after state transition is started,
            // in order to properly handle this case in OnSongApplied callback
            if (trigger == AudioRecordingTrigger.ActiveVoice && _songSelectionController.SelectedSong != null)
            {
                _songSelectionController.ApplySong(null, -1);
            }
        }

        private void OnSongApplied(IPlayableMusic selectedSong, int _)
        {
            if (_pageModel.PrevState is LevelEditorState.TemplateSetup) return;
            
            if (_stateController.IsTransitioning && _stateController.DestinationState == AudioRecordingState.Voice) return;
            
            var targetState = selectedSong == null ? AudioRecordingState.MusicActivated : AudioRecordingState.MusicSelected;
            
            if (_stateController.State == targetState) return;
            
            var trigger = selectedSong == null ? AudioRecordingTrigger.ActivateMusic : AudioRecordingTrigger.SelectMusic;
            
            _stateController.FireAsync(trigger);
        }

        private IEnumerator RequestMicrophonePermissionRoutine()
        {
            // we need to wait for a little until all state changed callbacks are processed
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            
            _microphonePermissionHelper.RequestPermission();
        }
    }
}