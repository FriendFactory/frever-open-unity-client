using System;
using UnityEngine;
using EnhancedUI.EnhancedScroller;

namespace UIManaging.Pages.StyleSelection.UI
{
    internal sealed class CharacterStyleSelectionList : StyleSelectionList
    {
        private readonly Vector2 _leftPaddingRange = new Vector2(126f, 295f);
        private readonly Vector2 _snapCellOffsetRange = new Vector2(-.08f, -.24f);
        private readonly Vector2 _ratioRange = new Vector2(.46182f, .75f);
        
        [SerializeField] private EnhancedScrollerCellView _selfieCellViewItem;

        public Action<bool> OnSelfieElementSelected { get; set; }
        public Action OnSelfieButtonClicked { get; set; }

        private bool _isSelfieElementSelected;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();
            var aspectRatio = Screen.width / (float)Screen.height;
            var t = Mathf.InverseLerp(_ratioRange.x, _ratioRange.y, aspectRatio);
            var snapOffset = Mathf.Lerp(_snapCellOffsetRange.x, _snapCellOffsetRange.y, t);
            var leftPadding = Mathf.Lerp(_leftPaddingRange.x, _leftPaddingRange.y, t);
            
            var scroller = GetComponent<EnhancedScroller>();
            scroller.padding.left = Mathf.RoundToInt(leftPadding);
            scroller.snapCellCenterOffset = snapOffset;
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override int GetNumberOfCells(EnhancedScroller scroller)
        {
            var cellsNumber = ContextData == null ? 0 : ContextData.StylePresets.Length;
            if (ContextData is not null && ContextData.ContainsSelfieButton)
            {
                ++cellsNumber;
            }
            
            return cellsNumber;
        }

        public override EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            // Check if this is the last element (selfie element)
            if (dataIndex == ContextData.StylePresets.Length)
            {
                var cellView = scroller.GetCellView(_selfieCellViewItem);
                var selfieItem = cellView.GetComponent<SelfieSelectionElement>();
                selfieItem.Initialize(OnButtonClicked, () => OnItemClicked(dataIndex));
                return cellView;
            }

            return base.GetCellView(scroller, dataIndex, cellIndex);
        }

        public override void Reset()
        {
            base.Reset();
            _isSelfieElementSelected = false;
        }

        public override void SetSelected(long styleId)
        {
            base.SetSelected(styleId);
            
            if (_isSelfieElementSelected)
            {
                _isSelfieElementSelected = false;
                OnSelfieElementSelected?.Invoke(false);
            }
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            var itemsCount = (ContextData?.StylePresets?.Length ?? 0) + 1; // +1 for selfie element
            _positionIndicator.Initialilze(itemsCount);
            
            _enhancedScroller.ReloadData();
            if (itemsCount > 1)
            {
                JumpToLastSavedPosition();
            }
        }

        //---------------------------------------------------------------------
        // Protected - Override base class's private methods
        //---------------------------------------------------------------------

        protected override void OnScrollerSnapped(int index)
        {
            bool isSelectingSelfieElement = index == ContextData.StylePresets.Length;
            
            if (isSelectingSelfieElement)
            {
                SelectedStyle = null;
                if (!_isSelfieElementSelected)
                {
                    _isSelfieElementSelected = true;
                    OnSelfieElementSelected?.Invoke(true);
                }
                _savedCellIndex = index;
                _positionIndicator.Refresh(index); // Update position indicator for selfie element
            }
            else
            {
                base.OnScrollerSnapped(index);
                if (_isSelfieElementSelected)
                {
                    _isSelfieElementSelected = false;
                    OnSelfieElementSelected?.Invoke(false);
                }
            }
        }

        protected override void JumpToLastSavedPosition()
        {
            _savedCellIndex = Mathf.Clamp(_savedCellIndex, 0, ContextData.StylePresets.Length);
            _enhancedScroller.JumpToDataIndex(_savedCellIndex, _enhancedScroller.snapJumpToOffset, _enhancedScroller.snapCellCenterOffset, _enhancedScroller.snapUseCellSpacing);
            _enhancedScroller._RefreshActive();
            OnScrollerSnapped(_savedCellIndex);
        }

        private void OnButtonClicked()
        {
            OnSelfieButtonClicked?.Invoke();
        }
    }
}