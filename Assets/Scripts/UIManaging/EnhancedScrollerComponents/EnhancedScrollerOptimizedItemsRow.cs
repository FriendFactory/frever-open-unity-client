using System;
using System.Collections.Generic;
using System.Linq;
using Abstract;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.EnhancedScrollerComponents
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class EnhancedScrollerOptimizedItemsRow<V, M> : MonoBehaviour, IEnhancedScrollerDelegate,
        IEnhancedScrollerRowItem<M> where V : BaseContextDataView<M>
    {
        private const float NEXT_PAGE_SCROLL_POSITION_THRESHOLD = 0.1f;
        
        [SerializeField] private bool _overrideCellSize = true;
        [SerializeField] protected float CellSize = 250f;

        [SerializeField] private EnhancedScroller _enhancedScroller;
        [SerializeField] private EnhancedScrollerCellView _cellViewPrefab;
        
        private List<M> _items;

        public IReadOnlyList<M> Items => _items;

        public EnhancedScroller EnhancedScroller => _enhancedScroller;

        public event Action ScrolledToBottom;

        protected EnhancedScrollerCellView CellViewPrefab => _cellViewPrefab;

        private void Awake()
        {
            _enhancedScroller.scrollerScrolled += OnScroll;
            _enhancedScroller.Delegate = this;
        }

        private void OnDestroy()
        {
            _enhancedScroller.scrollerScrolled -= OnScroll;
            _enhancedScroller.cellViewInstantiated = null;
            _enhancedScroller.cellViewReused = null;
            _enhancedScroller.cellViewWillRecycle = null;
        }

        public void SetScrollable(bool value)
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

        public void Setup(M[] itemsModels)
        {
            _items = itemsModels?.ToList() ?? new List<M>();
            _enhancedScroller.ReloadData();
        }

        public void AddItems(IEnumerable<M> itemsModels, bool append)
        {
            var itemsModelsArray = itemsModels as M[] ?? itemsModels.ToArray();
            
            if (append || _items.Count == 0)
            {
                _items.AddRange(itemsModelsArray);
            }
            else
            {
                _items.InsertRange(0, itemsModelsArray);
            }
            
            if (!append)
            {
                _enhancedScroller.Container.pivot = new Vector2(1f, 0.5f);
                _enhancedScroller.Container.anchoredPosition += new Vector2(_enhancedScroller.Container.rect.width, 0);
            }
            
            _enhancedScroller._Resize(true); // TODO: make separate method that handles prepending assets
            
            if (!append)
            {
                _enhancedScroller.Container.pivot = new Vector2(0f, 0.5f);
                _enhancedScroller.Container.anchoredPosition -= new Vector2(_enhancedScroller.Container.rect.width, 0);
            }
        }

        public int GetNumberOfCells(EnhancedScroller scroller) => _items?.Count ?? 0;

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            if (_overrideCellSize)
            {
                return CellSize;
            }

            var prefabRectTransform = _cellViewPrefab.GetComponent<RectTransform>();

            return _enhancedScroller.scrollDirection == EnhancedScroller.ScrollDirectionEnum.Horizontal ? prefabRectTransform.sizeDelta.x : prefabRectTransform.sizeDelta.y;
        }

        public virtual EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = scroller.GetCellView(_cellViewPrefab);
            var view = cellView.GetComponent<V>();

#if UNITY_EDITOR
            view.name = $"[{_cellViewPrefab.name}] {dataIndex}";
#endif
            view.Initialize(_items[dataIndex]);
            return cellView;
        }
        
        private void OnScroll(EnhancedScroller scroller, Vector2 val, float scrollPosition)
        {
            var scrolledToBottom = _enhancedScroller.NormalizedScrollPosition <= NEXT_PAGE_SCROLL_POSITION_THRESHOLD;
            if (!scrolledToBottom) return;
            
            ScrolledToBottom?.Invoke();
        }
    }
}