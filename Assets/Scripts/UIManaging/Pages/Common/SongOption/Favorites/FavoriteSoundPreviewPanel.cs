using System.Collections.Generic;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using StansAssets.Foundation.Patterns;
using UIManaging.Common.Toggles;
using UIManaging.Pages.Common.FavoriteSounds;
using UIManaging.Pages.Common.SongOption.Common;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Common.SongOption.Favorites
{
    internal sealed class FavoriteSoundPreviewPanel: SoundPreviewPanelBase
    {
        [SerializeField] private Button _playbackButton;
        [SerializeField] private List<ToggleSwapBase> _playabackSwappers;
        [SerializeField] private FavoriteSoundToggleHandler _favoriteSoundToggleHandler;
        
        [Inject] private SongSelectionController _songSelectionController;
        [Inject] private IBridge _bridge;

        private FavoriteSoundAdapter _favoriteSoundAdapter;
        private bool _isPlaying;

        private bool IsSelected { get; set; }

        private bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                _isPlaying = value;
                
                TogglePlaybackVisuals(_isPlaying);
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            _playbackButton.onClick.AddListener(TogglePlayback);

            _favoriteSoundAdapter = new FavoriteSoundAdapter(_bridge, ContextData as FavouriteMusicInfo);
            
            if (_songSelectionController.SelectedSong?.Id == ContextData.Id)
            {
                IsSelected = true;
                
                _songSelectionController.SelectedSongChanged += OnSelectedSongChanged;
                _songSelectionController.PlayedSongChanged += OnPlayedSongChanged;
                
                if (_songSelectionController.PlayedMusic?.Id == ContextData.Id)
                {
                    IsPlaying = true;
                }
            }
        }

        protected override void BeforeCleanUp()
        {
            base.BeforeCleanUp();
            
            _favoriteSoundAdapter.Dispose();
            
            _playabackSwappers.ForEach(swapper => swapper.Toggle(false));
            
            _playbackButton.onClick.RemoveListener(TogglePlayback);
            
            _songSelectionController.SelectedSongChanged -= OnSelectedSongChanged;
            _songSelectionController.PlayedSongChanged -= OnPlayedSongChanged;
        }
        
        private async void TogglePlayback()
        {
            if (_favoriteSoundAdapter.TargetSound == null)
            {
                await _favoriteSoundAdapter.GetTargetSoundAsync();
            }
            
            var eventModel = _favoriteSoundAdapter.FavoriteSound.Type == SoundType.UserSound
                ? ContextData
                : _favoriteSoundAdapter.TargetSound;
            
            StaticBus<SongSelectedEvent>.Post(new SongSelectedEvent(eventModel));

            if (IsSelected) return;

            IsSelected = true;
            IsPlaying = true;
            
            _songSelectionController.SelectedSongChanged += OnSelectedSongChanged;
            _songSelectionController.PlayedSongChanged += OnPlayedSongChanged;
        }

        private void TogglePlaybackVisuals(bool isOn)
        {
            _playabackSwappers.ForEach(swap => swap.Toggle(isOn));
        }
        
        private void OnPlayedSongChanged(IPlayableMusic from, IPlayableMusic to)
        {
            IsPlaying = _favoriteSoundAdapter?.TargetSound != null && to?.Id == _favoriteSoundAdapter.TargetSound.Id;
        }

        private void OnSelectedSongChanged(IPlayableMusic from, IPlayableMusic to)
        {
            IsSelected = false;
            IsPlaying = false;
            
            _songSelectionController.SelectedSongChanged -= OnSelectedSongChanged;
            _songSelectionController.PlayedSongChanged -= OnPlayedSongChanged;
        }
    }
}