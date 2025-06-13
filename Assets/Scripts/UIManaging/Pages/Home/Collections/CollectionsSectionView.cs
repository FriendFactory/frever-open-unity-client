using System.Collections.Generic;
using System.Linq;
using Abstract;
using Bridge.Models.ClientServer.ThemeCollection;
using Modules.ThemeCollection;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Home
{
    internal sealed class CollectionSectionModel
    {
        public readonly List<ThemeCollectionInfo> Collections;

        public CollectionSectionModel(IEnumerable<ThemeCollectionInfo> collections)
        {
            Collections = collections.ToList();
        }
    }

    internal sealed class CollectionsSectionView : BaseContextDataView<CollectionSectionModel>
    {
        [SerializeField] private Button _viewAllButton;
        [SerializeField] private CollectionsListView _collectionsList;

        [Inject] private IThemeCollectionService _collectionService;

        private void Awake()
        {
            _viewAllButton?.onClick.AddListener(OnViewAllClicked);
        }
        
        protected override void OnInitialized()
        {
            var model = new CollectionListModel(ContextData.Collections);
            _collectionsList.Initialize(model);
        }

        private void OnViewAllClicked()
        {
            _collectionService.ShowCollection();
            if (_viewAllButton)
            {
                _viewAllButton.interactable = false;
            }
        }
    }
}