using System.Collections.Generic;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using Extensions;
using UIManaging.Common.Toggles;
using UIManaging.Pages.Common.FavoriteSounds;
using UIManaging.Pages.Common.SongOption.Common;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.FavoriteSounds
{
    internal sealed class FeedSoundPreviewPanel: SoundPreviewToggleBasedPanel
    {
        [SerializeField] private List<ToggleSwapBase> _playbackSwappers;
        
        [Inject] private MusicPlayerController _musicPlayerController;

        protected override void OnPlaybackChanged(bool isOn)
        {
            if (isOn)
            {
                var target = ContextData;
                if (ContextData is FavouriteMusicInfo favoriteSound && favoriteSound.Type != SoundType.ExternalSong)
                {
                    target = favoriteSound.GetSoundAsset();
                }
                
                _musicPlayerController.PrepareMusicForPlaying(target);
                _musicPlayerController.PlayMusic(target, 0);

                _musicPlayerController.PlayingSoundChanged += OnPlayingSoundChanged;
            }
            else
            {
                _musicPlayerController.PlayingSoundChanged -= OnPlayingSoundChanged;
                
                _musicPlayerController.Stop();
            }
        }

        private void OnPlayingSoundChanged(IPlayableMusic sound)
        {
            _musicPlayerController.PlayingSoundChanged -= OnPlayingSoundChanged;
            
            if (sound?.Id == ContextData.Id) return;

            _playbackSwappers.ForEach(swapper => swapper.Toggle(false));
            PlaybackToggle.SetIsOnWithoutNotify(false);
        }
    }
}