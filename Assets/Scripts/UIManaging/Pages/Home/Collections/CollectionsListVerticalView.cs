using System.Collections.Generic;
using Bridge.Models.ClientServer.ThemeCollection;
using EnhancedUI.EnhancedScroller;
using Extensions;
using UnityEngine;

namespace UIManaging.Pages.Home
{
    internal sealed class CollectionsListVerticalView : CollectionsListView
    {
        [SerializeField] private GameObject _containerPrefab;
        
        private const float CELL_HEIGHT = 760F;
        private readonly List<ContainerData> _containersData = new();
        private ContainerData _currentContainer;
        private RectTransform[] _views;

        protected override void OnInitialized()
        {
            ProcessData();
            base.OnInitialized();
        }

        private void ProcessData()
        {
            _containersData.Clear();
            if (ContextData?.Collections == null) return;

            ContainerData pendingContainer = null;

            foreach (var collection in ContextData.Collections)
            {
                if (collection.HasLargeMarketingThumbnail)
                {
                    _containersData.Add(new ContainerData { BigItem = collection });
                }
                else
                {
                    if (pendingContainer == null)
                    {
                        pendingContainer = new ContainerData { FirstSmallItem = collection };
                        _containersData.Add(pendingContainer);
                    }
                    else
                    {
                        pendingContainer.SecondSmallItem = collection;
                        pendingContainer = null;
                    }
                }
            }

            // If we have an unpaired small item, move its container to the end
            if (pendingContainer != null)
            {
                _containersData.Remove(pendingContainer);
                _containersData.Add(pendingContainer);
            }

            _views = new RectTransform[_containersData.Count];
        }

        public override int GetNumberOfCells(EnhancedScroller scroller)
        {
            return _containersData.Count;
        }

        public override float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return _views[dataIndex] ? _views[dataIndex].GetSize().y : CELL_HEIGHT;
        }

        public override EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var containerData = _containersData[dataIndex];
            var containerCell = scroller.GetCellView(_containerPrefab.GetComponent<EnhancedScrollerCellView>());
            var container = containerCell.GetComponent<RectTransform>();

            // Clear existing children
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }

            if (containerData.BigItem is not null)
            {
                var bigCell = Instantiate(_bigPrefab, container);
                var bigView = bigCell.GetComponent<CollectionTileView>();
                bigView.Initialize(containerData.BigItem);
            }
            else
            {
                var firstSmallCell = Instantiate(_smallPrefab, container);
                var firstView = firstSmallCell.GetComponent<CollectionTileView>();
                firstView.Initialize(containerData.FirstSmallItem);

                var secondSmallCell = Instantiate(_smallPrefab, container);
                var secondView = secondSmallCell.GetComponent<CollectionTileView>();
                if (containerData.SecondSmallItem != null)
                {
                    secondView.Initialize(containerData.SecondSmallItem);
                }
                else
                {
                    secondView.Initialize(null);
                }
            }

            _views[dataIndex] = containerCell.transform as RectTransform;

            return containerCell;
        }

        private class ContainerData
        {
            public ThemeCollectionInfo BigItem;
            public ThemeCollectionInfo FirstSmallItem;
            public ThemeCollectionInfo SecondSmallItem;
        }
    }
}