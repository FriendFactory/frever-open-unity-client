using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using Bridge.Services._7Digital.Models.TrackModels;
using Common;
using Common.Collections;
using Extensions;
using Modules.Amplitude;
using JetBrains.Annotations;
using UIManaging.Pages.Common.Helpers;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace UIManaging.Pages.Common.SongOption.Common
{
    [UsedImplicitly]
    public class MusicPlayerController
    {
        private readonly SongPlayer _songPlayer;
        private readonly SongSelectionController _songSelectionController;
        private readonly MusicDownloadHelper _musicDownloadHelper;
        private readonly AmplitudeManager _amplitudeManager;
        private readonly StopWatchProvider _stopWatchProvider;

        private readonly DictionaryCache<long, object> _playablesCache;

        private Stopwatch _stopWatch;
        private IPlayableMusic _playedMusic;
        private CancellationTokenSource _downloadTokenSource;

        public IPlayableMusic PlayedMusic
        {
            get => _playedMusic;
            private set
            {
                if (_playedMusic == value) return;
                
                _playedMusic = value;
                
                PlayingSoundChanged?.Invoke(_playedMusic);
            }
        }

        public float CurrentTime => _songPlayer.GetCurrentTime();

        public event Action SongPlayingStarted;
        public event Action SongPlayingStopped;
        public event Action<IPlayableMusic> PlayingSoundChanged;
        public event Action<IPlayableMusic, float> MusicDownloaded;
        
        public MusicPlayerController(SongPlayer songPlayer, SongSelectionController songSelectionController, MusicDownloadHelper musicDownloadHelper, AmplitudeManager amplitudeManager, StopWatchProvider stopWatchProvider)
        {
            _songPlayer = songPlayer;
            _songSelectionController = songSelectionController;
            _musicDownloadHelper = musicDownloadHelper;
            _amplitudeManager = amplitudeManager;
            _stopWatchProvider = stopWatchProvider;
            
            _playablesCache = new DictionaryCache<long, object>();

            _songSelectionController.SelectedSongChanged += OnSelectedSongChanged;
        }

        public void PrepareMusicForPlaying(IPlayableMusic music)
        {
            if (PlayedMusic != null)
            {
                _songPlayer.Stop();
            }

            PlayedMusic = music;
            _stopWatch = _stopWatchProvider.GetStopWatch();
            _stopWatch.Restart();
        }

        public void PlayMusic(IPlayableMusic music, float activationCue, bool loop = false)
        {
            _downloadTokenSource = new CancellationTokenSource();
            
            // TrendingUserSounds model does not supports IPlayableMusic (implemented on client side),
            // so, we need to get associated UserSoundFullInfo model before downloading 
            if (music is PlayableTrendingUserSound trendingUserSound)
            {
                music = trendingUserSound.UserSound;
            }

            if (_playablesCache.TryGet(music.Id, out var playable))
            {
                Play(playable, activationCue, loop);
            }
            else if (music is ExternalTrackInfo)
            {
                _musicDownloadHelper.DownloadMusic(music.Id, OnExternalTrackDownloaded, OnFail, _downloadTokenSource.Token);
            }
            else if (music is FavouriteMusicInfo favoriteSound)
            {
                if (favoriteSound.Type == SoundType.ExternalSong)
                {
                    _musicDownloadHelper.DownloadMusic(favoriteSound.Id, OnMusicDownloaded, OnFail, _downloadTokenSource.Token);
                }
                else
                {
                    music = favoriteSound.GetSoundAsset();
                    _musicDownloadHelper.DownloadMusic(music, OnMusicDownloaded, OnFail, _downloadTokenSource.Token);
                }
            }
            else
            {
                _musicDownloadHelper.DownloadMusic(music, OnMusicDownloaded, OnFail, _downloadTokenSource.Token);
            }
            
            void OnExternalTrackDownloaded(AudioClip clip)
            {
                _playablesCache.Add(music.Id, clip);
                
                PlayLoadedMusic(music.Id, activationCue, loop);
            }

            void OnMusicDownloaded(AudioClip clip)
            {
                _playablesCache.Add(music.Id, clip);
                
                MusicDownloaded?.Invoke(music, _stopWatch.ElapsedMilliseconds.ToSeconds());
                
                _stopWatch.Stop();
                _stopWatchProvider.Dispose(_stopWatch);
                
                PlayLoadedMusic(music.Id, activationCue, loop);
            }

            void OnFail(string errorMessage)
            {
                Debug.LogError($"Fail to play music: {music.Id}. [Reason]: {errorMessage}");
            }
        }

        public void Stop()
        {
            _downloadTokenSource?.CancelAndDispose();
            _downloadTokenSource = null;
            
            PlayedMusic = null;
            _songPlayer.Stop();
            _songSelectionController.PlayedMusic = null;
        }

        public void Pause()
        {
            _songPlayer.Stop();
        }
        
        public void ClearCache()
        {
            _playablesCache.Clear();
        }
        
        public void SetVolume(float volume)
        {
            _songPlayer.SetVolume(volume);
        }

        private void PlayLoadedMusic(long audioId, float activationCue, bool loop = false)
        {
            if (audioId != _playedMusic?.Id) return;
            
            if (!_playablesCache.TryGet(audioId, out var playable)) return;
            
            Play(playable, activationCue, loop);
        }

        private void Play(object obj, float activationCue, bool loop = false)
        {
            SongPlayingStarted?.Invoke();
            _songPlayer.PlayForSeconds((AudioClip)obj, activationCue, OnStop, loop: loop);
            
            void OnStop()
            {
                Stop();
                SongPlayingStopped?.Invoke();
            }
        }

        private void OnSelectedSongChanged(IPlayableMusic prev, IPlayableMusic current)
        {
            // looks weird, because in such case cached item is removed from cache right after next song selection
            if(prev != null) _playablesCache.Remove(prev.Id);
        }
    }
}