using System;
using System.Linq;
using System.Threading.Tasks;
using Abstract;
using Bridge;
using Extensions;
using TMPro;
using UIManaging.EnhancedScrollerComponents.CellSpawners;
using UIManaging.Pages.Common.FavoriteSounds;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Common.SongOption.SongList
{
    internal sealed class GenrePanel : BaseContextDataView<GenrePanelModel>
    {
        [SerializeField] private Button _buttonSeeAll;
        [SerializeField] private TextMeshProUGUI _genreName;
        [SerializeField] private int _previewGenreSongsCount = 12;
        [SerializeField] private GenrePanelGridSpawner _gridSpawner;
        
        [Inject] private IBridge _bridge;
        [Inject] private SoundsFavoriteStatusCache _favoriteStatusCache;
        
        //---------------------------------------------------------------------
        // Fields
        //---------------------------------------------------------------------

        private readonly EnhancedScrollerGridSpawnerModel<PlayableSongModel> _gridSpawnerModel =
            new EnhancedScrollerGridSpawnerModel<PlayableSongModel>();

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _buttonSeeAll.onClick.AddListener(OnSeeAll);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override async void OnInitialized()
        {
            _genreName.text = ContextData.Genre.Name;
            var songsToShow = await GetSongs();
            _gridSpawner.SetRowsScrollable(false);
            _gridSpawnerModel.SetItems(songsToShow);
            InitializeGrid();
        }

        //---------------------------------------------------------------------
        // Private
        //---------------------------------------------------------------------

        private void OnSeeAll()
        {
            ContextData.ShowFullList();
        }

        private void InitializeGrid()
        {
            var rectTransform = (RectTransform)transform;
            var originalWidth = rectTransform.GetWidth();

            var width = rectTransform.GetWidth();
            var newRowSize = GetRowSize(width, originalWidth);
            _gridSpawner.SetRowSize(newRowSize);
            _gridSpawner.Initialize(_gridSpawnerModel);
        }

        private float GetRowSize(float currentWidth, float originalWidth)
        {
            return _gridSpawner.GetRowSize() + currentWidth - originalWidth;
        }

        private async Task<PlayableSongModel[]> GetSongs()
        {
            var result = await _bridge.GetSongsAsync(_previewGenreSongsCount, 0, genreId: ContextData.Genre.Id, commercialOnly: ContextData.CommercialOnly);
            if (result.IsError)
            {
                Debug.LogError($"### Failed to load songs. Reason: {result.ErrorMessage}");
                return Array.Empty<PlayableSongModel>();
            }

            var models = result.Models;
            
            models.ForEach(model => _favoriteStatusCache.AddToCacheIfNeeded(model));
            
            return result.Models.Select(x=> new PlayableSongModel(x)).ToArray();
        }
    }
}

