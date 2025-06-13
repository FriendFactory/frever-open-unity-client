using System;
using System.Collections;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Animated.Sequences
{
    [Serializable]
    public class AnimateSliderSequence : AnimationSequenceBase
    {
        [SerializeField] private bool _sliderOnDifferentObject;
        [ShowIf("_sliderOnDifferentObject")]
        [SerializeField] internal Slider _animatedSlider;

        [SerializeField] private bool _hasProgressText;
        [SerializeField, ShowIf("_hasProgressText")] private TMP_Text _progressText;
        [SerializeField] private bool _controlledByScript = true;

        private bool _useCurrentValue = false;
        [HideIf("@_controlledByScript || _useCurrentValue")]
        [SerializeField] private float _startingValue = 0.0f;

        [SerializeField] private float _time = 1.0f;
        [SerializeField] private AnimationCurve _easing = AnimationCurve.Linear(0, 0, 1, 1);

        [HideIf("_controlledByScript")]
        [SerializeField] private float _targetValue = 1.0f;

        [SerializeField] private AnimateSliderModel _model;

        private Coroutine _textAnimationRoutine;
        
        public void SetModel(AnimateSliderModel model)
        {
            _controlledByScript = true;
            _model = model;
        }

        public override void Play(GameObject animatedObject)
        {
            SetStartingValue();
            
            _model.TargetValue = Mathf.Clamp(_model.TargetValue, _animatedSlider.minValue, _animatedSlider.maxValue);
            var sequence = Build();

            PlayProgressTextRoutine();
            sequence.Play();
        }

        public void UpdateSliderValue(float value)
        {
            if (_animatedSlider == null) return;

            _animatedSlider.value = value;
            
            if(!_hasProgressText) return;
            _progressText.text = $"{value}/{_animatedSlider.maxValue}";
        }

        public override Sequence Build()
        {
            var sequence = DOTween.Sequence();

            var targetValue = _controlledByScript ? _model.TargetValue : _targetValue;
            sequence.SetEase(_easing)
                    .Append(_animatedSlider.DOValue(targetValue, _time))
                    .AppendCallback(OnSequenceFinished);

           
            return sequence;
        }
        
        private void SetStartingValue()
        {
            if (_useCurrentValue) return;

            var startingValue = _controlledByScript ? _model.StartValue : _startingValue;
            _animatedSlider.value = startingValue;
        }

        private IEnumerator AnimateProgressTextRoutine()
        {
            var startValue = _controlledByScript ? _model.StartValue : _startingValue;
            var targetValue = _controlledByScript ? _model.TargetValue : _targetValue;
            var elapsedTime = 0.0f;
            var progress = 0.0f;
            var difference = targetValue - startValue;
            
            while (progress <= 1.0f)
            {
                yield return new WaitForEndOfFrame();
                elapsedTime += Time.deltaTime;
                progress = elapsedTime / _time;
                
                var currentValue = Mathf.FloorToInt(startValue + difference * progress);
                if (_hasProgressText)
                {
                    _progressText.text = $"{currentValue}/{_animatedSlider.maxValue}";
                }
                
            }

            if (_hasProgressText)
            {
                _progressText.text = $"{targetValue}/{_animatedSlider.maxValue}";
            }

            _textAnimationRoutine = null;
        }

        private void PlayProgressTextRoutine()
        {
            if (_animatedSlider == null || !_animatedSlider.IsActive())
            {
                return;
            }
            
            if (_textAnimationRoutine != null)
            {
                _animatedSlider.StopCoroutine(AnimateProgressTextRoutine());
            }

            _textAnimationRoutine = _animatedSlider.StartCoroutine(AnimateProgressTextRoutine());
        }
    }
}