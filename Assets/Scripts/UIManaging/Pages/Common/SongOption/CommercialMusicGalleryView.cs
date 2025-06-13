using UIManaging.Pages.Common.SongOption.MusicLicense;
using UIManaging.Pages.Common.SongOption.SongList;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Common.SongOption
{
    internal class CommercialMusicGalleryView: MusicTypeGalleryViewBase
    {
        [SerializeField] private GenresListView _genresListView;
        
        [Inject] private MusicDataProvider _musicDataProvider;
        [Inject] private MusicSelectionStateController _stateSelectionController;

        protected override MusicLicenseType MusicLicenseType => MusicLicenseType.CommercialSounds;

        protected override void OnInitialized()
        {
            _genresListView.Initialize(new GenreListModel(_musicDataProvider, _stateSelectionController, true));
        }

        protected override void BeforeCleanUp()
        {
            _genresListView.CleanUp();
        }
    }
}