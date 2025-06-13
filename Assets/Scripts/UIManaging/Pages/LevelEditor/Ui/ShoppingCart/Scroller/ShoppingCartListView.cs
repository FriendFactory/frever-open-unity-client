using System;
using Abstract;
using EnhancedUI.EnhancedScroller;
using TMPro;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.ShoppingCart
{
    internal sealed class ShoppingCartListView : BaseContextDataView<ShoppingCartListModel>, IEnhancedScrollerDelegate
    {
        [SerializeField] private EnhancedScroller _enhancedScroller;
        [SerializeField] private EnhancedScrollerCellView _assetView;
        [SerializeField] private EnhancedScrollerCellView _headerView;
        [SerializeField] private TextMeshProUGUI _noItemsText;

        private float _assetCellHeight;
        private float _headerCellHeight;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action<ShoppingCartItemModel> ItemClicked;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _enhancedScroller.Delegate = this;
            _assetCellHeight = _assetView.GetComponent<RectTransform>().rect.height;
            _headerCellHeight = _headerView.GetComponent<RectTransform>().rect.height;

            if (_noItemsText == null) return;
            UpdateNoMatchText();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Reload()
        {
            if (IsDestroyed) return;
            _enhancedScroller.ReloadData();
            UpdateNoMatchText();
        }

        //---------------------------------------------------------------------
        // IEnhancedScrollerDelegate
        //---------------------------------------------------------------------

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return ContextData == null ? 0 : ContextData.Items.Length;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            switch (ContextData.Items[dataIndex])
            {
                case ShoppingCartAssetModel _:
                    return _assetCellHeight;
                case ShoppingCartHeaderModel _:
                    return _headerCellHeight;
                default:
                    return 0;
            }
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellItem = ContextData.Items[dataIndex];
            switch (ContextData.Items[dataIndex])
            {
                case ShoppingCartAssetModel cellAsset:
                {
                    var cellView = scroller.GetCellView(_assetView);
                    var itemView = cellView.GetComponent<ShoppingCartAssetView>();
                    var ordinal = dataIndex + (cellAsset.IsConfirmed ? 1 : 0);
                    itemView.Initialize(cellAsset, ordinal);
                    itemView.ItemClicked += OnItemClicked;
                    return cellView;
                }
                case ShoppingCartHeaderModel cellHeader:
                {
                    var cellView = scroller.GetCellView(_headerView);
                    var itemView = cellView.GetComponent<ShoppingCartHeaderView>();
                    itemView.Initialize(cellHeader);
                    return cellView;
                }
                default:
                {
                    Debug.LogWarning($"Cell type is not supported: {cellItem?.GetType()}");
                    return null;
                }
            }
        }

        //---------------------------------------------------------------------
        // BaseContextDataView
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            Reload();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnItemClicked(ShoppingCartItemModel item)
        {
            ItemClicked?.Invoke(item);
        }

        private void UpdateNoMatchText()
        {
            var hasItems = ContextData?.Items.Length > 0;
            _noItemsText.gameObject.SetActive(!hasItems);
        }
    }
}