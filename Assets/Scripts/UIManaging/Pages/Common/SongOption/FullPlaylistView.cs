using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.Assets;
using Bridge.Services._7Digital.Models.TrackModels;
using Extensions;
using UIManaging.Pages.Common.FavoriteSounds;
using UIManaging.Pages.Common.SongOption.SongList;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Common.SongOption
{
    internal class FullPlaylistView : MusicViewBase<FullPlaylistListModel>
    {
        [SerializeField] private SongListRow _songListRow;
        [SerializeField] private TrackListRow _trackListRow;
        
        [Inject] private SoundsFavoriteStatusCache _soundsFavoriteStatusCache;
        
        protected override string Name => ContextData.Name;
        
        private FullPlaylistListModel ContextData { get; set; }

        public override Task InitializeAsync(FullPlaylistListModel model, CancellationToken token)
        {
            ContextData = model;

            _songListRow.SetActive(false);
            _trackListRow.SetActive(false);
            switch (ContextData.Playables)
            {
                case SongInfo[] playableSongInfo:
                    _songListRow.Initialize(playableSongInfo.Select(song => new PlayableSongModel(song)).ToArray());
                    _songListRow.SetActive(true);
                    break;
                case ExternalTrackInfo[] playableTrackInfo:
                    _trackListRow.Initialize(playableTrackInfo
                                       .Select(track => new PlayableTrackModel(track, _soundsFavoriteStatusCache.IsFavorite(track)))
                                       .ToArray());
                    _trackListRow.SetActive(true);
                    break;
            }
                         
            return base.InitializeAsync(model, token);
        }

        protected override void OnCleanUp()
        {
            if (_songListRow.IsInitialized)
            {
                _songListRow.CleanUp();
            }

            if (_trackListRow.IsInitialized)
            {
                _trackListRow.CleanUp();
            }
        }

        protected override void MoveBack()
        {
            _stateSelectionController.Fire(MusicNavigationCommand.ClosePlaylist);
        }
    }
}