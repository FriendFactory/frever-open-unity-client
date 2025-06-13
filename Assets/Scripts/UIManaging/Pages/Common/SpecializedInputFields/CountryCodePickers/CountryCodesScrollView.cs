using System;
using Abstract;
using EnhancedUI.EnhancedScroller;
using UIManaging.Common;
using UIManaging.Common.ScrollSelector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIManaging.Pages.Common.RegistrationInputFields.CountryCodePickers
{
    public sealed class CountryCodesScrollView : BaseContextDataView<ScrollSelectorModel>, IEnhancedScrollerDelegate
    {
        [SerializeField] private ExtendedScrollRect _extendedScrollRect;
        [SerializeField] private EnhancedScroller _enhancedScroller;
        [SerializeField] private EnhancedScrollerCellView _enhancedScrollerCellView;
        
        [SerializeField] private Button _cancelButton;
        [SerializeField] private Button _confirmButton;

        private int _dataIndex;
        
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action OnCanceled;
        public event Action<int> OnConfirmed;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            _enhancedScroller.Delegate = this;
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }
        
        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return ContextData == null ? 0 : ContextData.Items.Length;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return _enhancedScrollerCellView.GetComponent<RectTransform>().rect.height;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellsView = scroller.GetCellView(_enhancedScrollerCellView);
            var itemView = cellsView.GetComponent<ScrollSelectorItemView>();
            itemView.Initialize(ContextData.Items[dataIndex]);
            return cellsView;
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            _cancelButton.onClick.AddListener(OnCancel);
            _confirmButton.onClick.AddListener(OnConfirm);
            _enhancedScroller.scrollerSnapped += OnScrollerSnapped;
            _extendedScrollRect.OnEndDragEvent += OnScrollRectEndDrag;
            
            _enhancedScroller.ReloadData();
            _enhancedScroller.JumpToDataIndex(ContextData.InitialDataIndex, 0.5f, 0.5f);
            _enhancedScroller._RefreshActive();
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();

            _enhancedScroller.scrollerSnapped -= OnScrollerSnapped;
            _extendedScrollRect.OnEndDragEvent -= OnScrollRectEndDrag;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void OnCancel()
        {
            OnCanceled?.Invoke();
        }

        private void OnConfirm()
        {
            OnConfirmed?.Invoke(_dataIndex);
        }

        private void OnScrollRectEndDrag(PointerEventData pointerEventData)
        {
            _enhancedScroller.Snap();
        }

        private void OnScrollerSnapped(EnhancedScroller enhancedScroller, int cellIndex, int dataIndex, EnhancedScrollerCellView cellView)
        {
            _dataIndex = dataIndex;
        }
    }
}