using System;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using Extensions;
using Models;
using Modules.LevelManaging.Editing.CameraManaging.CameraInputHandling;
using Modules.LevelManaging.Editing.LevelManagement;
using StansAssets.Foundation.Patterns;
using UIManaging.Pages.Common.SongOption.AudioMixer;
using UIManaging.Pages.Common.SongOption.Common;
using UIManaging.Pages.Common.SongOption.MusicCue;
using UIManaging.Pages.Common.SongOption.MusicLicense;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Common.SongOption
{
    internal abstract class MusicTypeGalleryViewBase: BaseContextlessView
    {
        [SerializeField] private RectTransform _scrollLayout;
        [SerializeField] private AudioMixerScreen _audioMixerScreen;
        
        [Inject] private ILevelManager _levelManager;
        [Inject] private SongSelectionController _songSelectionController;
        [Inject] private ICameraInputController _cameraInputController;
        [Inject] private MusicPlayerController _musicPlayerController;
        [Inject] private MusicCueScreen _musicCueScreen;
        [Inject] private LocalUserDataHolder _userDataHolder;
        [Inject] private IBridge _bridge;
        
        private int _activationCue;
        private CancellationTokenSource _downloadMusicCancellationToken;

        protected abstract MusicLicenseType MusicLicenseType { get; }

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action SongSelectionOpened;
        public event Action SongSelectionClosed;

        //---------------------------------------------------------------------
        // Protected 
        //---------------------------------------------------------------------

        protected override void OnShow()
        {
            _cameraInputController.Activate(false);
            
            SetupSelectedSong();
            var activationCue = _levelManager.TargetEvent.GetMusicController()?.ActivationCue ?? 0;
            
            OnSongSelectionChanged(null, _songSelectionController.SelectedSong);
            _musicCueScreen.UpdateSliderCue(activationCue);
            _activationCue = activationCue;
            _audioMixerScreen.Init(OnPlay);
            
            // need to subscribe here, because otherwise event will be fired 2 times for two inheritor views
            _songSelectionController.SelectedSongChanged += OnSongSelectionChanged;
            _songSelectionController.SongApplied += OnSongApplied;
            
            StaticBus<SongSelectedEvent>.Subscribe(OnSongSelected);
            
            SongSelectionOpened?.Invoke();
        }
        
        protected override void OnHide()
        {
            _cameraInputController.Activate(true);
            _audioMixerScreen.gameObject.SetActive(false);
            _musicCueScreen.Hide();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollLayout);
            gameObject.SetActive(false);
            
            Stop();
            
            _songSelectionController.SelectedSongChanged -= OnSongSelectionChanged;
            _songSelectionController.SongApplied -= OnSongApplied;
            
            SongSelectionOpened = null;
            SongSelectionClosed = null;
            
            StaticBus<SongSelectedEvent>.Unsubscribe(OnSongSelected);
            
            _downloadMusicCancellationToken?.Cancel();
            SongSelectionClosed?.Invoke();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void Stop()
        {
            _musicPlayerController.Stop();
        }

        private async void SetupSelectedSong()
        {
            IPlayableMusic selectedSong = null;
            var musicController = _levelManager.TargetEvent.GetMusicController();
            if (musicController != null)
            {
                var userSound = await GetUserSoundAsync(musicController);
                selectedSong = musicController.Song ?? userSound ?? musicController.ExternalTrack;
            }
            _songSelectionController.SelectedSong = selectedSong;
        }

        private void OnSongSelected(IPlayableMusic music, float activationCue)
        {
            _songSelectionController.SelectedSong = music;
            
            OnPlay(music, activationCue);
        }
        
        private void OnPlay(IPlayableMusic music, float activationCue)
        {
            if (music == null) return;
            
            var isSameMusic = _musicPlayerController.PlayedMusic?.Id == music.Id;
            if (isSameMusic)
            {
                Stop();
                return;
            }

            _musicPlayerController.PrepareMusicForPlaying(music);
            _musicPlayerController.PlayMusic(music, activationCue);
            
            _songSelectionController.SelectedSong = music;
            _songSelectionController.PlayedMusic = music;
        }
        
        private void OnPlayFromCue(IPlayableMusic music, float activationCue)
        {
            if (music == null) return;
            
            _musicPlayerController.PrepareMusicForPlaying(music);
            _musicPlayerController.PlayMusic(music, activationCue);
            
            _songSelectionController.SelectedSong = music;
            _songSelectionController.PlayedMusic = music;
        }

        private void OnSongSelectionChanged(IPlayableMusic prev, IPlayableMusic current)
        {
            GC.Collect();
            Resources.UnloadUnusedAssets();
            
            if (current == null)
            {
                _musicCueScreen.gameObject.SetActive(false);
                return;
            }
            
            _musicCueScreen.gameObject.SetActive(true);
            if (prev != null && prev.Id != current.Id) _activationCue = 0;

            _musicCueScreen.Init(_activationCue, current, OnPlay, OnPlayFromCue);
        }
        
        private void OnSongApplied(IPlayableMusic song, int activationCue)
        {
            Stop();
        }

        private async Task<IPlayableMusic> GetUserSoundAsync(MusicController musicController)
        {
            var userSound = musicController.UserSound;
            
            if (userSound == null) return null;

            var owner = userSound.Owner;

            if (owner == null)
            {
                var result = await _bridge.GetUserSoundAsync(userSound.Id, default);

                if (result.IsError)
                {
                    Debug.LogWarning($"[{GetType().Name}] Failed to get user sound # {result.ErrorMessage}");
                    return null;
                }

                if (result.IsRequestCanceled) return null;

                userSound = result.Model;
            }

            if (userSound.Owner?.Id == _userDataHolder.GroupId) return userSound;
            
            var trendingUserSound = new TrendingUserSound { UserSound = userSound, Owner = userSound.Owner };
            
            return new PlayableTrendingUserSound(trendingUserSound);
        }

        private void OnSongSelected(SongSelectedEvent @event) => OnSongSelected(@event.Playable, @event.ActivationCue);
    }
}