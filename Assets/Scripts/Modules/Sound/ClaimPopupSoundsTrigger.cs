using System;
using System.Collections;
using UnityEngine;

namespace Modules.Sound
{
    public sealed class ClaimPopupSoundsTrigger
    {
        private const float TIME_TILL_REWARDS_SHOW_UP = 0.41f;

        private ISoundManager _soundManager;
        private WaitForSecondsRealtime _waitForRewardToShowUp = new WaitForSecondsRealtime(TIME_TILL_REWARDS_SHOW_UP);

        public ClaimPopupSoundsTrigger(ISoundManager soundManager)
        {
            _soundManager = soundManager;
        }

        public void PlayClaimPopupSound()
        {
            _soundManager.Play(SoundType.ClaimPopup, MixerChannel.Button);
        }
        
        public IEnumerator PlayRewardAnimationSounds(Action onCompleted)
        {
            yield return _waitForRewardToShowUp;
            
            _soundManager.Play(SoundType.WhooshCurrency, MixerChannel.SpecialEffects);
            _soundManager.Play(SoundType.Applause, MixerChannel.SpecialEffects);

            onCompleted?.Invoke();
        }
    }
}