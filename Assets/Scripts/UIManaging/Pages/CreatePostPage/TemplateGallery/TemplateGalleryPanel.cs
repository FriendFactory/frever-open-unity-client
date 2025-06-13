using System;
using System.Collections.Generic;
using Bridge.Models.ClientServer.StartPack.Metadata;
using UIManaging.Common.SearchPanel;
using UIManaging.Common.Templates;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.CreatePostPage.TemplateGallery
{
    public sealed class TemplateGalleryPanel : MonoBehaviour
    {
        private const string SEARCH_BAR_PLACEHOLDER_TEXT = "Search";
        
        public event Action OnGridScrolled;
        public event Action OnGridLoaded;
        
        [SerializeField] private TemplatesGrid _templatesGrid;
        [SerializeField] private SearchPanelView _searchBar;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private int? CharactersCount
        {
            get => _templatesGrid.CharactersCount;
            set => _templatesGrid.CharactersCount = value;
        }

        public bool FirstPageLoaded => _templatesGrid.FirstPageLoaded;


        private void Awake()
        {
            _searchBar.PlaceholderText = SEARCH_BAR_PLACEHOLDER_TEXT;
            _searchBar.InputCompleted += OnSearchChanged;
            _searchBar.InputCleared += OnSearchCleared;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Init(TemplateGalleryState galleryState = null)
        {
            if (galleryState != null)
            {
                _searchBar.SetTextWithoutNotify(galleryState.SearchQuery);
                CharactersCount = galleryState.CharacterCount;

                if (!string.IsNullOrEmpty(galleryState.SearchQuery))
                {
                    OnSearchChanged(galleryState.SearchQuery);
                    return;
                }
                
                _templatesGrid.DownloadTemplates(galleryState.CharacterCount);
            }
            else
            {
                _templatesGrid.DownloadTemplates((int?)null);
            }
            
            _templatesGrid.OnFirstPageLoaded += OnGridLoaded;
            
            RefreshTabs();

            _templatesGrid.OnScrolled += OnScrolled;
        }

        public void RefreshTabs()
        {
            //_categorySelector.Init(OnCategoryChanged, OnSubCategoryChanged);
        }
        
        public TemplateGalleryState GetGalleryState()
        {
            return new TemplateGalleryState
            {
                SearchQuery = _searchBar.Text,
                CharacterCount = CharactersCount
            };
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnSearchChanged(string filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                _searchBar.ClearWithoutNotify();
            }

            _templatesGrid.DownloadTemplates(filter);
        }

        private void OnSearchCleared()
        {
            _templatesGrid.DownloadTemplates(string.Empty);
        }

        private void OnScrolled()
        {
            OnGridScrolled?.Invoke();
        }
    }
}