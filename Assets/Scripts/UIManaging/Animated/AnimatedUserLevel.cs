using System;
using System.Collections;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace UIManaging.Animated
{
    public class AnimatedUserLevel : MonoBehaviour
    {
        [SerializeField] protected bool _hasAnimator;
        [SerializeField, ShowIf("_hasAnimator")]
        protected Animator _animator;
        
        [Space(10)]
        [SerializeField]
        protected float _animationTime = 2.0f;
        [FormerlySerializedAs("_fadeOutTime")] [SerializeField]
        protected float _additionalAnimationTime;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _temporaryLevelText;

        public event Action AnimationFinished;

        protected Coroutine _coroutine;

        protected static readonly int START = Animator.StringToHash("Start");

        private void OnDisable()
        {
            if (_coroutine != null)
                StopCoroutine(_coroutine);
        }

        public virtual void Animate(int previousLevel, int currentLevel)
        {
            _temporaryLevelText.text = currentLevel.ToString();
            _levelText.text = previousLevel.ToString();

            if(_hasAnimator)
                _animator.SetTrigger(START);
            
            DelayedUpdateUserLevelText(currentLevel);
        }

        protected void DelayedUpdateUserLevelText(int userLevel)
        {
            if (_coroutine != null)
                StopCoroutine(_coroutine);

            _coroutine = StartCoroutine(DelayedUpdateUserLevelTextRoutine(userLevel));
        }

        protected virtual IEnumerator DelayedUpdateUserLevelTextRoutine(int userLevel)
        {
            yield return new WaitForSecondsRealtime(_animationTime);

            _temporaryLevelText.text = userLevel.ToString();
            _levelText.text = userLevel.ToString();
            _coroutine = null;

            yield return new WaitForSeconds(_additionalAnimationTime);

            InvokeAnimationFinished();
        }
        
        protected void InvokeAnimationFinished()
        {
            AnimationFinished?.Invoke();
        }
    }
}