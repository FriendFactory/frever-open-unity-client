using System.Collections.Generic;
using System.Linq;
using Abstract;
using EnhancedUI.EnhancedScroller;
using ModestTree;
using UIManaging.Common;
using UIManaging.Core;
using UnityEngine;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;

namespace UIManaging.Pages.StyleSelection.UI
{
    internal class StyleSelectionList : BaseContextDataView<StyleSelectionListModel>, IEnhancedScrollerDelegate
    {
        [SerializeField] private ScrollRectEnhancedScrollerSnapping _enhancedScrollerSnapping;
        [SerializeField] protected EnhancedScroller _enhancedScroller;
        [SerializeField] private EnhancedScrollerCellView _cellViewItem;
        [SerializeField] private List<Sprite> _backgroundImages;
        [SerializeField] protected ListPositionIndicator _positionIndicator;

        public CharacterInfo SelectedStyle { get; protected set; }

        protected int _savedCellIndex;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        protected virtual void Awake()
        {
            _enhancedScroller.Delegate = this;
        }

        private void OnEnable()
        {
            _enhancedScrollerSnapping.Snapping += OnScrollerSnapped;
        }

        private void OnDisable()
        {
            _enhancedScrollerSnapping.Snapping -= OnScrollerSnapped;
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public virtual void SetSelected(long styleId)
        {
            var style = ContextData.StylePresets.First(x => x.Id == styleId);
            SelectedStyle = style;
            _savedCellIndex = ContextData.StylePresets.IndexOf(style);
            _positionIndicator.Refresh(_savedCellIndex);
        }

        public virtual int GetNumberOfCells(EnhancedScroller scroller)
        {
            return ContextData == null ? 0 : ContextData.StylePresets.Length;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return ContextData?.CellSize ?? 0f;
        }

        public virtual EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = scroller.GetCellView(_cellViewItem);
            var styleItem = cellView.GetComponent<StyleSelectionElement>();
            
            var style = ContextData.StylePresets[dataIndex];
            var background = GetElementBackground(dataIndex);
            styleItem.Initialize(background, ContextData.PresetThumbnails[style], ()=>OnItemClicked(dataIndex));
            
            return cellView;
        }

        public virtual void Reset()
        {
            _savedCellIndex = 0;
        }

        public void SetSelectedSelfieItem()
        {
            var selfieIndex = ContextData.StylePresets.Length;
            _enhancedScrollerSnapping.Snap(selfieIndex);
            OnScrollerSnapped(selfieIndex);
        }

        public void Clear()
        {
            if (ContextData != null)
            {
                CleanUp();
            }
        
            OnInitialized();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            var itemsCount = ContextData?.StylePresets?.Length ?? 0;
            _positionIndicator.Initialilze(itemsCount);
            
            _enhancedScroller.ReloadData();
            if (itemsCount > 0)
            {
                JumpToLastSavedPosition();
            }
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            
            _positionIndicator.CleanUp();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        protected void OnItemClicked(int index)
        {
            _enhancedScrollerSnapping.Snap(index);
            _positionIndicator.Refresh(index);
        }
        protected virtual void JumpToLastSavedPosition()
        {
            _savedCellIndex = Mathf.Clamp(_savedCellIndex, 0, ContextData.StylePresets.Length - 1);
            _enhancedScroller.JumpToDataIndex(_savedCellIndex, _enhancedScroller.snapJumpToOffset, _enhancedScroller.snapCellCenterOffset, _enhancedScroller.snapUseCellSpacing);
            _enhancedScroller._RefreshActive();
            OnScrollerSnapped(_savedCellIndex);
        }
        
        protected virtual void OnScrollerSnapped(int index)
        {
            SelectedStyle = ContextData.StylePresets[index];
            _savedCellIndex = index;
            _positionIndicator.Refresh(index);
        }
        
        private Sprite GetElementBackground(int dataIndex)
        {
            var spriteIndex = dataIndex % _backgroundImages.Count;
            return _backgroundImages[spriteIndex];
        }
    }
}