using UIManaging.Animated.Sequences;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Animated.Behaviours
{
    public class AnimateSliderBehaviour : SequenceAnimationController
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private AnimateSliderSequence _animateSliderSequence;

        public float SliderMaxValue
        {
            get => _slider.maxValue;
            set => _slider.maxValue = value;
        }

        public AnimateSliderModel Model
        {
            set => _animateSliderSequence.SetModel(value);
        }

        private void Awake()
        {
            _animateSliderSequence = Animation.GetAnimationElementAt(0).Slider;
        }

        public override void Reset()
        {
            _slider = GetComponent<Slider>();
            base.Reset();
        }

        public void UpdateSliderValue(float value)
        {
            _animateSliderSequence?.UpdateSliderValue(value);
        }
        
        internal override void BuildSequence()
        {
            Animation = SequenceElementHelper.NewAnimation(SequenceType.Sequence)
                                             .AnimateSlider(_slider);
        }
    }
}