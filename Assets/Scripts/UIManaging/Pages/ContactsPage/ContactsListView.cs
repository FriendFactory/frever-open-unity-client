using System;
using System.Linq;
using Abstract;
using EnhancedUI.EnhancedScroller;
using UIManaging.Pages.ContactsPage.CellViews;
using UnityEngine;

namespace UIManaging.Pages.ContactsPage
{
    public sealed class ContactsListView : BaseContextDataView<ContactsListModel>, IEnhancedScrollerDelegate
    {
        [SerializeField] private EnhancedScroller _enhancedScroller;
        [SerializeField] private ContactsCellView[] _contactsCellViews;

        private void Awake()
        {
            _enhancedScroller.Delegate = this;
        }
        
        protected override void OnInitialized()
        {
            if (IsContactsEmpty()) return;
            
            OnContactsChanged();
        }

        private void OnContactsChanged()
        {
            _enhancedScroller.ReloadData();
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return IsContactsEmpty() ? 0 : ContextData.Contacts.Length;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return _contactsCellViews.First().GetComponent<RectTransform>().rect.height;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var contactModel = ContextData.Contacts[dataIndex];
            var contactCellViewPrefab = _contactsCellViews.First(x=>x.Type == contactModel.Type);
            var cellView = scroller.GetCellView(contactCellViewPrefab) as ContactsCellView;
            cellView.Initialize(ContextData.Contacts[dataIndex]);
            return cellView;
        }

        private bool IsContactsEmpty()
        {
            return ContextData?.Contacts == null || ContextData.Contacts.Length == 0;
        }
    }
}
