using Extensions;
using TMPro;
using UIManaging.Common.SearchPanel;
using UIManaging.Pages.Feed.Remix.Collection;
using UIManaging.Pages.Feed.Remix.Loaders;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;

namespace UIManaging.Pages.Feed.Remix
{
    internal sealed class CharacterSelectionPopup : CharacterSelectionPopupBase
    {
        [SerializeField] private TextMeshProUGUI _header;
        [SerializeField] private TextMeshProUGUI _headerDescription;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private SearchPanelView _searchPanel;
        [SerializeField] private SearchCharacterButtonList _searchCharacterButtonList;
        [SerializeField] private CanvasGroup _searchCanvasGroup;

        protected override bool CanDeselectCharacters => true;

        private SearchCharacterButtonListModel _searchCharactersListModel;
        
        //---------------------------------------------------------------------
        // Protected 
        //---------------------------------------------------------------------

        protected override void OnConfigure(CharacterSelectionPopupConfiguration configuration)
        {
            base.OnConfigure(configuration);

            _header.text = configuration.Header;
            _headerDescription.text = configuration.HeaderDescription;
            _headerDescription.SetActive(!string.IsNullOrEmpty(configuration.HeaderDescription));
            _descriptionText.text = configuration.ReasonText;
            
            _searchPanel.InputCleared += OnSearchCleared;
            _searchPanel.InputCompleted += OnSearchChanged;
        }

        protected override void CleanUp()
        {
            _searchCanvasGroup.Disable();
            
            _searchPanel.InputCleared -= OnSearchCleared;
            _searchPanel.InputCompleted -= OnSearchChanged;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        protected override void CloseWindow()
        {
            base.CloseWindow();

            _searchPanel.Clear();
        }

        private void OnSearchChanged(string inputText)
        {
            _searchCharactersListModel?.Dispose();
            
            var friendsLoader = new PaginatedFriendsCharactersLoader(_bridge, PAGE_SIZE, inputText, Configs.UniverseId);
            _searchCharactersListModel =
                new SearchCharacterButtonListModel(SelectedCharacterManager, friendsLoader, CanDeselectCharacters);
            _searchCharacterButtonList.Initialize(_searchCharactersListModel);
            _searchCanvasGroup.Enable();
        }
        
        private void OnSearchCleared()
        {
            _searchCanvasGroup.Disable();
            
            _searchCharactersListModel?.Dispose();
            _searchCharactersListModel = null;
            
            _characterSelectionListModel?.UpdateSelection();
        }
    }
}