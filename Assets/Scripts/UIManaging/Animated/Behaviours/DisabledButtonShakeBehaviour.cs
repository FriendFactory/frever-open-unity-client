using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UIManaging.Animated.Sequences;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIManaging.Animated.Behaviours
{
    public class DisabledButtonShakeBehaviour : SequenceAnimationController, IPointerUpHandler, IPointerDownHandler
    {
        public UnityEvent OnNonInteractableClick;
        
        [SerializeField] private Button _button;

        private bool _isPlaying;
        private bool _wasInteractable;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_button == null) return;
            _wasInteractable = _button.interactable;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_button == null) return;
            if (_button.interactable) return;
            if (_isPlaying) return;
            if(_wasInteractable) return;

            _isPlaying = true;
            _wasInteractable = true;

            Animation.Play(gameObject, OnAnimationDone);
            
            OnNonInteractableClick?.Invoke();
        }
        
        private void OnAnimationDone()
        {
            _isPlaying = false;
        }

        internal override void BuildSequence()
        {
            var toRight = new AnimatePositionModel
            {
                Ease = Ease.OutElastic,
                UseLocalPosition = true,
                UseRelativePosition = true,
                UseCurrentPosition = true,
                Time = 0.066f,
                ToPosition = new Vector3(-15, 0, 0),
            };
            var toLeft = new AnimatePositionModel
            {
                Ease = Ease.OutElastic,
                UseLocalPosition = true,
                UseRelativePosition = true,
                UseCurrentPosition = true,
                Time = 0.066f,
                ToPosition = new Vector3(15, 0, 0),
            };
            
            Animation = SequenceElementHelper.NewAnimation(SequenceType.Sequence)
                                                   .AnimatePosition(toLeft)
                                                   .AnimatePosition(toRight)
                                                   .AnimatePosition(toRight)
                                                   .AnimatePosition(toLeft)
                                                   .AnimatePosition(toLeft)
                                                   .AnimatePosition(toRight);
        }

        [Button("Preview")]
        private void Preview()
        {
            Animation.Play(gameObject, null);
        }
    }
}