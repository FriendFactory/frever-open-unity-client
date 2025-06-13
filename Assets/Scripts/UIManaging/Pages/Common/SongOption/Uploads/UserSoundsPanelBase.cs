using System.Collections.Generic;
using Abstract;
using Bridge.Models.Common;
using Common.Abstract;
using UIManaging.EnhancedScrollerComponents;
using UnityEngine;

namespace UIManaging.Pages.Common.SongOption.Uploads
{
    internal abstract class UserSoundsPanelBase<TModel, TItemModel, TListModel, TItem, TList>: BaseContextPanel<TListModel> 
        where TModel : IEntity
        where TListModel: UserSoundsListModelBase<TModel, TItemModel>
        where TList : BaseEnhancedScrollerView<TItem, TItemModel>
        where TItem : BaseContextDataView<TItemModel>
    {
        [SerializeField] protected TList _userSoundsList;
        [SerializeField] protected GameObject _emptyListPanel;

        private void Awake()
        {
            _emptyListPanel.SetActive(false);
        }

        protected override void OnInitialized()
        {
            _userSoundsList.Initialize(new BaseEnhancedScroller<TItemModel>(new List<TItemModel>()));

            ContextData.NewPageAppended += OnFirstPageLoaded;
            ContextData.LastPageLoaded += OnLastPageLoaded;
            
            ContextData.DownloadFirstPage();
        }

        protected override void BeforeCleanUp()
        {
            ContextData.Reset();
            
            Unsubscribe();
        }

        protected void OnFirstPageLoaded()
        {
            _userSoundsList.ContextData.SetItems(ContextData.ItemModels);
            
            _emptyListPanel.SetActive(ContextData.ItemModels.Count == 0);

            ContextData.NewPageAppended -= OnFirstPageLoaded;
            ContextData.NewPageAppended += OnNextPageLoaded;

            _userSoundsList.ScrolledToOneRectFromBottom += DownloadNextPage;
        }

        private void DownloadNextPage() => ContextData.DownloadNextPage();

        private void OnNextPageLoaded()
        {
            _userSoundsList.Resize();
        }

        private void OnLastPageLoaded()
        {
            Unsubscribe();
        }

        private void Unsubscribe()
        {
            ContextData.NewPageAppended -= OnFirstPageLoaded;
            ContextData.NewPageAppended -= OnNextPageLoaded;
            
            _userSoundsList.ScrolledToOneRectFromBottom -= DownloadNextPage;
        }
    }
}