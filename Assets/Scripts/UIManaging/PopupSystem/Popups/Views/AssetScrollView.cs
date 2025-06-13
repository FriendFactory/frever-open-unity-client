using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups
{
    public class AssetScrollView : MonoBehaviour
    {
        [SerializeField]
        private ScrollRect _scrollRect;

    //    private GridLayoutGroup _gridLayout;
        private Vector2 _originSize;
        private RectTransform rectTransform;
        private bool _expandGridCount;

        public RectTransform Content => _scrollRect.content;

        public void Setup(bool expandGridCount)
        {
            _expandGridCount = expandGridCount;
            _scrollRect = GetComponent<ScrollRect>();
         //   _gridLayout = Content.GetComponent<GridLayoutGroup>();
            rectTransform = transform as RectTransform;
            _originSize = rectTransform.sizeDelta;
            _scrollRect.onValueChanged.AddListener(ScrollRect_OnValueChanged);
        }
        public void OnViewExpansion()
        {
            // if (_expandGridCount)
            // {
            //     _gridLayout.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            //     _gridLayout.constraintCount = 2;
            //     _scrollRect.viewport.sizeDelta = new Vector2(_scrollRect.viewport.sizeDelta.x, _gridLayout.cellSize.y * _gridLayout.constraintCount);
            //     rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, _scrollRect.viewport.sizeDelta.y + 100);
            // }
        }

        public void OnViewContraction()
        {
            // if (_expandGridCount)
            // {
            //     _gridLayout.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            //     _gridLayout.constraintCount = 1;
            //     _scrollRect.viewport.sizeDelta = new Vector2(_scrollRect.viewport.sizeDelta.x, _gridLayout.cellSize.y * _gridLayout.constraintCount);
            //     rectTransform.sizeDelta = _originSize;
            // }
        }

        private void OnEnable ()
        {
            ScrollRect_OnValueChanged(Vector2.zero);
        }

        private void ScrollRect_OnValueChanged(Vector2 diff)
        {
            // var gridGroup = _scrollRect.content.GetComponent<GridLayoutGroup>();
            // var childWidth = _scrollRect.content.childCount * gridGroup.cellSize.x + (_scrollRect.content.childCount-1)*gridGroup.spacing.x;
            //
            // if (rectTransform.rect.width >= childWidth) return;
        }
    }
}
