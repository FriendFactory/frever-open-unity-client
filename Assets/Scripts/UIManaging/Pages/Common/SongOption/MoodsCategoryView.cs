using System.Threading;
using System.Threading.Tasks;
using UIManaging.Localization;
using UIManaging.Pages.Common.SongOption.SongList;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Common.SongOption
{
    internal class MoodsCategoryView: MusicViewBase<MusicViewModel>
    {
        [SerializeField] private GenresListView _genresListView;
        
        [Inject] private MusicDataProvider _musicDataProvider;
        [Inject] private MusicSelectionStateController _musicSelectionStateController;
        [Inject] private MusicGalleryLocalization _localization;
        
        protected override string Name => _localization.MoodsCategoryHeader;
        
        public override Task InitializeAsync(MusicViewModel model, CancellationToken token)
        {
            _genresListView.Initialize(new GenreListModel(_musicDataProvider, _musicSelectionStateController, false));
            
            return base.InitializeAsync(model, token);
        }
    }
}