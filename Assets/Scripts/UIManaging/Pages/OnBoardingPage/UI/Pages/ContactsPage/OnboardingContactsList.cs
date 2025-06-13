using Common.Abstract;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace UIManaging.Pages.OnBoardingPage.UI.Pages
{
    internal class OnboardingContactsList: BaseContextView<OnboardingContactsListModel>, IEnhancedScrollerDelegate
    {
        [SerializeField] private EnhancedScroller _scroller;
        [SerializeField] private EnhancedScrollerCellView _contactsItem;
        
        private float _itemHeight;
        
        protected override void OnInitialized()
        {
            _itemHeight = _contactsItem.GetComponent<RectTransform>().rect.height;

            _scroller.Delegate = this;
        }

        protected override void BeforeCleanUp()
        {
            _scroller.Delegate = null;
            _scroller.ClearAll();
        }

        public int GetNumberOfCells(EnhancedScroller scroller) => ContextData.Items.Count;

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex) => _itemHeight;

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var model = ContextData.Items[dataIndex];

            var cellView = scroller.GetCellView(_contactsItem);
            var contactsItem = cellView.GetComponent<OnboardingContactsItem>();

            if (!contactsItem.IsInitialized || contactsItem.ContextData.Id != model.Id)
            {
                contactsItem.Initialize(model);
            }

            return cellView;
        }
    }
}