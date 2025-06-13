using System;
using System.Collections;
using Modules.Sound;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace UIManaging.Pages.Tasks.RewardPopUp
{
    public class FlyingRewardsAnimationController : MonoBehaviour
    {
        [Inject] private ISoundManager _soundManager;
        
        [SerializeField] private Animator _softCurrencyAnimator;
        [SerializeField] private Animator _hardCurrencyAnimator;
        [SerializeField] private Animator _experienceAnimator;
        [FormerlySerializedAs("_xpBarDelay"), SerializeField] 
        private float _timeTillFirstElementWillReachTarget = 1.5f;
        [SerializeField] private float _timeTillLastElementWillReachTarget = 2f;

        public event Action FirstElementReachedTarget;
        public event Action LastElementReachedTarget;
        
        private Coroutine _waitForElementsRoutine;
        private static readonly int START = Animator.StringToHash("Start");

        private void OnDisable()
        {
            if(_waitForElementsRoutine != null)
                StopCoroutine(_waitForElementsRoutine);
        }

        public void Play(bool softCurrencyAnimation, bool hardCurrencyAnimation, bool experienceAnimation)
        {
            if (softCurrencyAnimation)
            {
                _softCurrencyAnimator.SetTrigger(START);
            }

            if (hardCurrencyAnimation)
            {
                _hardCurrencyAnimator.SetTrigger(START);
            }

            if (experienceAnimation)
            {
                _experienceAnimator.SetTrigger(START);
            }
            
            PlaySoundEffects(experienceAnimation || hardCurrencyAnimation, softCurrencyAnimation);
            
            WaitForElementsToReachTarget();
        }
        
        private void WaitForElementsToReachTarget()
        {
            if (_waitForElementsRoutine != null)
            {
                StopCoroutine(_waitForElementsRoutine);
            }
            
            _waitForElementsRoutine = StartCoroutine(WaitForElementsToReachTargetRoutine());
        }

        private IEnumerator WaitForElementsToReachTargetRoutine()
        {
            yield return new WaitForSecondsRealtime(_timeTillFirstElementWillReachTarget);
            
            FirstElementReachedTarget?.Invoke();

            yield return new WaitForSecondsRealtime(_timeTillLastElementWillReachTarget - _timeTillFirstElementWillReachTarget);
            
            LastElementReachedTarget?.Invoke();
            
            _waitForElementsRoutine = null;
        }

        private void PlaySoundEffects(bool playXpGemSFX, bool playCoinsSFX)
        {
            _soundManager.Play(SoundType.WhooshCurrency, MixerChannel.SpecialEffects);
            
            if (playXpGemSFX) _soundManager.Play(SoundType.GemXp, MixerChannel.SpecialEffects);
            
            if (playCoinsSFX) _soundManager.Play(SoundType.Coins, MixerChannel.SpecialEffects);
        }
    }
}