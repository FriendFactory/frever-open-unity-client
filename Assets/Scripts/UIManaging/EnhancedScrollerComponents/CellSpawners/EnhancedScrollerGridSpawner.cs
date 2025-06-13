using System;
using System.Collections.Generic;
using System.Linq;
using Abstract;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.EnhancedScrollerComponents.CellSpawners
{
    public abstract class
        EnhancedScrollerGridSpawner<Cell, Row, Model> : BaseContextDataView<EnhancedScrollerGridSpawnerModel<Model>>, IEnhancedScrollerDelegate
        where Cell : BaseContextDataView<Model>
        where Row : EnhancedScrollerOptimizedItemsRow<Cell, Model>
    {
        [SerializeField] private int itemsPerRow = 4;
        [SerializeField] protected float RowSize = 250f;
        [SerializeField] private EnhancedScroller _enhancedScroller;
        [SerializeField] private EnhancedScrollerCellView _rowPrefab;

        private bool _areRowsScrollable = true;
        private float _firstRowScrollPosition;

        public event Action<Row> OnRowCreated;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        private bool IsGridScrollable => _enhancedScroller.ScrollRect.enabled;
        public EnhancedScroller EnhancedScroller
        {
            get => _enhancedScroller;
            set
            {
                if(_enhancedScroller != null) _enhancedScroller.Delegate = null;
                _enhancedScroller = value;
                _enhancedScroller.Delegate = this;
            }
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected virtual void Awake()
        {
            _enhancedScroller.Delegate = this;
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            _enhancedScroller.cellViewInstantiated = null;
            _enhancedScroller.cellViewReused = null;
            _enhancedScroller.cellViewWillRecycle = null;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void SetRowPrefab(EnhancedScrollerCellView rowPrefab)
        {
            _rowPrefab = rowPrefab;
        }

        public EnhancedScroller GetFirstRowEnhancedScroller()
        {
            var row = transform.GetComponentInChildren<Row>();
            return row != null ? row.EnhancedScroller : null;
        }
        
        public void SetFirstRowScrollPosition(int itemIndex, bool refreshForCurrentRows = false)
        {
            var row = GetComponentInChildren<Row>();
            if(row == null) return;
            
            var scroller = row.EnhancedScroller;
            
            _firstRowScrollPosition = Mathf.Max(0f, scroller.GetScrollPositionForCellViewIndex(itemIndex, EnhancedScroller.CellViewPositionEnum.Before)) 
                                    + scroller.snapCellCenterOffset * row.GetCellViewSize(null, 0);

            
            if (refreshForCurrentRows)
            {
                scroller.ScrollPosition = _firstRowScrollPosition;
            }
        }

        public void SetScrollPosition(int itemIndex)
        {
            EnhancedScroller.ScrollPosition = EnhancedScroller.GetScrollPositionForCellViewIndex(itemIndex, EnhancedScroller.CellViewPositionEnum.Before);
        }

        public void SetGridScrollable(bool value)
        {
            if (_enhancedScroller.ScrollRect != null)
            {
                _enhancedScroller.ScrollRect.enabled = value;
            }
            else
            {
                _enhancedScroller.GetComponent<ScrollRect>().enabled = value;
            }
        }

        public void SetRowsScrollable(bool value)
        {
            _areRowsScrollable = value;

            var rows = GetComponentsInChildren<Row>();

            for (int i = 0; i < rows.Length; i++)
            {
                rows[i].SetScrollable(value);
            }
        }

        public void SetAmountOfItemsPerRowSilent(int amount)
        {
            itemsPerRow = amount;
        }

        public void SetAmountOfItemsPerRow(int amount)
        {
            itemsPerRow = amount;
            _enhancedScroller.ReloadData();
        }
        
        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var enhancedCellView = scroller.GetCellView(_rowPrefab);
            var rowView = enhancedCellView.GetComponent<Row>();
            var itemsToSkip = dataIndex * itemsPerRow;
#if UNITY_EDITOR
            var viewDescription = "<Empty>";

            if (_areRowsScrollable)
            {
                viewDescription = $"{0}-{ContextData.Items.Count - 1}";
            }
            else if (itemsPerRow > 0)
            {
                viewDescription = $"{itemsToSkip}-{itemsToSkip + itemsPerRow - 1}";
            }

            rowView.name = $"[{_rowPrefab.name}] {viewDescription}";
#endif
            var selectedModels = _areRowsScrollable 
                ? ContextData.Items.ToArray()
                : ContextData.Items.Skip(itemsToSkip).Take(itemsPerRow).ToArray();
            rowView.SetScrollable(_areRowsScrollable);
            rowView.Setup(selectedModels);
            rowView.EnhancedScroller.ScrollPosition = _firstRowScrollPosition;
            
            OnRowCreated?.Invoke(rowView);
            
            return enhancedCellView;
        }
        
        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            if (ContextData == null)
            {
                return 0;
            }
            
            return _areRowsScrollable ? 1 : Mathf.CeilToInt((float) ContextData.Items.Count / itemsPerRow);
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return RowSize;
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            ContextData.OnItemsChangedEvent += OnDataSet;
            ContextData.OnItemsAddedEvent += AddData;
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();

            if (ContextData != null)
            {
                ContextData.OnItemsChangedEvent -= OnDataSet;
                ContextData.OnItemsAddedEvent -= AddData;
            }
            
            _enhancedScroller.ClearAll();
        }

        protected void ReloadData()
        {
            _enhancedScroller.ReloadData();
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnDataSet(List<Model> models)
        {
            ReloadData();
        }
        
        private void AddData(List<Model> models, bool append)
        {
            if (_areRowsScrollable)
            {
                transform.GetComponentInChildren<Row>().AddItems(models, append);
            }
            else
            {
                if (!append)
                {
                    _enhancedScroller.Container.pivot = new Vector2(0.5f, 0);
                    _enhancedScroller.Container.anchoredPosition -= new Vector2(0, _enhancedScroller.Container.rect.height);
                }
                
                _enhancedScroller._Resize(true); // TODO: make separate method that handles prepending assets

                if (!append)
                {
                    _enhancedScroller.Container.pivot = new Vector2(0.5f, 1);
                    _enhancedScroller.Container.anchoredPosition += new Vector2(0, _enhancedScroller.Container.rect.height);
                }
                
                var rowsToUpdate = _enhancedScroller.GetComponentsInChildren<Row>()
                                               .Where(row => row.Items.Count < itemsPerRow);

                foreach (var row in rowsToUpdate)
                {
                    row.Setup(row.Items.Concat(models.Take(itemsPerRow - row.Items.Count)).ToArray());
                }                                 
            }
        }
    }
}