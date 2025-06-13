using System.Linq;
using Bridge.Services._7Digital.Models.TrackModels;
using Extensions;
using UIManaging.Pages.Common.SongOption.MusicCategory;
using UIManaging.Pages.Common.SongOption.MusicLicense;
using UIManaging.Pages.Common.SongOption.SongList;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Common.SongOption
{
    internal sealed class AllMusicGalleryView: MusicTypeGalleryViewBase
    {
        [SerializeField] private PromotedSongsPanel _promotedSongsPanel;
        [SerializeField] private PlaylistView _playlistView;
        [SerializeField] private GenresListView _genresListView;
        
        [Inject] private MusicDataProvider _musicDataProvider;
        [Inject] private MusicSelectionStateController _selectionStateController;
        [Inject] private MusicLicenseManager _musicLicenseManager;
        
        protected override MusicLicenseType MusicLicenseType => MusicLicenseType.AllSounds;

        protected override void OnInitialized()
        {
            _playlistView.SetActive(false);
            _genresListView.SetActive(false);

            if (_musicLicenseManager.PremiumSoundsEnabled)
            {
                _playlistView.SetActive(true);
                SetupPlaylist();
            }
            else
            {
                _genresListView.SetActive(true);
                SetupGenresPlaylist();
            }

            if (!_promotedSongsPanel.IsInitialized)
            {
                _promotedSongsPanel.Initialize();
            }
        }

        protected override void BeforeCleanUp()
        {
            _promotedSongsPanel.CleanUp();

            if (_playlistView.IsInitialized)
            {
                _playlistView.CleanUp();
            }

            if (_genresListView.IsInitialized)
            {
                _genresListView.CleanUp();
            }
        }

        private void SetupGenresPlaylist()
        {
            _genresListView.Initialize(new GenreListModel(_musicDataProvider, _selectionStateController, false));
        }

        private void SetupPlaylist()
        {
            var playlistModels = _musicDataProvider.GetPlaylistsModels();
            
            if (playlistModels == null) return;
            
            var playListViewModel = new PlaylistViewModel(playlistModels?.ToArray(), ShowFullPlaylist);
            _playlistView.Initialize(playListViewModel);
        }
        
        private void ShowFullPlaylist(string playlistName, ExternalTrackInfo[] tracks)
        {
            var playlistModel = new FullPlaylistListModel()
            {
                Name = playlistName,
                Playables = tracks
            };
            
            _selectionStateController.FireAsync(MusicNavigationCommand.OpenPlaylist, playlistModel);
        }
    }
}