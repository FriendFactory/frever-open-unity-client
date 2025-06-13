using System.Collections;
using Coffee.UIExtensions;
using UIManaging.Common.RankBadge;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Animated
{
    public class AnimatedCreatorScoreLevel : AnimatedUserLevel
    {
        [SerializeField] private Image _currentBadge;
        [SerializeField] private Image _temporaryBadge;
        [SerializeField] private UIParticle _confettiParticle;
        
        [Inject] private RankBadgeManager _rankBadgeManager; 
        
        public override void Animate(int previousLevel, int currentLevel)
        {
            _temporaryBadge.sprite = _rankBadgeManager.GetBadgeSprite(currentLevel);
            
            _currentBadge.sprite =  _rankBadgeManager.GetBadgeSprite(previousLevel);
            _currentBadge.color = previousLevel > 0 ? Color.white : Color.clear;

            if (_hasAnimator) _animator.SetTrigger(START);
            
            DelayedUpdateUserLevelText(currentLevel);
        }

        protected override IEnumerator DelayedUpdateUserLevelTextRoutine(int userLevel)
        {
            yield return new WaitForSecondsRealtime(_animationTime);
            
            _temporaryBadge.sprite = _currentBadge.sprite = _rankBadgeManager.GetBadgeSprite(userLevel);
            _currentBadge.color = userLevel > 0 ? Color.white : Color.clear;
            _coroutine = null;

            yield return new WaitForSeconds(_additionalAnimationTime);

            _confettiParticle.Play();
            
            InvokeAnimationFinished();
        }
    }
}