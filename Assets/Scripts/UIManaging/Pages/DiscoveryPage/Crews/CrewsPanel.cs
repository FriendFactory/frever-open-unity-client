using System.Collections.Generic;
using Bridge;
using Bridge.Models.ClientServer.Crews;
using Modules.Amplitude;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Common.SearchPanel;
using UIManaging.EnhancedScrollerComponents;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.DiscoveryPage
{
    public class CrewsPanel : MonoBehaviour
    {
        [SerializeField] private CrewsListView _crewsList;
        [SerializeField] private SearchPanelView _searchPanelView;
        [SerializeField] private GameObject _noResultPanel;

        [Inject] private IBridge _bridge;
        [Inject] private PageManager _pageManager;
        [Inject] private LocalUserDataHolder _userData;
        [Inject] private AmplitudeManager _amplitudeManager; 

        private CrewsLoader _crewsLoader;

        public void Show(string searchPanelText, bool isBacked)
        {
            if (!isBacked || _crewsLoader == null)
            {
                _crewsLoader = new CrewsLoader(_bridge, _amplitudeManager);
                _crewsList.Initialize(new BaseEnhancedScroller<CrewShortInfo>(new List<CrewShortInfo>()));
            }
           
            gameObject.SetActive(true);
            OnSearchInputCompleted(searchPanelText);
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
            UnsubscribeEvents();
        }
        
        private void SubscribeEvents()
        {
            _searchPanelView.InputCompleted += OnSearchInputCompleted;
            _searchPanelView.InputCleared += OnSearchInputCleared;
            _crewsList.OnCellClick -= OpenCrewPage;
            _crewsList.OnCellClick += OpenCrewPage;
            _crewsLoader.NewPageAppended += OnFirstLoaded;
            _crewsLoader.LastPageLoaded += OnLastPageLoaded;
        }

        private void UnsubscribeEvents()
        {
            _searchPanelView.InputCompleted -= OnSearchInputCompleted;
            _searchPanelView.InputCleared -= OnSearchInputCleared;
            _crewsList.ScrolledToOneRectFromBottom -= OnScrollerAtBottom;
            _crewsList.OnCellClick -= OpenCrewPage;

            if (_crewsLoader != null)
            {
                _crewsLoader.NewPageAppended -= OnPageLoaded;
                _crewsLoader.LastPageLoaded -= OnLastPageLoaded;
            }
        }
        
        private void OnSearchInputCleared()
        {
            OnSearchInputCompleted(string.Empty);
        }

        private void OnSearchInputCompleted(string input)
        {
            _crewsLoader.Filter = input;
            _crewsLoader.Reset();
            UnsubscribeEvents();
            SubscribeEvents();
            _noResultPanel.SetActive(false);
            _crewsLoader.DownloadFirstPage();
        }
        
        private void OnFirstLoaded()
        {
            _crewsLoader.NewPageAppended -= OnFirstLoaded;
            _crewsLoader.NewPageAppended += OnPageLoaded;
            _crewsList.ScrolledToOneRectFromBottom += OnScrollerAtBottom;
            _crewsList.ContextData.SetItems(_crewsLoader.Models);
        }

        private void OnPageLoaded()
        {
            _crewsList.Resize();
        }

        private void OnLastPageLoaded()
        {
            if (_crewsLoader.Models.Count == 0)
            {
                _noResultPanel.SetActive(true);
                _crewsList.ContextData.SetItems(new List<CrewShortInfo>());
            }
            else
            {
                _noResultPanel.SetActive(false);
                _crewsList.Resize();
            }

            _crewsLoader.NewPageAppended -= OnFirstLoaded;
            _crewsLoader.NewPageAppended -= OnPageLoaded;
            _crewsLoader.LastPageLoaded -= OnLastPageLoaded;
            _crewsList.ScrolledToOneRectFromBottom -= OnScrollerAtBottom;
        }

        private void OnScrollerAtBottom()
        {
            _crewsLoader.DownloadNextPage();
        }

        private void OpenCrewPage(CrewShortInfo cellModel)
        {
            if (_userData.UserProfile.CrewProfile?.Id == cellModel.Id)
            {
                _pageManager.MoveNext(new CrewPageArgs());
            }
            else
            {
                _pageManager.MoveNext(new CrewInfoPageArgs(cellModel));
            }
        }
    }
}