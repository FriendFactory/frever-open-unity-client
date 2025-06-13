using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace UIManaging.Animated.Sequences
{
    [Serializable]
    public sealed class AnimatePositionSequence : AnimationSequenceBase
    {
        [SerializeField] private bool _useAnchoredPosition;
        [SerializeField] private bool _useLocalPosition;
        [SerializeField] private bool _useRelativePosition;
        [SerializeField] private bool _isTargetingGameObject;
        
        [Header("Start")] 
        [SerializeField] private bool _useCurrentPosition;
        [SerializeField, HideIf("_useCurrentPosition")] private Transform _startObject;
        [SerializeField, HideIf("_isTargetingGameObject"), HideIf("_useCurrentPosition")] private Vector3 _startPosition;

        [Header("Animation Parameters")] 
        [SerializeField] private float _time = 1.0f;
        [SerializeField] private Ease _easing = Ease.Linear;
        
        [Header("End")] 
        [SerializeField, ShowIf("_isTargetingGameObject")] private Transform _targetObject;
        [SerializeField, HideIf("_isTargetingGameObject")] private Vector3 _targetPosition;
        
        public void Initialize(AnimatePositionModel? model)
        {
            if(model is null) return;

            _useAnchoredPosition = model.Value.UseAnchoredPosition;
            _useLocalPosition = model.Value.UseLocalPosition;
            _useRelativePosition = model.Value.UseRelativePosition;
            _useCurrentPosition = model.Value.UseCurrentPosition;

            _time = model.Value.Time;

            _startPosition = model.Value.FromPosition;
            _targetPosition = model.Value.ToPosition;
            
            _easing = model.Value.Ease;
        }
        
        public override void Play(GameObject animatedObject)
        {
            SetStartingPosition(animatedObject);

            var sequence = GetSequence(animatedObject);
            
            sequence.Play();
        }

        private void SetStartingPosition(GameObject animatedObject)
        {
            if (_useAnchoredPosition)
            {
                var rectTransform = animatedObject.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = _startPosition;
                
                return;
            }
            if (!_useCurrentPosition)
                animatedObject.transform.position = _startObject == null ? _startPosition : -_startObject.position;
        }

        private Sequence GetSequence(GameObject animatedObject)
        {
            var sequence = DOTween.Sequence();

            _targetPosition = _isTargetingGameObject ? _targetObject.position : _targetPosition;
            sequence.Append(Move(animatedObject))
                           .AppendCallback(OnSequenceFinished);

            SetEasing(sequence);
            
            return sequence;
        }

        private void SetEasing(Sequence sequence)
        {
                sequence.SetEase(_easing); 
        }

        private Vector3 GetRelativeTargetPosition(GameObject animatedObject)
        {
            return animatedObject.transform.localPosition + _targetPosition;
        }

        private TweenerCore<Vector3, Vector3, VectorOptions> Move(GameObject animatedObject)
        {
            var target = _useRelativePosition ? GetRelativeTargetPosition(animatedObject) : _targetPosition;

            if (_useAnchoredPosition)
            {
                var rectTransform = animatedObject.GetComponent<RectTransform>();
                return rectTransform.DOAnchorPos3D(target, _time);
            }

            return _useLocalPosition
                ? animatedObject.transform.DOLocalMove(target, _time)
                : animatedObject.transform.DOMove(target, _time);
        }
        
        [Button]
        private void Preview(GameObject animatedObject)
        {
            var startPosition = animatedObject.transform.position;
            AnimationFinished += _ => OnPreviewFinished(animatedObject, startPosition);
            Play(animatedObject);
        }

        private void OnPreviewFinished(GameObject animatedObject, Vector3 startPosition)
        {
            AnimationFinished -= _ => OnPreviewFinished(animatedObject, startPosition);
            animatedObject.transform.position = startPosition;
        }
    }
}