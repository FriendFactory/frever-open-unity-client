using Abstract;
using TMPro;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Common.SongOption.Playlists
{
    public class PlaylistItem: BaseContextDataButton<PlaylistItemModel>
    {
        [SerializeField] private TMP_Text _title;

        [Inject] private MusicSelectionStateController _stateController;
        
        protected override void OnInitialized()
        {
            _title.text = ContextData.PlaylistInfo?.Title ?? "";
        }

        protected override void OnUIInteracted()
        {
            base.OnUIInteracted();

            var fullPlaylistModel = new FullPlaylistListModel()
            {
                Name = ContextData.PlaylistInfo.Title,
                Playables = ContextData.PlaylistInfo.Tracks
            };

            _stateController.FireAsync(MusicNavigationCommand.OpenPlaylist, fullPlaylistModel);
        }
    }
}