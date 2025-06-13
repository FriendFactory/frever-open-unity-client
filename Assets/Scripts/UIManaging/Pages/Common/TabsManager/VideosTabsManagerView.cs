using DG.Tweening;
using Extensions;
using UnityEngine;

namespace UIManaging.Pages.Common.TabsManager
{
    public sealed class VideosTabsManagerView : TabsManagerView
    {
        [SerializeField] private float _toogleMoveSpeed = 2500f;
        [SerializeField] private RectTransform _selectionRect;
        [SerializeField] private float _videoGridSpacing;

        private Tween _selectionRectMoveTween;
        private float _tabSize;
        private float _spacingOffset;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnDisable()
        {
            _selectionRectMoveTween?.Kill();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override void Init(TabsManagerArgs tabsManagerArgs)
        {
            base.Init(tabsManagerArgs);
            SetMarkerSize();
            ShowSelectionRect();
        }

        private void SetMarkerSize()
        {
            var size = ((RectTransform)transform).GetWidth();
            _tabSize = (size - _videoGridSpacing * (TabsManagerArgs.Tabs.Length - 1))
                     / TabsManagerArgs.Tabs.Length;
            _selectionRect.sizeDelta = new Vector2(_tabSize, _selectionRect.sizeDelta.y);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnToggleSetOn(int index, bool setByUser)
        {
            base.OnToggleSetOn(index, setByUser);
            _selectionRectMoveTween?.Kill();
            var targetXPosition = (_tabSize + _videoGridSpacing) * index;
            var targetPosition = new Vector2(targetXPosition, _selectionRect.anchoredPosition.y);
            _selectionRectMoveTween = _selectionRect.DOAnchorPos(targetPosition, _toogleMoveSpeed).SetSpeedBased().SetEase(Ease.Linear);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void ShowSelectionRect()
        {
            _selectionRect.gameObject.SetActive(true);
        }

        private void HideSelectionRect()
        {
            _selectionRect.gameObject.SetActive(false);
        }
    }
}