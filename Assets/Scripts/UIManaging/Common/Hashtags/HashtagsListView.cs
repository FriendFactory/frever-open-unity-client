using System;
using Abstract;
using Bridge.Models.VideoServer;
using EnhancedUI.EnhancedScroller;
using TMPro;
using UnityEngine;

namespace UIManaging.Common.Hashtags
{
    internal sealed class HashtagsListView : BaseContextDataView<HashtagsListModel>, IEnhancedScrollerDelegate
    {
        [SerializeField] private EnhancedScroller _enhancedScroller;
        [SerializeField] private EnhancedScrollerCellView _hashtagView;
        [SerializeField] private TextMeshProUGUI _noMatchText;

        private float _cellHeight;
        private bool _hasNoMatchText;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action<HashtagInfo> ItemClicked;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _enhancedScroller.Delegate = this;
            _cellHeight = _hashtagView.GetComponent<RectTransform>().rect.height;

            if (_noMatchText == null) return;

            _hasNoMatchText = true;
            UpdateNoMatchText();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Reload()
        {
            if (IsDestroyed) return;
            _enhancedScroller.ReloadData();

            if (!_hasNoMatchText) return;
            UpdateNoMatchText();
        }

        //---------------------------------------------------------------------
        // IEnhancedScrollerDelegate
        //---------------------------------------------------------------------

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return ContextData == null ? 0 : ContextData.Hashtags.Length;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return _cellHeight;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = scroller.GetCellView(_hashtagView);
            var itemView = cellView.GetComponent<HashtagItemView>();
            itemView.Initialize(ContextData.Hashtags[dataIndex]);
            itemView.ItemClicked += OnItemClicked;
            return cellView;
        }

        //---------------------------------------------------------------------
        // BaseContextDataView
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnItemClicked(HashtagInfo info)
        {
            ItemClicked?.Invoke(info);
        }

        private void UpdateNoMatchText()
        {
            _noMatchText.gameObject.SetActive(ContextData.Hashtags.Length == 0);
        }
    }
}