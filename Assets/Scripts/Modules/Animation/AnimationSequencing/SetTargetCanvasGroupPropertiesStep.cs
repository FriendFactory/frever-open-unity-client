using System;
using BrunoMikoski.AnimationSequencer;
using DG.Tweening;
using UnityEngine;

namespace Modules.AnimationSequencing
{
    [Serializable]
    public class SetTargetCanvasGroupPropertiesStep: AnimationStepBase
    {
        [SerializeField] private CanvasGroup _targetCanvasGroup;
        [SerializeField] private float _targetAlpha;
        
        private float _originalAlpha;

        public override string DisplayName => "Set Target CanvasGroup Properties";
        
        public override void AddTweenToSequence(Sequence animationSequence)
        {
            Sequence behaviourSequence = DOTween.Sequence();
            behaviourSequence.SetDelay(Delay);

            behaviourSequence.AppendCallback(() =>
            {
                _originalAlpha = _targetCanvasGroup.alpha; 
                _targetCanvasGroup.alpha = _targetAlpha;
            });
            
            if (FlowType == FlowType.Join)
                animationSequence.Join(behaviourSequence);
            else
                animationSequence.Append(behaviourSequence);
        }

        public override void ResetToInitialState()
        {
            _targetCanvasGroup.alpha = _originalAlpha; 
        }
        
        public override string GetDisplayNameForEditor(int index)
        {
            var display = _targetCanvasGroup != null ? _targetCanvasGroup.name : "NULL";
            
            return $"{index}. Set {display} Properties";
        } 
    }
}