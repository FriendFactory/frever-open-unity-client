using UnityEngine;
using Zenject;

namespace Modules.Sound
{
    public sealed class AutoplaySoundTrigger: MonoBehaviour
    {
        private enum AutoplayType
        {
            Manual,
            OnAwake,
            OnEnable,
        }
        
        [SerializeField] private SoundType _soundType;
        [SerializeField] private MixerChannel _mixerChannel = MixerChannel.SpecialEffects;
        [SerializeField] private AutoplayType _autoplayType = AutoplayType.OnAwake;
        [SerializeField] private bool _playOnce;
        
        [Inject] private SoundManager _soundManager;
        
        private bool _hasPlayed;
        
        private void Awake()
        {
            if (_autoplayType == AutoplayType.OnAwake)
            {
                PlaySound();
            }
        }
        
        private void OnEnable()
        {
            if (_autoplayType == AutoplayType.OnEnable)
            {
                PlaySound();
            }
        }

        public void PlaySound()
        {
            if (_playOnce && _hasPlayed) return;
            
            _hasPlayed = true;
            
            _soundManager.Play(_soundType, _mixerChannel);
        }
    }
}