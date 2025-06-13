using System.Collections.Generic;
using System.Linq;
using EnhancedUI.EnhancedScroller;
using Extensions;
using UnityEngine;

namespace UIManaging.Pages.VideoMessage.Emojis
{
    internal sealed class EmotionPanelItemsScrollView: MonoBehaviour, IEnhancedScrollerDelegate
    {
        [SerializeField] private EnhancedScroller _enhancedScroller;
        [SerializeField] private EmojiUIItem _itemViewPrefab;
        private EmotionPanelItemsScrollViewModel _model;

        private void Awake()
        {
            _enhancedScroller.Delegate = this;
        }

        public void Init(EmotionPanelItemsScrollViewModel model)
        {
            _model = model;
            _enhancedScroller.ReloadData();
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return _model == null ? 0 : _model.EmojiUiItemModels.Count;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return _itemViewPrefab.GetComponent<RectTransform>().rect.height;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var emojiUiItem = scroller.GetCellView(_itemViewPrefab);
            var model = _model.EmojiUiItemModels.ElementAt(dataIndex);

            ((EmojiUIItem)emojiUiItem).Setup(model);
            return emojiUiItem;
        }

        public void RefreshSelectionState()
        {
            var shownItemViews = _enhancedScroller.GetActiveViews().Cast<EmojiUIItem>().ToArray();
            foreach (var shownItemView in shownItemViews)
            {
                shownItemView.RefreshSelectionState();
            }
        }
    }

    internal sealed class EmotionPanelItemsScrollViewModel
    {
        public List<EmojiUiItemModel> EmojiUiItemModels;
    }
}