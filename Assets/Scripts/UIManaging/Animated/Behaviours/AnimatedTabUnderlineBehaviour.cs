using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UIManaging.Animated.Sequences;
using UnityEngine;

namespace UIManaging.Animated.Behaviours
{
    public class AnimatedTabUnderlineBehaviour : SequenceAnimationController
    {
        [SerializeField] private bool _scaleIndicatorWithTabSize = true;
        [SerializeField] private bool _getTabsFromParent;
        [SerializeField, ShowIf("_getTabsFromParent")] private Transform _parent;
        
        [Space]
        [SerializeField] private List<RectTransform> _tabs;
        [SerializeField] private RectTransform _underline;

        private int _targetTabIndex;
        
        public bool IsInitialized { get; private set; }

        public void Initialize(int index = 0)
        {
            if (_getTabsFromParent)
            {
                for (var i = 1; i < _parent.childCount; i++)
                {
                    var child = _parent.GetChild(i);
                    _tabs.Add(child.GetComponent<RectTransform>());
                }
            }
            
            SetTargetTabIndex(index);
            var x = GetTabPositionX();
            _underline.anchoredPosition = new Vector3(x, _underline.anchoredPosition.y);
            
            IsInitialized = true;
            
            if (!_scaleIndicatorWithTabSize) return;
            
            var size = GetTabSize();
            var scale = new Vector3(size / 100, 1);
            _underline.localScale = scale;
        }

        public void SetTargetTabIndex(int index)
        {
            _targetTabIndex = index;
            BuildSequence();
        }

        public void SetTargetTabIndexImmediate(int index)
        {
            _underline.gameObject.SetActive(true);
            _targetTabIndex = index;
            _underline.anchoredPosition = new Vector2(GetTabPositionX(), _underline.anchoredPosition.y);
            
            if (!_scaleIndicatorWithTabSize) return;
            
            var size = GetTabSize();
            var scale = new Vector3(size / 100, 1);
            _underline.localScale = scale;
        }
        internal override void BuildSequence()
        {
            var positionMModel = GetPositionModel();
            
            Animation = SequenceElementHelper.NewAnimation(SequenceType.Parallel)
                                             .AnimatePosition(positionMModel);

            if (!_scaleIndicatorWithTabSize) return;
            
            var size = GetTabSize();
            var scale = new Vector3(size / 100, 1);
            Animation.AnimateScale(_underline.localScale, scale, 0.2f);
        }

        private AnimatePositionModel GetPositionModel()
        {
            var position = GetTabPositionX();
            return new AnimatePositionModel
            {
                Ease = Ease.OutQuad,
                UseCurrentPosition = true,
                UseAnchoredPosition = true,
                Time = 0.4f,
                FromPosition = _underline.anchoredPosition,
                ToPosition = new Vector3(position, _underline.anchoredPosition.y, 0)
            };

        }
        
        private float GetTabSize()
        {
            return _tabs[_targetTabIndex].rect.width;
        }

        private float GetTabPositionX()
        {
            return _tabs[_targetTabIndex].anchoredPosition.x;
        }

        [Button]
        private void Preview(int tabIndex)
        {
            SetTargetTabIndex(tabIndex);
            Play(null);
        }
    }
}