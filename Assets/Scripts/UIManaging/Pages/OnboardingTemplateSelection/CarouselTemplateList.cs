using System.Collections.Generic;
using System.Linq;
using Abstract;
using Bridge.Models.ClientServer.Template;
using EnhancedUI.EnhancedScroller;
using UIManaging.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.OnboardingTemplateSelection
{
    internal sealed class CarouselTemplateList : BaseContextDataView<CarouselTemplateListModel>, IEnhancedScrollerDelegate
    {
        [SerializeField] private ScrollRectEnhancedScrollerSnapping _enhancedScrollerSnapping;
        [SerializeField] private EnhancedScroller _enhancedScroller;
        [SerializeField] private EnhancedScrollerCellView _cellViewItem;
        [SerializeField] private ToggleGroup _markersGroup;
        [SerializeField] private Toggle _markerPrefab;
        
        private readonly Dictionary<long, Toggle> _markers = new Dictionary<long, Toggle>();

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public TemplateInfo SelectedTemplateInfo { get; private set; }
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
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
        
        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return ContextData == null ? 0 : ContextData.TemplateInfo.Length;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return ContextData.CellSize;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = scroller.GetCellView(_cellViewItem);
            var templateItem = cellView.GetComponent<CarouselTemplateItem>();
            templateItem.Initialize(ContextData.TemplateInfo[dataIndex], ()=>OnTemplateClicked(dataIndex));
            return cellView;
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            _enhancedScroller.ReloadData();
            CreateMarkers();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void CreateMarkers()
        {
            for (var i = 0; i < ContextData.TemplateInfo.Length; i++)
            {
                var marker = Instantiate(_markerPrefab, _markersGroup.transform);
                marker.group = _markersGroup;
                
                var index = i;
                marker.onValueChanged.AddListener((x)=> OnMarkerClicked(x, index));
                _markers.Add(index, marker);
            }
            
            _markers.Values.First().SetIsOnWithoutNotify(true);
        }

        private void OnMarkerClicked(bool isOn, int index)
        {
            if (!isOn) return;
            
            _enhancedScrollerSnapping.Snap(index);
        }

        private void OnTemplateClicked(int index)
        {
            _enhancedScrollerSnapping.Snap(index);
        }

        private void OnScrollerSnapped(int index)
        {
            var marker = _markers[index];
            marker.isOn = true;
            SelectedTemplateInfo = ContextData.TemplateInfo[index];
        }
    }
}
