using UIManaging.Localization;
using UIManaging.Pages.Common.SongOption.MusicLicense;
using UIManaging.Pages.Common.TabsManager;
using Zenject;

namespace UIManaging.Pages.Common.SongOption.Search
{
    internal sealed class AllMusicSearchView: MusicSearchView
    {
        [Inject] private MusicLicenseManager _musicLicenseManager;
        [Inject] private MusicGalleryLocalization _localization;

        private TabModel[] _premiumTabModels;
        private TabModel[] _tabModels;

        protected override void OnInitialized()
        {
            InitializeTabs();
            
            base.OnInitialized();
        }

        protected override int InitialTabIndex => 0;
        
        protected override TabsManagerArgs GetTabManagerArgs() =>
            new TabsManagerArgs(_musicLicenseManager.PremiumSoundsEnabled ? _premiumTabModels : _tabModels);

        protected override MusicSearchType GetSearchTypeFromIndex(int tabIndex) =>
            (MusicSearchType)(_musicLicenseManager.PremiumSoundsEnabled ? tabIndex : tabIndex + 1);

        private void InitializeTabs()
        {
            _premiumTabModels = new TabModel[]
            {
                new TabModel(0, _localization.SearchTrendingTab),
                new TabModel(1, _localization.SearchMoodTab), 
                new TabModel(2, _localization.SearchUserSoundsTab),
            };

            _tabModels = new TabModel[]
            {
                new TabModel(0, _localization.SearchMoodTab), 
                new TabModel(1, _localization.SearchUserSoundsTab),
            };
        }
    }
}