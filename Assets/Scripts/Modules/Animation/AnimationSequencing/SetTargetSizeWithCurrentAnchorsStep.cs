using System;
using BrunoMikoski.AnimationSequencer;
using DG.Tweening;
using Extensions;
using UnityEngine;

namespace Modules.AnimationSequencing
{
    [Serializable]
    public class SetTargetSizeWithCurrentAnchorsStep: AnimationStepBase
    {
        [SerializeField] private RectTransform _targetTransform;
        [SerializeField] private RectTransform.Axis _axis;
        [SerializeField] private float _size;
        
        private float _originalSize;

        public override string DisplayName => "Set Target Size With Current Anchors";
        
        public override void AddTweenToSequence(Sequence animationSequence)
        {
            var behaviourSequence = DOTween.Sequence();
            behaviourSequence.SetDelay(Delay);

            behaviourSequence.AppendCallback(() =>
            {
                _originalSize = _targetTransform.GetSizeWithCurrentAnchors(_axis); 
                _targetTransform.SetSizeWithCurrentAnchors(_axis, _size);
            });
            
            if (FlowType == FlowType.Join)
                animationSequence.Join(behaviourSequence);
            else
                animationSequence.Append(behaviourSequence);
        }

        public override void ResetToInitialState()
        {
            _targetTransform.SetSizeWithCurrentAnchors(_axis, _originalSize);
        }
    }
}