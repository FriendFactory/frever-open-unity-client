using Abstract;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace UIManaging.Common.SelectionPanel
{
    public abstract class SelectionPanelView<TSelectionPanelModel, TSelectionItemModel> : BaseContextDataView<TSelectionPanelModel>, IEnhancedScrollerDelegate
        where TSelectionPanelModel: class, ISelectionPanelModel<TSelectionItemModel>
        where TSelectionItemModel: class, ISelectionItemModel
    {
        [SerializeField] private EnhancedScroller _enhancedScroller;
        [SerializeField] private EnhancedScrollerCellView _itemPrefab;

        private float _itemWidth;

        protected EnhancedScroller EnhancedScroller => _enhancedScroller;
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _enhancedScroller.Delegate = this;
            _itemWidth = _itemPrefab.GetComponent<RectTransform>().rect.width;
        }
            
        //---------------------------------------------------------------------
        // BaseContextDataView
        //---------------------------------------------------------------------
            
        protected override void OnInitialized()
        {
            ContextData.ItemSelectionChanged += OnItemSelectionChanged;
            
            _enhancedScroller.ReloadData();
        }

        protected override void BeforeCleanup()
        {
            if (ContextData != null)
            {
                ContextData.ItemSelectionChanged -= OnItemSelectionChanged;
            }
            
            base.BeforeCleanup();
        }
        
        //---------------------------------------------------------------------
        // IEnhancedScrollerDelegate
        //---------------------------------------------------------------------
        
        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return ContextData?.SelectedItems?.Count ?? 0;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return _itemWidth;
        }

        public virtual EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var item = scroller.GetCellView(_itemPrefab);

            var contextDataView = item.GetComponent<BaseContextDataView<TSelectionItemModel>>();
            contextDataView.Initialize(ContextData.SelectedItems[dataIndex]);

            return item;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected void Resize()
        {
            if (!_enhancedScroller.gameObject.activeInHierarchy) return;
            _enhancedScroller._RecycleAllCells();
            _enhancedScroller._Resize(true);
            _enhancedScroller._RefreshActive();
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnItemSelectionChanged(TSelectionItemModel item)
        {
            Resize();
        }
    }

    public sealed class SelectionPanelView : SelectionPanelView<SelectionPanelModel, ISelectionItemModel> { }
}