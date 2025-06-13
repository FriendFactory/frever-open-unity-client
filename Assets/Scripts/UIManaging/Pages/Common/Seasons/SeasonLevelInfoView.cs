using System;
using Sirenix.OdinInspector;
using TMPro;
using UIManaging.Animated.Behaviours;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Common.Seasons
{
    public class SeasonLevelInfoView : AnimatedSliderView<ISeasonLevelInfoModel>
    {
        [SerializeField] private AnimatedLevelIconBehaviour _levelIconAnimation;
        [SerializeField] private Image _sliderFill;
        [Space] 
        [SerializeField] private TMP_Text _userLevel;
        [SerializeField] private ParticleSystem _fireworksVFX;

        public event Action SequenceFinished;

        public void UpdateXp(int xp)
        {
            if (!(ContextData is SeasonLevelInfoAnimatedModel model)) return;
            
            model.UpdateRemainingXp(xp);
        }
        
        protected override void OnInitialized()
        {
            _userLevel.text = ContextData.Level.ToString();
            StartNextCycle();
        }

        private void OnSliderAnimationComplete()
        {
            AnimationFinished -= OnSliderAnimationComplete;
            
            _userLevel.text = ContextData.Level.ToString();
            
            PlayScale();
        }

        [Button]
        private void PlayScale()
        {
            _levelIconAnimation.PlayFireworks += OnFireworks;
            _levelIconAnimation.Play(OnScaleAnimationFinished);
        }

        private void OnFireworks()
        {
            _levelIconAnimation.PlayFireworks -= OnFireworks;
            _fireworksVFX.Play();
        }
        
        private void OnScaleAnimationFinished()
        {
            StartNextCycle();
        }

        private void OnSequenceFinished()
        {
            AnimationFinished -= OnSequenceFinished;
            
            SequenceFinished?.Invoke();
        }

        private void StartNextCycle()
        {
            if (ContextData.UpdateForNextCycle())
            {
                AnimationFinished += OnSliderAnimationComplete;
            }
            else
            {
                AnimationFinished += OnSequenceFinished;
            }

            var tempColor = _sliderFill.color;
            tempColor.a = 1;
            _sliderFill.color = tempColor;
            
            Play(ContextData.StartValue, ContextData.EndValue, ContextData.MaxValue);
        }
    }
}