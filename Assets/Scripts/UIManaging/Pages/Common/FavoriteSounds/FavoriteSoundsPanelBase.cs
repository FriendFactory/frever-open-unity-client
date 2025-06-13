using System.Collections.Generic;
using Bridge.ClientServer.Assets.Music;
using Common.Abstract;
using UIManaging.EnhancedScrollerComponents;
using UIManaging.Pages.FavoriteSounds;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Common.FavoriteSounds
{
    public abstract class FavoriteSoundsPanelBase<TItem, TList> : BaseContextlessPanel
        where TItem : SoundItemBase<UsedSoundItemModel>
        where TList : BaseEnhancedScrollerView<TItem, UsedSoundItemModel> 
    {
        [SerializeField] private TList _favoriteSoundsList;
        [SerializeField] private GameObject _emptyListPanel;
        
        [Inject] private IFavouriteMusicService _favouriteMusicService; 
        
        private FavoriteSoundsListModel _favoriteSoundsListModel;

        protected abstract bool PremiumEnabled { get; }
        protected abstract bool CommercialOnly { get; }
        protected FavoriteSoundsListModel FavoriteSoundsListModel => _favoriteSoundsListModel;
        protected TList FavoriteSoundsList => _favoriteSoundsList; 

        private void Awake()
        {
            _emptyListPanel.SetActive(false);
        }

        protected override void OnInitialized()
        {
            _favoriteSoundsListModel = new FavoriteSoundsListModel(_favouriteMusicService, PremiumEnabled, CommercialOnly);
            _favoriteSoundsList.Initialize(new BaseEnhancedScroller<UsedSoundItemModel>(new List<UsedSoundItemModel>()));

            _favoriteSoundsList.OnCellViewInstantiatedEvent += OnItemInstantiated;

            _favoriteSoundsListModel.NewPageAppended += OnFirstPageLoaded;
            _favoriteSoundsListModel.LastPageLoaded += OnLastPageLoaded;
            
            _favoriteSoundsListModel.DownloadFirstPage();
        }

        protected override void BeforeCleanUp()
        {
            _favoriteSoundsListModel.Reset();
            
            Unsubscribe();
        }
        
        protected virtual void OnItemInstantiated(TItem item) { }

        protected void OnFirstPageLoaded()
        {
            _favoriteSoundsList.ContextData.SetItems(_favoriteSoundsListModel.ItemModels);
            
            _emptyListPanel.SetActive(_favoriteSoundsListModel.ItemModels.Count == 0);
            
            _favoriteSoundsListModel.NewPageAppended -= OnFirstPageLoaded;
            _favoriteSoundsListModel.NewPageAppended += OnNextPageLoaded;
            
            _favoriteSoundsList.ScrolledToOneRectFromBottom += DownloadNextPage;
        }       
        
        private void OnNextPageLoaded()
        {
            _favoriteSoundsList.Resize();
        }

        private void OnLastPageLoaded()
        {
            // in certain cases when the number of items is 0 last page loaded event triggers instead of the first page
            _emptyListPanel.SetActive(_favoriteSoundsListModel.ItemModels.Count == 0);
            
            Unsubscribe();
        }

        private void DownloadNextPage() => _favoriteSoundsListModel.DownloadNextPage();

        private void Unsubscribe()
        {
            _favoriteSoundsListModel.NewPageAppended -= OnFirstPageLoaded;
            _favoriteSoundsListModel.NewPageAppended -= OnNextPageLoaded;
            _favoriteSoundsListModel.LastPageLoaded -= OnLastPageLoaded;
            
            _favoriteSoundsList.ScrolledToOneRectFromBottom -= DownloadNextPage;
        }
    }
}