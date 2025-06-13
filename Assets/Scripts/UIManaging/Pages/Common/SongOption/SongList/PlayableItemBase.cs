using System;
using System.Threading;
using Abstract;
using Bridge.Models.Common;
using Extensions;
using TMPro;
using UIManaging.Pages.Common.FavoriteSounds;
using UIManaging.Pages.Common.Helpers;
using UIManaging.Pages.Common.SongOption.Common;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Common.SongOption.SongList
{
    public abstract class PlayableItemBase<TModel> : BaseContextDataButton<TModel> where TModel: PlayableItemModel
    {
        [SerializeField] protected TMP_Text _artistName;
        [SerializeField] protected TMP_Text _labelName;
        [SerializeField] protected TextMeshProUGUI _songLength;
        [SerializeField] protected RawImage _thumbnailImage;
        [SerializeField] private FavoriteSoundToggleHandler _favoriteSoundToggleHandler;
        [Header("Playback")]
        [SerializeField] private GameObject[] _playObjs;
        [SerializeField] private GameObject[] _stopObjs;
        
        [Inject] protected MusicDownloadHelper MusicDownloadHelper;
        [Inject] private SongSelectionController _songSelectionController;
        [Inject] private SoundsFavoriteStatusCache _favoriteStatusCache;
        
        protected CancellationTokenSource CancellationTokenSource;
        
        private bool _thumbnailLoaded;
        
        protected abstract IPlayableMusic Music { get; } 

        private bool IsSelected { get; set; }
        private bool IsPlayed { get; set; }

        private void Awake()
        {
            SetPlay(false);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            // there is no mechanism in scroller delegate to control behaviour on show/hide
            // that's why some of the control logic relies on Unity internal events
            if (ContextData == null) return;

            SetSelected(_songSelectionController.SelectedSong?.Id == ContextData.Id);
            SetPlay(false);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (_thumbnailLoaded) Destroy(_thumbnailImage.texture);
            CancellationTokenSource?.Cancel();
            CancellationTokenSource?.Dispose();
        }
        
        protected abstract void DownloadThumbnail();

        protected override void OnInitialized()
        {
            _button.enabled = true;
            if (_songSelectionController.SelectedSong?.Id == ContextData.Id)
            {
                SetSelected(true);
                _songSelectionController.SelectedSongChanged += SelectedSongChanged;
                _songSelectionController.PlayedSongChanged += OnPlayedSongChanged;
                
                if (_songSelectionController.PlayedMusic?.Id == ContextData.Id)
                {
                    SetPlay(true);
                }
            }
            
            _labelName.text = ContextData.Title;
            _artistName.text = ContextData.ArtistName;
            var timeSpan = TimeSpan.FromMilliseconds(ContextData.Duration);
            _songLength.text = timeSpan.ToString(@"mm\:ss");
            
            DownloadThumbnail();

            var sound = ContextData.Music is PlayableTrendingUserSound trendingUserSound
                ? trendingUserSound.UserSound
                : ContextData.Music;
            
            _favoriteStatusCache.AddToCacheIfNeeded(sound);

            var isFavorite = _favoriteStatusCache.IsFavorite(sound);

            _favoriteSoundToggleHandler.Initialize(new SoundItemModel(sound, isFavorite));
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
        
            _artistName.text = string.Empty;
            _labelName.text = string.Empty;
            _songLength.text = string.Empty;

            IsSelected = false;
            _playObjs.SetAllActive(false);
            _stopObjs.SetAllActive(true);

            _songSelectionController.SelectedSongChanged -= SelectedSongChanged;
            _songSelectionController.PlayedSongChanged -= OnPlayedSongChanged;
            
            if (_thumbnailLoaded)
            {
                Destroy(_thumbnailImage.texture);
                _thumbnailImage.texture = null;
                _thumbnailLoaded = false;
            }
            
            _favoriteSoundToggleHandler.CleanUp();

            _button.enabled = false;
        }

        protected override void OnUIInteracted()
        {
            base.OnUIInteracted();
            OnClickPreview();
        }

        protected void OnThumbnailLoaded(Texture texture)
        {
            if (_thumbnailImage == null ) return;
            _thumbnailLoaded = true;
            _thumbnailImage.texture =  texture;
        }

        protected virtual void OnThumbnailLoadingFailed(string error)
        {
            Debug.LogWarning(error);
        }

        private void SetPlay(bool isPlaying)
        {
            IsPlayed = isPlaying;
            _stopObjs.SetAllActive(!IsPlayed);
            _playObjs.SetAllActive(IsPlayed);
        }

        private void SetSelected(bool isSelected)
        {
            IsSelected = isSelected;

            if (!IsSelected)
            {
                _stopObjs.SetAllActive(false);
                _playObjs.SetAllActive(false);
            }
        }

        private void OnClickPreview()
        {
            ContextData.TogglePlay();

            if (IsSelected) return;
            
            SetSelected(true);
            SetPlay(true);
            _songSelectionController.SelectedSongChanged += SelectedSongChanged;
            _songSelectionController.PlayedSongChanged += OnPlayedSongChanged;
        }

        private void SelectedSongChanged(IPlayableMusic from, IPlayableMusic to)
        {
            SetSelected(false);
            SetPlay(false);
            _songSelectionController.SelectedSongChanged -= SelectedSongChanged;
            _songSelectionController.PlayedSongChanged -= OnPlayedSongChanged;
        }

        private void OnPlayedSongChanged(IPlayableMusic from, IPlayableMusic to)
        {
            SetPlay(to?.Id == ContextData.Id);
        }
    }
}