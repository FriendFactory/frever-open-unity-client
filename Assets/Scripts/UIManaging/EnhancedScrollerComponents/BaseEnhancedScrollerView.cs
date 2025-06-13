using System;
using Abstract;
using EnhancedUI.EnhancedScroller;
using Sirenix.OdinInspector;
using UIManaging.EnhancedScrollerComponents.CellSpawners;
using UnityEngine;

namespace UIManaging.EnhancedScrollerComponents
{
    public abstract class BaseEnhancedScrollerView<V, M> : BaseContextDataView<BaseEnhancedScroller<M>>, IEnhancedScrollerDelegate where V : BaseContextDataView<M>
    {
        public event Action<V> OnCellViewInstantiatedEvent; 
        public event Action<V> OnCellViewReusedEvent; 
        public event Action<V> OnCellViewRecycledEvent;
        public event Action ScrolledToOneRectFromBottom;

        [SerializeField] protected EnhancedScroller _scroller;
        
        [SerializeField] private bool _overrideCellSize;
        [SerializeField, ShowIf(nameof(CanShowOverridenCellSizeField))] private float _overridenCellSize = 100f;
        [SerializeField] private EnhancedScrollerCellView _cellViewPrefab;
        [SerializeField] private BaseEnhancedScrollerSpawner _enhancedScrollerSpawner;

        protected virtual void Awake()
        {
            _scroller.Delegate = this;
            _scroller.cellViewInstantiated = OnCellViewInstantiated;
            _scroller.cellViewReused = OnCellViewReused;
            _scroller.cellViewWillRecycle = OnCellViewRecycled;
            _scroller.scrollerScrolled = OnScrollerScrolled;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _scroller.cellViewInstantiated = null;
            _scroller.cellViewReused = null;
            _scroller.cellViewWillRecycle = null;
        }

        public void Resize()
        {
            _scroller._Resize(true);
        }

        private void OnCellViewInstantiated(EnhancedScroller scroller, EnhancedScrollerCellView cellView)
        {
            var component = cellView.GetComponent<V>();
            OnCellViewInstantiatedEvent?.Invoke(component);
        }

        private void OnCellViewReused(EnhancedScroller scroller, EnhancedScrollerCellView cellView)
        {
            var component = cellView.GetComponent<V>();
            OnCellViewReusedEvent?.Invoke(component);
        }
        
        private void OnCellViewRecycled(EnhancedScrollerCellView cellView)
        {
            var component = cellView.GetComponent<V>();
            OnCellViewRecycledEvent?.Invoke(component);
        }

        protected override void OnInitialized()
        {
            _scroller.ReloadData();
            ContextData.OnItemsChangedEvent += OnItemsChanged;
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            if (ContextData == null) return;
            ContextData.OnItemsChangedEvent -= OnItemsChanged;
        }

        private void OnItemsChanged()
        {
            _scroller.ReloadData();
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return ContextData?.Items?.Count ?? 0;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            if (_overrideCellSize)
            {
                return _overridenCellSize;
            }
            
            var prefabSize = _cellViewPrefab.GetComponent<RectTransform>().rect.size;

            switch (_scroller.scrollDirection)
            {
                case EnhancedScroller.ScrollDirectionEnum.Horizontal:
                    return prefabSize.x;
                case EnhancedScroller.ScrollDirectionEnum.Vertical:
                    return prefabSize.y;
                default:
                    return 100f;
            }
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = scroller.GetCellView(_cellViewPrefab);
            _enhancedScrollerSpawner.InitializeCellView<V, M>(_cellViewPrefab.name, cellView, scroller, ContextData.Items, dataIndex, cellIndex);
            return cellView;
        }
        
        private bool CanShowOverridenCellSizeField()
        {
            return _overrideCellSize;
        }
        
        private void OnScrollerScrolled(EnhancedScroller scroller, Vector2 val, float scrollposition)
        {
            if (scrollposition + scroller.ScrollRectSize < scroller.ScrollSize) return;
            ScrolledToOneRectFromBottom?.Invoke();
        }
    }
}