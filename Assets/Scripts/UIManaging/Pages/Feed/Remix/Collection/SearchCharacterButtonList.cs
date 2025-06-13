using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace UIManaging.Pages.Feed.Remix.Collection
{
    internal sealed class SearchCharacterButtonList : BaseCharacterSelectionButtonList<SearchCharacterButtonListModel>
    {
        [SerializeField] private GameObject _noUserMatchPanel;

        public override int GetNumberOfCells(EnhancedScroller scroller) => ContextData.Models.Count;

        public override EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = scroller.GetCellView(_characterButton);
            var characterButton = cellView.GetComponent<CharacterButton>();
            var model = ContextData.Models[dataIndex];
            
            characterButton.Initialize(model);
            
            return cellView;
        }
        
        protected override void OnInitialized()
        {
            _noUserMatchPanel.SetActive(false);
            
            base.OnInitialized();
        }

        protected override void LastPageLoaded()
        {
            _noUserMatchPanel.SetActive(ContextData.Models.Count == 0);
            
            base.LastPageLoaded();
        }
    }
}
