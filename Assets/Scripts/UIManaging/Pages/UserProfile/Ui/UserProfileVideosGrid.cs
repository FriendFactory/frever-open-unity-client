using System;
using UIManaging.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIManaging.Pages.UserProfile.Ui
{
    public class UserProfileVideosGrid : VideoGrid
    {
        [SerializeField] private ProfileScrollablePanel _scrollablePanel;
        [SerializeField] private LimitedScrollRect _limitedScrollRect;

        private HorizontalOrVerticalLayoutGroup _scrollRectLayoutGroup;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action<float> OnScrollablePanelYPositionChangedEvent;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnEnable()
        {
            _scrollablePanel.OnIsScrollablePartOnTopChangedEvent += OnIsScrollablePartOnTopChangedEvent;
            _scrollablePanel.OnDragEvent += OnDrag;
            
            _limitedScrollRect.OnDragWithDisabledScrollDown += LimitedScrollDragWithDisabledScrollDown;
            _limitedScrollRect.OnBeginDragEvent += OnLimitedScrollBeginDrag;
            _limitedScrollRect.OnEndDragEvent += OnLimitedScrollEndDrag;
            _limitedScrollRect.OnDragOverTopBorderEvent += OnDragOverTopBorder;
        }

        private void OnDisable()
        {
            _scrollablePanel.OnIsScrollablePartOnTopChangedEvent -= OnIsScrollablePartOnTopChangedEvent;
            _scrollablePanel.OnDragEvent -= OnDrag;

            _limitedScrollRect.OnDragWithDisabledScrollDown -= LimitedScrollDragWithDisabledScrollDown;
            _limitedScrollRect.OnEndDragEvent -= OnLimitedScrollEndDrag;
            _limitedScrollRect.OnBeginDragEvent -= OnLimitedScrollBeginDrag;
            _limitedScrollRect.OnDragOverTopBorderEvent -= OnDragOverTopBorder;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void UpdateFirstPage()
        {
            ContextData.DownloadFirstPage();
            ContextData.AdjustToRowSize(_itemsInRow);
            _enhancedScroller.ReloadData();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnIsScrollablePartOnTopChangedEvent()
        {
            _limitedScrollRect.EnableScrollDown = _scrollablePanel.IsExpanded;

            if (_scrollRectLayoutGroup == null)
            {
                _scrollRectLayoutGroup = _limitedScrollRect.content.GetComponent<HorizontalOrVerticalLayoutGroup>();
            }

            _scrollRectLayoutGroup.enabled = false;
            _scrollRectLayoutGroup.enabled = true;
        }

        private void OnDrag(float scrollablePanelYPosition)
        {
            // Method _RefreshActive was made public to allow updating Enhanced Scroller items without performance penalty
            _enhancedScroller._RefreshActive();
            OnScrollablePanelYPositionChangedEvent?.Invoke(scrollablePanelYPosition);
        }

        private void LimitedScrollDragWithDisabledScrollDown(PointerEventData pointerEventData)
        {
            _scrollablePanel.OnDrag(pointerEventData);
        }
        
        private void OnLimitedScrollBeginDrag(PointerEventData pointerEventData)
        {
            _scrollablePanel.OnBeginDrag(pointerEventData);
        }
        
        private void OnLimitedScrollEndDrag(PointerEventData pointerEventData)
        {
            _scrollablePanel.OnEndDrag(pointerEventData);
        }
        
        private void OnDragOverTopBorder(PointerEventData pointerEventData)
        {
            var dragData = new PointerEventData(EventSystem.current) {delta = Vector2.up * pointerEventData.delta.y};
            _scrollablePanel.OnDrag(dragData);
        }
    }
}