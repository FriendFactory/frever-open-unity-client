using System;
using Abstract;
using Bridge.Services.UserProfile;
using EnhancedUI.EnhancedScroller;
using TMPro;
using UnityEngine;

namespace UIManaging.Common.SearchPanel
{
    public class SearchListView : BaseContextDataView<SearchListModel>, IEnhancedScrollerDelegate
    {
        public event Action OnScrolledToLastScreen;
        
        [SerializeField] protected EnhancedScroller _enhancedScroller;
        [SerializeField] protected EnhancedScrollerCellView _searchUserView;
        [SerializeField] protected TextMeshProUGUI _noUserMatchText;

        private RectTransform _searchUserViewRect;
        
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action<long> ItemDisplayed;
        public event Action<Profile> ProfileButtonClicked;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected virtual void Awake()
        {
            _enhancedScroller.Delegate = this;
            _enhancedScroller.scrollerScrolled += OnScrolled;
            _noUserMatchText.gameObject.SetActive(false);
            _searchUserViewRect = _searchUserView.GetComponent<RectTransform>();
        }

        private void OnScrolled(EnhancedScroller scroller, Vector2 val, float scrollposition)
        {
            if (scrollposition + scroller.ScrollRectSize < scroller.ScrollSize) return;
            OnScrolledToLastScreen?.Invoke();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public virtual int GetNumberOfCells(EnhancedScroller scroller)
        {
            return ContextData == null ? 0 : ContextData.Users.Length;
        }

        public virtual float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return _searchUserViewRect.rect.height;
        }

        public virtual EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = scroller.GetCellView(_searchUserView);
            
            var itemView = cellView.GetComponent<SearchUserBaseItemView>();
            itemView.Initialize(ContextData.Users[dataIndex]);
            itemView.ProfileButtonClicked += OnProfileButtonClicked;
            ItemDisplayed?.Invoke(itemView.Id);
            
            return cellView;
        }

        public void Reload()
        {
            if (IsDestroyed) return;
            
            _enhancedScroller.ReloadData();
            UpdateNoUserMatchText(true);
        }
        
        public void Refresh()
        {
            if (IsDestroyed || !_enhancedScroller.gameObject.activeInHierarchy) return;
            
            _enhancedScroller._Resize(true);
            UpdateNoUserMatchText();
        }

        public void SetProfiles(Profile[] profiles)
        {
            ContextData.SetProfiles(profiles);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInitialized() { }

        protected virtual void UpdateNoUserMatchText(bool isReload = false)
        {
            _noUserMatchText.gameObject.SetActive(ContextData.IsSearchResult && ContextData.Users.Length == 0);
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void OnProfileButtonClicked(Profile profile)
        {
            ProfileButtonClicked?.Invoke(profile);
        }
    }
}