using System;
using BrunoMikoski.AnimationSequencer;
using DG.Tweening;
using Modules.Animation;
using TMPro;
using UnityEngine;

namespace Modules.AnimationSequencing
{
    [Serializable]
    public sealed class TextValueIncrementDOTweenAction: DOTweenActionBase
    {
        [SerializeField] private TextIncrementAnimationDataProvider _animationDataProvider;
        
        public override string DisplayName => "Increment text value";
        public override Type TargetComponentType => typeof(TMP_Text);

        private TMP_Text _targetText;
        private TextIncrementAnimationModel _animationModel;
        private string _initialText;

        protected override Tweener GenerateTween_Internal(GameObject target, float duration)
        {
            if (!_targetText)
            {
                _targetText = target.GetComponent<TMP_Text>();
                if (!_targetText)
                {
                    Debug.LogError($"{target} does not have {TargetComponentType} component");
                    return null;
                }
            }

            if (!_animationDataProvider)
            {
                Debug.LogError($"{target} {nameof(TextIncrementAnimationDataProvider)} is missed");
                return null;
            }

            _animationModel = _animationDataProvider.AnimationModel;
            if (_animationModel == null)
            {
                Debug.LogError($"{target} {nameof(TextIncrementAnimationDataProvider)} has empty model");
                return null;
            }
                
            _initialText = _targetText.text;
            var textTween = _targetText.DOValueAsTextChange(_animationModel.From, _animationModel.To, duration);

            return textTween;
        }

        public override void ResetToInitialState()
        {
            if (!_targetText) return;

            _targetText.text = _initialText;
        }
    }
}