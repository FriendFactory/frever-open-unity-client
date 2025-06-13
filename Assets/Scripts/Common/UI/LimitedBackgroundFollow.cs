using System;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using System.Threading.Tasks;

namespace Common.UI
{
    public class LimitedBackgroundFollow: MonoBehaviour
    {
        [Flags]
        private enum Limit
        {
            Top = 1,
            Bottom = 1 << 1,
            Left = 1 << 2,
            Right = 1 << 3
        }
        
        private readonly Vector3[] _scrollPoints = new Vector3[4];
        private readonly Vector3[] _containerPoints = new Vector3[4];
        
        [SerializeField] private EnhancedScroller _followTarget;
        [SerializeField] private RectTransform _followContainer;
        [SerializeField] private Limit _limits;

        private RectTransform _rectTransform;
        private Vector3 _startOffset;
        
        private void Awake()
        {
            if (_followContainer == null)
            {
                _followContainer = transform.parent.GetComponent<RectTransform>();
            }

            _rectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            _rectTransform.pivot = _followTarget.Container.pivot;
            _startOffset = _rectTransform.position - _followTarget.Container.position;
            OnPositionChanged(_rectTransform.position);
        }

        private async void OnEnable()
        {
            await Task.Yield();
            _followTarget.ScrollRect.onValueChanged.AddListener(OnPositionChanged);
        }

        private void OnDisable()
        {
            _followTarget.ScrollRect.onValueChanged.RemoveListener(OnPositionChanged);
        }

        private void OnPositionChanged(Vector2 newPos)
        {
            _rectTransform.position = _followTarget.Container.position + _startOffset;

            _rectTransform.GetWorldCorners(_scrollPoints);
            _followContainer.GetWorldCorners(_containerPoints);
            
            if ((_limits & Limit.Top) == Limit.Top && _scrollPoints[2].y > _containerPoints[2].y)
            {
                _rectTransform.position -= new Vector3(0, _scrollPoints[2].y - _containerPoints[2].y, 0);
            }
            
            if ((_limits & Limit.Bottom) == Limit.Bottom && _scrollPoints[0].y < _containerPoints[0].y)
            {
                _rectTransform.position -= new Vector3(0, _scrollPoints[0].y - _containerPoints[0].y, 0);
            }
            
            if ((_limits & Limit.Right) == Limit.Right && _scrollPoints[2].x > _containerPoints[2].x)
            {
                _rectTransform.position -= new Vector3(_scrollPoints[2].x - _containerPoints[2].x, 0, 0);
            }

            if ((_limits & Limit.Left) == Limit.Left && _scrollPoints[0].x < _containerPoints[0].x)
            {
                _rectTransform.position -= new Vector3(_scrollPoints[0].x - _containerPoints[0].x, 0, 0);
            }
        }
    }
}