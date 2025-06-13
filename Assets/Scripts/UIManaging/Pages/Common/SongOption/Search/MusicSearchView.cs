using System;
using System.Collections.Generic;
using System.Linq;
using Common.Abstract;
using UIManaging.Pages.Common.TabsManager;
using UnityEngine;

namespace UIManaging.Pages.Common.SongOption.Search
{
    internal abstract class MusicSearchView: BaseContextView<MusicSearchListModelsProvider>
    {
        [SerializeField] private TabsManagerView _tabsManagerView;
        [SerializeField] private List<MusicSearchTypeView> _searchTypeViews;
        
        private MusicSelectionStateController _selectionStateController;
        private Dictionary<MusicSearchType, MusicSearchTypeView> _searchTypeViewsMap;

        protected abstract int InitialTabIndex { get; }
        
        private Dictionary<MusicSearchType, MusicSearchTypeView> SearchTypeViewsMap => 
            _searchTypeViewsMap ?? (_searchTypeViewsMap = _searchTypeViews.ToDictionary(view => view.MusicSearchType, view => view));

        public event Action<MusicSearchType> TabSelectionChanged;

        protected override void OnInitialized()
        {
            _searchTypeViews.ForEach(view => view.Initialize(ContextData));

            _tabsManagerView.Init(GetTabManagerArgs());
            
            _tabsManagerView.TabSelectionCompleted += OnTabSelectionChanged;
        }

        protected override void OnShow()
        {
            ToggleTabs(InitialTabIndex);
        }

        protected override void OnHide()
        {
            HideAllTabs();
        }

        protected override void BeforeCleanUp()
        {
            _tabsManagerView.TabSelectionCompleted -= OnTabSelectionChanged;
            
            _searchTypeViews.ForEach(view => view.CleanUp());
        }

        protected abstract TabsManagerArgs GetTabManagerArgs();
        protected abstract MusicSearchType GetSearchTypeFromIndex(int tabIndex);
        
        private void HideAllTabs()
        {
            _searchTypeViews.ForEach(view => view.Hide());
        }

        private void OnTabSelectionChanged(int tabIndex)
        {
            var searchType = GetSearchTypeFromIndex(tabIndex);
            
            ToggleTabs(searchType);
            
            TabSelectionChanged?.Invoke(searchType);
        }

        private void ToggleTabs(MusicSearchType searchType)
        {
            HideAllTabs();

            if (SearchTypeViewsMap.TryGetValue(searchType, out var view))
            {
                view.Show();
            }
        }
        
        private void ToggleTabs(int activeTabIndex)
        {
            var searchType = GetSearchTypeFromIndex(activeTabIndex);
            
            ToggleTabs(searchType);
        }
    }
}