using UIManaging.Localization;
using UIManaging.Pages.Common.TabsManager;
using Zenject;

namespace UIManaging.Pages.Common.SongOption.Search
{
    internal sealed class CommercialMusicSearchView: MusicSearchView
    {
        private const int COMMERCIAL_TAB_INDEX = 0;
        
        [Inject] private MusicGalleryLocalization _localization;
        
        protected override int InitialTabIndex => COMMERCIAL_TAB_INDEX;
        
        protected override TabsManagerArgs GetTabManagerArgs()
        {
            // maybe it worth to encapsulate index and name in MusicSearchTypeView 
            var tabModels = new[]
            {
                new TabModel(COMMERCIAL_TAB_INDEX, _localization.SearchCommercialTab),
            };

            return new TabsManagerArgs(tabModels);
        }

        protected override MusicSearchType GetSearchTypeFromIndex(int _) => MusicSearchType.CommercialSongs;
    }
}