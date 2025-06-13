using System;
using Abstract;
using Modules.Sound;
using Sirenix.OdinInspector;
using UIManaging.Animated.Sequences;
using UnityEngine;
using Zenject;

namespace UIManaging.Animated.Behaviours
{
    public class AnimatedSliderView<TAnimatedSliderModel> : BaseContextDataView<TAnimatedSliderModel> 
        where TAnimatedSliderModel: IAnimatedSliderModel
    {
        [Inject] private ISoundManager _soundManager;
        
        [SerializeField] private AnimateSliderBehaviour _animateSliderBehaviour;

        private readonly AnimateSliderModel _sliderModel = new AnimateSliderModel();

        public event Action AnimationFinished;

        protected override void OnInitialized()
        {
            Play(ContextData.StartValue, ContextData.EndValue, ContextData.MaxValue);
        }
        
        [Button]
        protected void Play(float startValue, float targetValue, int maxValue)
        {
            if (startValue != targetValue)
            {
                _soundManager.Play(SoundType.XpBarSweep, MixerChannel.SpecialEffects);
            }
            
            _animateSliderBehaviour.SliderMaxValue = maxValue;
            
            if (Math.Abs(startValue - targetValue) < 0.01)
            {
                _animateSliderBehaviour.UpdateSliderValue(targetValue);

                AnimationFinished?.Invoke();
                return;
            }
            
            _sliderModel.StartValue = startValue;
            _sliderModel.TargetValue = targetValue;
            
            _animateSliderBehaviour.Model = _sliderModel;
            _animateSliderBehaviour.Play(OnAnimationFinished);
        }

        private void OnAnimationFinished()
        {
            AnimationFinished?.Invoke();
        }
    }
    
    public class AnimatedSliderView: AnimatedSliderView<IAnimatedSliderModel> { }
}