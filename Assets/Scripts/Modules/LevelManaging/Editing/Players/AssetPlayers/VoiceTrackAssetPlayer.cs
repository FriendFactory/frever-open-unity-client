using Modules.LevelManaging.Assets;
using UnityEngine;

namespace Modules.LevelManaging.Editing.Players.AssetPlayers
{
    internal sealed class VoiceTrackAssetPlayer: GenericTimeDependAssetPlayer<IVoiceTrackAsset>
    {
        private AudioSource _audioSource;
        private float _volume;
        
        public void SetAudioSource(AudioSource audioSource)
        {
            _audioSource = audioSource;
        }
        
        public void SetVoiceSoundVolume(float value)
        {
            _volume = value;
        }

        public override void Simulate(float time)
        {
        }

        protected override void OnPlay()
        {
            _audioSource.clip = Target.VoiceClip.AudioClip;
            _audioSource.time = 0;
            _audioSource.volume = _volume;
            _audioSource.Play();
        }

        protected override void OnPause()
        {
            _audioSource.Pause();
        }

        protected override void OnResume()
        {
            Play();
        }

        protected override void OnStop()
        {
            _audioSource.Stop();
        }
    }
}