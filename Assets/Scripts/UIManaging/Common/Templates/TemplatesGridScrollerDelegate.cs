using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Template;
using EnhancedUI.EnhancedScroller;
using UIManaging.EnhancedScrollerComponents;
using UnityEngine;

namespace UIManaging.Common.Templates
{
    public abstract class TemplatesGridScrollerDelegate : MonoBehaviour, IEnhancedScrollerDelegate
    {
        [Header("EnhancedScroller")]
        [SerializeField] private int _itemsInRow = 3;
        [SerializeField] private float _cellHeight = 640f;
        [SerializeField] protected EnhancedScroller _enhancedScroller;
        [SerializeField] private EnhancedScrollerCellView _templatesRowPrefab;

        protected readonly List<TemplateInfo> Templates = new List<TemplateInfo>();

        private int _lastDataIndex;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected virtual void Awake()
        {
            _enhancedScroller.cellViewVisibilityChanged += CellViewVisibilityChanged;
        }

        protected virtual void OnDestroy()
        {
            _enhancedScroller.cellViewVisibilityChanged -= CellViewVisibilityChanged;
        }

        //---------------------------------------------------------------------
        // IEnhancedScrollerDelegate
        //---------------------------------------------------------------------

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return Templates == null ? 0 : Mathf.CeilToInt((float) Templates.Count / _itemsInRow);
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return _cellHeight;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = scroller.GetCellView(_templatesRowPrefab);
            _lastDataIndex = dataIndex;
            return cellView;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void CellViewVisibilityChanged(EnhancedScrollerCellView cellView)
        {
            var rowItem = cellView.GetComponent<IEnhancedScrollerRowItem<TemplateInfo>>();
            var selectedTemplates = Templates.Skip(_lastDataIndex * _itemsInRow).Take(_itemsInRow).ToArray();
            rowItem.Setup(selectedTemplates);
        }
    }
}