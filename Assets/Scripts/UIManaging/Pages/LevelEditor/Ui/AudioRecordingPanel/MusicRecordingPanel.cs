using Bridge.Models.Common;
using Common.Abstract;
using Extensions;
using Modules.LevelManaging.Editing;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.Common.SongOption.Common;
using UIManaging.Pages.Common.SongOption.SongTitlePanel;
using UnityEngine;
using Zenject;
using PlayMode = Modules.LevelManaging.Editing.Players.EventPlaying.PlayMode;

namespace UIManaging.Pages.LevelEditor.Ui
{
    public class MusicRecordingPanel: BaseContextlessPanel
    {
        [SerializeField] private CanvasGroup _musicNonSelectedGroup;
        [SerializeField] private CanvasGroup _musicSelectedGroup;
        [SerializeField] private SongSelectedPanel _songSelectedPanel;
        
        [Inject] private SongSelectionController _songSelectionController;
        [Inject] private AudioRecordingStateController _stateController;
        [Inject] private ILevelManager _levelManager;
        
        protected override void OnInitialized()
        {
            _stateController.TransitionStarted += OnTransitionStarted;
            _stateController.RecordingStateChanged += OnRecordingStateChanged;
            
            _levelManager.EventLoadingCompleted += OnEventLoaded;
            _levelManager.SongChanged += OnSongChanged; 
            
            _songSelectionController.SongApplied += OnSongApplied;
        }

        protected override void BeforeCleanUp()
        {
            _stateController.TransitionStarted -= OnTransitionStarted;
            _stateController.RecordingStateChanged -= OnRecordingStateChanged;
            
            _levelManager.EventLoadingCompleted -= OnEventLoaded;
            _levelManager.SongChanged -= OnSongChanged; 
            
            _songSelectionController.SongApplied -= OnSongApplied;
        }

        private void OnTransitionStarted(AudioRecordingState source, AudioRecordingState destination)
        {
            _musicNonSelectedGroup.interactable = false;
            _musicSelectedGroup.interactable = false;
        }

        private void OnRecordingStateChanged(AudioRecordingState source, AudioRecordingState destination)
        {
            _musicNonSelectedGroup.interactable = destination == AudioRecordingState.MusicActivated;
            _musicSelectedGroup.interactable = destination == AudioRecordingState.MusicActivated || destination == AudioRecordingState.MusicSelected;

            var showNonSelected = destination == AudioRecordingState.Voice || destination == AudioRecordingState.MusicActivated;
            var showSelected = destination == AudioRecordingState.MusicSelected || destination == AudioRecordingState.MusicPreviewed;
            
            _musicNonSelectedGroup.SetActive(showNonSelected);
            _musicSelectedGroup.SetActive(showSelected);
        }

        private void OnEventLoaded() 
        {
            var playMode = _levelManager.CurrentPlayMode;
            if (playMode == PlayMode.Preview || playMode == PlayMode.Recording || playMode == PlayMode.PreviewWithCameraTemplate) return;
            RefreshState();
        }

        private void OnSongApplied(IPlayableMusic selectedSong, int _)
        {
            if (selectedSong == null) return;
            
            _songSelectedPanel.UpdateSongTitle(selectedSong);
        }

        /// <summary>
        /// Callback to handle a case when song has changed after camera permission request
        /// </summary>
        private void OnSongChanged()
        {
            _levelManager.SongChanged -= OnSongChanged; 
            
            RefreshState();
        }

        private void RefreshState()
        {
            var targetEvent = _levelManager.TargetEvent;
            var musicController = targetEvent.GetMusicController();
            var audioRecordingState = _stateController.State;

            if (musicController == null)
            {
                if (audioRecordingState == AudioRecordingState.MusicSelected)
                {
                    _stateController.FireAsync(AudioRecordingTrigger.ActivateMusic);
                }
                return;
            }
            
            var song = musicController.Song;
            var userSound = musicController.UserSound;
            var externalSong = musicController.ExternalTrack;
            var currentMusic = song ?? (IPlayableMusic) userSound ?? externalSong;

            if (currentMusic != null)
            {
                _songSelectedPanel.UpdateSongTitle(currentMusic);
            }

            if (audioRecordingState != AudioRecordingState.MusicSelected)
            {
                var trigger = audioRecordingState == AudioRecordingState.MusicPreviewed ? AudioRecordingTrigger.StopMusicPreview : AudioRecordingTrigger.SelectMusic;
                _stateController.FireAsync(trigger);
            }
        }
    }
}