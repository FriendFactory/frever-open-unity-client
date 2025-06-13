using System;
using Bridge.Models.Common;
using JetBrains.Annotations;
using Modules.LevelManaging.Assets.AssetDependencies;
using UIManaging.Pages.Common.SongOption.Common;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Common.SongOption.AudioMixer
{
    public class AudioMixerScreen : BaseSongOptionScreen
    {
        [SerializeField] private SongOptionScreenSwitcher _songOptionScreenSwitcher;

        [SerializeField] private Button _applyButton;
        [SerializeField] private Slider _voiceVolumeSlider;
        [SerializeField] private Slider _musicVolumeSlider;

        private AudioSource _characterAudioSource;
        private AudioSource _songAudioSource;
        private AudioSource _songPreviewAudioSource;
        private SongSelectionController _songSelectionController;
        private MusicPlayerController _musicPlayerController;
        
        private Action<IPlayableMusic, float> _onPlay;
        private IPlayableMusic _prevSong;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject, UsedImplicitly]
        private void Construct(AudioSourceManager audioManager, SongSelectionController songSelectionController,
            MusicPlayerController musicPlayerController)
        {
            _characterAudioSource = audioManager.CharacterAudioSource;
            _songAudioSource = audioManager.SongAudioSource;
            _songPreviewAudioSource = audioManager.SongPreviewAudioSource;
            _songSelectionController = songSelectionController;
            _musicPlayerController = musicPlayerController;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Start()
        {
            _applyButton.onClick.AddListener(ApplyVolume);
            _voiceVolumeSlider.onValueChanged.AddListener(VoiceVolumeChanged);
            _musicVolumeSlider.onValueChanged.AddListener(MusicVolumeChanged);
        }

        private void OnEnable()
        {
            _voiceVolumeSlider.value = _characterAudioSource != null ? _characterAudioSource.volume : 1;
            _musicVolumeSlider.value = _songAudioSource.volume;

            _prevSong = _songSelectionController.PlayedMusic ?? _songSelectionController.SelectedSong;
            _musicPlayerController.Stop();

            if (_prevSong != null ) _onPlay?.Invoke(_prevSong, 0);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Init(Action<IPlayableMusic, float> onPlay)
        {
            _onPlay = onPlay;

            if (_characterAudioSource != null) _characterAudioSource.volume = 1;
            _voiceVolumeSlider.value = _characterAudioSource != null ? _characterAudioSource.volume : 1;

            if (_songSelectionController.SelectedSong == null)
            {
                _songAudioSource.volume = 1;
                _songPreviewAudioSource.volume = 1;
            }

            _musicVolumeSlider.value = _songAudioSource.volume;
        }

        public int GetVoiceVolume()
        {
            var val = (int) (_voiceVolumeSlider.value * 100f - 1);
            return Mathf.Clamp(val, 0, 99);
        }

        public int GetMusicVolume()
        {
            var val = (int) (_musicVolumeSlider.value * 100f - 1);
            return Mathf.Clamp(val, 0, 99);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void MusicVolumeChanged(float volume)
        {
            _songAudioSource.volume = volume;
            _songPreviewAudioSource.volume = volume;
        }

        private void VoiceVolumeChanged(float volume)
        {
            if (_characterAudioSource != null) _characterAudioSource.volume = volume;
        }

        private void ApplyVolume()
        {
            _songOptionScreenSwitcher.Show(null);
        }
    }
}
