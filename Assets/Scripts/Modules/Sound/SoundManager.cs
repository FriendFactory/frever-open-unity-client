using System;
using Common.Pools;
using Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;
using Zenject;

namespace Modules.Sound
{
    public sealed class SoundManager : MonoBehaviour, ISoundManager
    {
        private const string BUTTON_VOLUME = "ButtonVolume";
        private const string SFX_VOLUME = "SfxVolume";
        [Inject]
        private SoundBank _bank;

        [SerializeField] 
        private AudioMixer _mixer;
        [SerializeField] 
        private AudioMixerGroup _buttonGroup;
        [SerializeField] 
        private AudioMixerGroup _specialEffectsGroup;

        private IPool<SoundController> _soundControllerPool;

        private void Awake()
        {
            _soundControllerPool = new SoundControllerPool();
        }

        private void Start()
        {
            RestoreAudioSettings();
        }

        private void OnDestroy()
        {
            _soundControllerPool.Clear();
        }

        [Button]
        public void Play(SoundType soundType, MixerChannel mixerChannel)
        {
            var mixerGroup = MixerChannelToMixerGroup(mixerChannel);

            if (!_bank.AudioClipOfType(soundType, out var clip)) return;
            
            AvailableSoundSource().Play(clip, mixerGroup);
        }

        [Button]
        public void MuteChannel(bool mute, MixerChannel mixerChannel)
        {
            var targetVolume = mute ? -80.0f : 0.0f;
            var paramName = GetVolumeParameterNameFor(mixerChannel);
            
            PlayerPrefs.SetFloat(paramName, targetVolume);
            _mixer.SetFloat(paramName, targetVolume);
        }

        public bool IsChannelMuted(MixerChannel mixerChannel)
        {
            var prefKey = GetVolumeParameterNameFor(mixerChannel);
            return PlayerPrefs.GetFloat(prefKey, 0.0f) == -80.0f;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private SoundController AvailableSoundSource()
        {
            var controller = _soundControllerPool.Get();
           controller.Finished += OnPlaybackFinished;
           
           return controller;
        }

        private void OnPlaybackFinished(SoundController soundController)
        {
            soundController.Finished -= OnPlaybackFinished;
            soundController.MarkAsUsed();
        }

        private AudioMixerGroup MixerChannelToMixerGroup(MixerChannel channel)
        {
            switch (channel)
            {
                case MixerChannel.Button:
                    return _buttonGroup;
                case MixerChannel.SpecialEffects:
                    return _specialEffectsGroup;
                default:
                    throw new NotImplementedException();
            }
        }

        private string GetVolumeParameterNameFor(MixerChannel mixerChannel)
        {
            switch (mixerChannel)
            {
                case MixerChannel.Button:
                    return BUTTON_VOLUME;
                case MixerChannel.SpecialEffects:
                    return SFX_VOLUME;
                default:
                    throw new NotImplementedException();
            }
        }

        private void RestoreAudioSettings()
        {
            ((MixerChannel[])Enum.GetValues(typeof(MixerChannel))).ForEach(RestoreChannelSettings);
        }

        private void RestoreChannelSettings(MixerChannel mixerChannel)
        {
            var paramName = GetVolumeParameterNameFor(mixerChannel);
            var targetVolume = PlayerPrefs.GetFloat(paramName, 0.0f);
            _mixer.SetFloat(paramName, targetVolume);
        }
    }
}