using System;
using System.Collections.Generic;
using Bridge.Models.Common;
using Extensions;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.Common.SongOption;
using UIManaging.Pages.Common.SongOption.Common;
using UIManaging.Pages.UmaEditorPage.Ui;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow
{
    [UsedImplicitly]
    internal sealed class BackgroundMusicPlayer: IInitializable, IDisposable
    {
        private readonly ILevelManager _levelManager;
        private readonly UmaLevelEditorPanelModel _umaLevelEditorPanelModel;
        private readonly MusicSelectionPageModel _musicSelectionPageModel;
        private readonly MusicPlayerController _musicPlayerController;
        private readonly LevelEditorPageModel _levelEditorPageModel;
        
        private readonly HashSet<LevelEditorState> _playOnStates = new HashSet<LevelEditorState>
        {
            LevelEditorState.TemplateSetup,
            LevelEditorState.Dressing,
            LevelEditorState.PurchasableAssetSelection,
            LevelEditorState.AssetSelection,
        };
        
        private IPlayableMusic _currentMusic;
        private float _activationCue;

        private bool IsPlaying { get; set; }
        private bool IsMusicSelectionPageOpened { get; set; }

        public BackgroundMusicPlayer(MusicPlayerController musicPlayerController, MusicSelectionPageModel musicSelectionPageModel,
            LevelEditorPageModel levelEditorPageModel, ILevelManager levelManager, UmaLevelEditorPanelModel umaLevelEditorPanelModel)
        {
            _musicPlayerController = musicPlayerController;
            _musicSelectionPageModel = musicSelectionPageModel;
            _levelEditorPageModel = levelEditorPageModel;
            _levelManager = levelManager;
            _umaLevelEditorPanelModel = umaLevelEditorPanelModel;
        }

        public void Initialize()
        {
            _musicSelectionPageModel.PageOpened += OnPageOpened;
            _musicSelectionPageModel.PageClosed += OnPageClosed;
            
            _levelEditorPageModel.StateChanged += OnStateChanged;
            _levelEditorPageModel.StartOverRequested += OnStartOverRequested;
            
            _levelManager.SongChanged += OnSongChanged;
            _levelManager.TemplateApplyingCompleted += OnSongChanged;

            _umaLevelEditorPanelModel.PanelOpened += OnUmaPanelOpened;
            _umaLevelEditorPanelModel.PanelClosed += OnUmaPanelClosed;
        }

        private void CleanUp()
        {
            Stop();
            
            _currentMusic = null;
            
            _musicSelectionPageModel.PageOpened -= OnPageOpened;
            _musicSelectionPageModel.PageCloseRequested -= OnPageClosed;
            
            _levelEditorPageModel.StateChanged -= OnStateChanged;
            _levelEditorPageModel.StartOverRequested -= OnStartOverRequested;
            
            _levelManager.SongChanged -= OnSongChanged;
            _levelManager.TemplateApplyingCompleted -= OnSongChanged;
            
            _umaLevelEditorPanelModel.PanelOpened -= OnUmaPanelOpened;
            _umaLevelEditorPanelModel.PanelClosed -= OnUmaPanelClosed;
        }

        public void Dispose()
        {
            CleanUp();
        }

        private void OnSongChanged()
        {
            var targetEvent = _levelManager.TargetEvent;
            var musicController = targetEvent.GetMusicController();

            if (musicController == null)
            {
                _currentMusic = null;
                Stop();
                return;
            }

            var song = musicController.Song;
            var userSound = musicController.UserSound;
            var externalSong = musicController.ExternalTrack;
            var currentMusic = song ?? (IPlayableMusic)userSound ?? externalSong;

            _currentMusic = currentMusic;
            _activationCue = musicController.ActivationCue / 1000f;
            
            _musicPlayerController.PrepareMusicForPlaying(_currentMusic);
        }

        private void OnStateChanged(LevelEditorState state)
        {
            // SongChanged event is not fired when LE is opened from a template
            if (_levelEditorPageModel.PrevState == LevelEditorState.None && _currentMusic == null)
            {
                OnSongChanged();
            }

            if (IsMusicSelectionPageOpened) return;

            if (state is LevelEditorState.PurchasableAssetSelection or LevelEditorState.AssetSelection &&
                _levelEditorPageModel.PrevState == LevelEditorState.Default) return;
            
            if (_playOnStates.Contains(state))
            {
                Play();
            }
            else
            {
                Stop();
            }
        }

        private void OnPageOpened()
        {
            IsMusicSelectionPageOpened = true;
            
            if (_levelEditorPageModel.PrevState == LevelEditorState.Default) return;
            
            _musicPlayerController.Stop();
        }

        private void OnPageClosed()
        {
            IsMusicSelectionPageOpened = false;
            
            // page is closed when LE is already switched to another state
            if (_levelEditorPageModel.EditorState == LevelEditorState.Default) return;
            
            Play();
        }
        
        private void Play()
        {
            if (IsPlaying) return;
            
            if (_currentMusic == null) return;
            
            IsPlaying = true;

            _musicPlayerController.PlayMusic(_currentMusic, _activationCue, true);
        }
        
        private void Stop()
        {
            if (!IsPlaying) return;
            
            if (_currentMusic == null) return;
            
            IsPlaying = false;
            
            _musicPlayerController.Stop();
        }

        private void OnUmaPanelOpened()
        {
            if (_levelEditorPageModel.PrevState == LevelEditorState.Default) return;
            
            _musicPlayerController.SetVolume(0f);
        }

        private void OnUmaPanelClosed()
        {
            if (_levelEditorPageModel.PrevState == LevelEditorState.Default) return;
            
            _musicPlayerController.SetVolume(1f);
        }

        private void OnStartOverRequested()
        {
            Stop();
            
            _musicPlayerController.ClearCache();
        }
    }
}