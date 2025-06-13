using System;
using System.Threading;
using Bridge;
using Bridge.Models.Common;
using Bridge.Models.ClientServer.Assets;
using Bridge.Services._7Digital.Models.TrackModels;
using Extensions;
using Modules.Amplitude.Events;
using Modules.Amplitude.Signals;
using UIManaging.Pages.Common.Helpers;
using UIManaging.Pages.Common.SongOption.Common;
using UIManaging.Pages.Common.UsersManagement.BlockedUsersManaging;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.Common.SongOption.MusicCue
{
    public sealed class MusicCueScreen : BaseSongOptionScreen
    {
        private const float MAX_AUDIO_CLIP_LENGTH = 30f;
        
        [SerializeField] private MusicCueSlider _musicCueSlider;
        [SerializeField] private Button _applyButton;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private RawImage _thumbnailImage;
        [SerializeField] private Texture2D _defaultThumbnail;
        [SerializeField] private GameObject _thumbnailPauseImage;
        [SerializeField] private GameObject _thumbnailPlayImage;
        [SerializeField] private Toggle _playButton;
        [SerializeField] private MusicCueSoundPreviewInfoPanel _songInfoPanel;
        
        [Inject] private SongSelectionController _songSelectionController;
        [Inject] private MusicDownloadHelper _musicDownloadHelper;
        [Inject] private SongPlayer _songPlayer;
        [Inject] private CharacterThumbnailsDownloader _characterThumbnailsDownloader;
        [Inject] private IBridge _bridge; 
        [Inject] private IBlockedAccountsManager _bLockedAccountsManager;
        [Inject] private SignalBus _signalBus;

        private IPlayableMusic _currentSong;
        private AudioClip _currentAudioClip;
        private int _activationCue;
        private Action<IPlayableMusic, float> _onPlay;
        private Action<IPlayableMusic, float> _onPlayFromCue;
        private bool _playCursor;
        
        private CancellationTokenSource _downloadSongCancellationToken;
        private CancellationTokenSource _downloadThumbnailCancellationToken;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _applyButton.onClick.AddListener(OnApply);
            _cancelButton.onClick.AddListener(OnCancel);
            _playButton.onValueChanged.AddListener(OnPlay);
            _songSelectionController.PlayedSongChanged += OnPlayedSongChanged;
        }

        private void Update()
        {
            if (_playCursor == false) return;
            var currentTime = _songPlayer.GetCurrentTime();
            _musicCueSlider.UpdatePlayerTime(currentTime);
        }

        private void OnDisable()
        {
            _downloadSongCancellationToken?.Cancel();
            _downloadThumbnailCancellationToken?.Cancel();
        }

        private void OnDestroy()
        {
            _applyButton.onClick.RemoveListener(OnApply);
            _cancelButton.onClick.RemoveListener(OnCancel);
            _playButton.onValueChanged.RemoveListener(OnPlay);
            _songSelectionController.PlayedSongChanged -= OnPlayedSongChanged;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Init(int activationCue, IPlayableMusic song, Action<IPlayableMusic, float> onPlay, Action<IPlayableMusic, float> onPlayFromCue)
        {
            var isSameSong = _currentSong != null && _currentSong.Id == song.Id;
            if (isSameSong) return;
            
            _onPlay = onPlay;
            _onPlayFromCue = onPlayFromCue;
            _activationCue = activationCue;
            _playButton.isOn = false;
            _currentSong = song;
            _applyButton.interactable = false;
            
            TogglePlayImage(true);

            if (_currentSong == null) return;

            _thumbnailImage.texture = null;
            
            _songInfoPanel.Initialize(new SoundPreviewInfoPanelModel(song));

            switch (_currentSong)
            {
                case SongInfo songInfo:
                    SetupSong(songInfo);
                    break;
                case UserSoundFullInfo userSound:
                    SetupUserSound(userSound);
                    break;
                case PlayableTrendingUserSound trendingUserSound:
                    SetupTrendingOUserSound(trendingUserSound);
                    break;
                case ExternalTrackInfo track:
                    SetupExternalTrack(track);
                    break;
                case FavouriteMusicInfo favoriteSound:
                    SetupFavoriteSound(favoriteSound);
                    break;
            }
        }

        public void UpdateSliderCue(int activationCue)
        {
            _musicCueSlider.UpdatePlayerTime(activationCue.ToSeconds());
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void SetupUserSound(UserSoundFullInfo userSound)
        {
            var groupId = _bridge.Profile.GroupId; 
            
            DownloadSong(userSound);
            DownloadCharacterThumbnail(groupId);
        }

        private void SetupTrendingOUserSound(PlayableTrendingUserSound trendingUserSound)
        {
            var sound = trendingUserSound.UserSound;
            var owner = trendingUserSound.Owner;
            
            _currentSong = sound;
            
            DownloadSong(sound);
            DownloadCharacterThumbnail(owner.Id);
        }

        private void SetupSong(SongInfo song)
        {
            DownloadSong(song);
            DownloadThumbnail(song);
        }

        private void SetupFavoriteSound(FavouriteMusicInfo favoriteSound)
        {
            _currentSong = favoriteSound.GetSoundAsset();
  
            DownloadSong(_currentSong);
            _musicDownloadHelper.DownloadFavoriteSoundThumbnail(favoriteSound, OnThumbnailLoaded, (x)=>OnFail(x, _currentSong.Id));
        }
        
        private void DownloadSong(IFilesAttachedEntity song)
        {
            _downloadSongCancellationToken = new CancellationTokenSource();
            _musicDownloadHelper.DownloadMusic(song, OnMusicLoaded, (x)=>OnFail(x, song.Id), _downloadSongCancellationToken.Token);
        }

        private void DownloadThumbnail(IFilesAttachedEntity song)
        {
            _musicDownloadHelper.DownloadThumbnail(song, Resolution._128x128, OnThumbnailLoaded, (x)=>OnFail(x, song.Id));
        }

        private void DownloadCharacterThumbnail(long groupId)
        {
            _downloadThumbnailCancellationToken = new CancellationTokenSource();
            _characterThumbnailsDownloader.GetCharacterThumbnailByUserGroupId(groupId, Resolution._128x128, OnThumbnailLoaded, OnThumbnailLoadingFailed, _downloadThumbnailCancellationToken.Token);

            void OnThumbnailLoadingFailed(string errorMessage)
            {
                Debug.LogWarning($"[{GetType().Name}] Failed to load character thumbnail with groupId {groupId} # {errorMessage}");
                _thumbnailImage.texture = _defaultThumbnail;
            }
        }
        
        private void SetupExternalTrack(ExternalTrackInfo track)
        {
            _downloadSongCancellationToken = new CancellationTokenSource();
            _musicDownloadHelper.DownloadMusic(track.Id, clip => OnExternalTrackDownloaded(clip, track.Id), (x)=>OnFail(x, track.Id), _downloadSongCancellationToken.Token);
            _musicDownloadHelper.DownloadThumbnail(track, OnThumbnailLoaded, (x)=>OnFail(x, track.Id));
        }

        private void OnExternalTrackDownloaded(AudioClip clip, long trackId)
        {
            var amplitudeEvent = new ExternalTrackDownloadedEvent(trackId);
            
            _signalBus.Fire(new AmplitudeEventSignal(amplitudeEvent));

            OnMusicLoaded(clip);
        }

        private void OnMusicLoaded(AudioClip clip)
        {
            _activationCue = 0;
            _currentAudioClip = clip;
            var length = _currentSong.Duration == 0
                ? Mathf.Min(_currentAudioClip.length, MAX_AUDIO_CLIP_LENGTH)
                : Mathf.Min(_currentAudioClip.length, _currentSong.Duration / 1000f);
            
            _musicCueSlider.Init( _activationCue, PlayWithCue, length);
            _applyButton.interactable = true;
        }
        
        private void OnThumbnailLoaded(Texture2D texture)
        {
            _thumbnailImage.texture = texture;
        }
        
        private void OnFail(string obj, long songId)
        {
            Debug.LogError($"Fail to load music {songId}: {obj}");
        }

        private void OnApply()
        {
            _activationCue = _musicCueSlider.GetActivationCue();
            if (_currentSong != null && _currentSong.IsLicensed())
            {
                _musicDownloadHelper.KeepInCache(_currentSong.Id, _currentAudioClip);
            }
            _songSelectionController.ApplySong(_currentSong, _activationCue);
        }

        private void OnCancel()
        {
            _songSelectionController.ApplySong(null, -1);
            _songPlayer.Stop();
            ResetValues();
            gameObject.SetActive(false);
        }

        private void OnPlay(bool value)
        {
            var activationCue = GetActivationCue();
            
            if(_currentSong == null) return;

            _onPlay(_currentSong, activationCue);
        }

        private void PlayWithCue()
        {
            var activationCue = GetActivationCue();
            
            if(_currentSong == null) return;

            _onPlayFromCue(_currentSong, activationCue);
        }

        private float GetActivationCue()
        {
            var activationCue = _musicCueSlider.GetActivationCue().ToSeconds();
            _musicCueSlider.UpdatePlayerTime(activationCue);

            return activationCue;
        }
        
        private void OnPlayedSongChanged(IPlayableMusic previousSong, IPlayableMusic currentSong)
        {
            if (currentSong == null)
            {
                TogglePlayImage(true);
            }
            else if(_currentSong?.Id == currentSong.Id)
            {
                TogglePlayImage(false);
            }
        }

        private void TogglePlayImage(bool val)
        {
            _thumbnailPlayImage.SetActive(val);
            _thumbnailPauseImage.SetActive(!val);
            _playCursor = !val;
        }

        private void ResetValues()
        {
            _currentSong = null;
            _currentAudioClip = null;
            _activationCue = 0;
        }
    }
}
