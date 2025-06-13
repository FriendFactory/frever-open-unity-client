using Abstract;
using Extensions;
using UIManaging.Pages.Common.SongOption.SongList;
using UnityEngine;

namespace UIManaging.Pages.Common.SongOption.Search
{
    internal abstract class BasePlayableSearchView<TItemModel, TListModel, TSearchList> : MusicSearchTypeView
        where TItemModel : PlayableItemModel
        where TListModel : class, ISearchListModel<TItemModel>
        where TSearchList: BaseContextDataView<TListModel>
    {
        [SerializeField] protected TSearchList _playableSearchList;
        [SerializeField] protected CanvasGroup _notFoundPanel;

       protected TListModel SearchListModel { get; set; }

        protected override void OnInitialized()
        {
            SearchListModel = ContextData.GetListModel<TListModel>(MusicSearchType);

            if (SearchListModel == null)
            {
                Debug.LogError($"[{GetType().Name}] Failed to cast search list model");
                return;
            }
                
            SearchListModel.DataChanged += OnModelChanged;
            SearchListModel.FetchFailed += OnFetchFailed;
            
            _playableSearchList.Initialize(SearchListModel);
        }

        protected override void BeforeCleanUp()
        {
            if (SearchListModel == null) return;

            SearchListModel.DataChanged -= OnModelChanged;
            SearchListModel.FetchFailed -= OnFetchFailed;
        }

        private void OnModelChanged()
        {
            _notFoundPanel.SetActive(SearchListModel.FirstPageLoaded && SearchListModel.Models.Count == 0);
        }

        private void OnFetchFailed()
        {
            _notFoundPanel.SetActive(true);
        }

        protected override void OnShow()
        {
            _notFoundPanel.SetActive(false);
        }
    }
}