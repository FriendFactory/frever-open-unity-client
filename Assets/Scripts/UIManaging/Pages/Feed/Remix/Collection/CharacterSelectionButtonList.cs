using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace UIManaging.Pages.Feed.Remix.Collection
{
    internal class CharacterSelectionButtonList : BaseCharacterSelectionButtonList<CharacterSelectionListModel>
    {
        [SerializeField] private EnhancedScrollerCellView _categoryHeader;

        private float _categoryCellViewSize;

        protected override void Awake()
        {
            base.Awake();

            _categoryCellViewSize = _categoryHeader.GetComponent<RectTransform>().rect.height;
        }

        public override int GetNumberOfCells(EnhancedScroller scroller) => ContextData.CharacterCellViewModels.Count;

        public override float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            var viewModel = ContextData.CharacterCellViewModels[dataIndex];
            
            return viewModel.IsHeader ? _categoryCellViewSize : base.GetCellViewSize(scroller, dataIndex);
        }

        public override EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var viewModel = ContextData.CharacterCellViewModels[dataIndex];

            if (viewModel.IsHeader)
            {
                var headerModel = ContextData.CharacterCategoryModels[viewModel.Index];
                var headerCellView = scroller.GetCellView(_categoryHeader);
                var header = headerCellView.GetComponent<CharacterSelectionCategoryHeader>();

                header.SetHeader(headerModel.CategoryName);

                return headerCellView;
            }

            var characterModel = ContextData.Models[viewModel.Index];
            var cellView = scroller.GetCellView(_characterButton);
            var characterButton = cellView.GetComponent<CharacterButton>();
            characterButton.Initialize(characterModel);

            return cellView;
        }
    }
}