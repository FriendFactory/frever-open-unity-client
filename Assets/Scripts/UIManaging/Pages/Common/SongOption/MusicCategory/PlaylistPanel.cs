using System.Collections;
using System.Linq;
using Abstract;
using Extensions;
using TMPro;
using UIManaging.EnhancedScrollerComponents.CellSpawners;
using UIManaging.Pages.Common.SongOption.SongList;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Common.SongOption.MusicCategory
{
    internal sealed class PlaylistPanel : BaseContextDataView<PlaylistPanelModel>
    {
        [SerializeField] private Button _buttonSeeAll;
        [SerializeField] private TextMeshProUGUI _playlistName;
        [SerializeField] private int _pageTrackCount = 12;
        [SerializeField] private PlaylistPanelGridSpawner _gridSpawner;

        //---------------------------------------------------------------------
        // Fields
        //---------------------------------------------------------------------

        private readonly EnhancedScrollerGridSpawnerModel<PlayableTrackModel> _gridSpawnerModel =
            new EnhancedScrollerGridSpawnerModel<PlayableTrackModel>();

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            StartCoroutine(InitializeGrid());
            _buttonSeeAll.onClick.AddListener(OnSeeAll);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            var tracksToShow = ContextData.Tracks.Take(_pageTrackCount).ToArray();
            _gridSpawner.SetRowsScrollable(false);
            _gridSpawnerModel.SetItems(tracksToShow);
            _playlistName.text = ContextData.PlaylistName;
        }

        //---------------------------------------------------------------------
        // Private
        //---------------------------------------------------------------------

        private void OnSeeAll()
        {
            ContextData.ShowFullList();
        }

        private IEnumerator InitializeGrid()
        {
            var rectTransform = (RectTransform)transform;
            var originalWidth = rectTransform.GetWidth();
            
            yield return new WaitForEndOfFrame();
            
            var width = rectTransform.GetWidth();
            var newRowSize = GetRowSize(width, originalWidth);
            _gridSpawner.SetRowSize(newRowSize);
            _gridSpawner.Initialize(_gridSpawnerModel);
        }

        private float GetRowSize(float currentWidth, float originalWidth)
        {
            return _gridSpawner.GetRowSize() + currentWidth - originalWidth;
        }
    }
}
