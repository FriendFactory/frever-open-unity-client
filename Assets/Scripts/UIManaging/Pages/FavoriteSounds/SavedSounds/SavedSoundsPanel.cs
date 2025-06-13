using Modules.MusicLicenseManaging;
using Navigation.Core;
using UIManaging.Pages.Common.FavoriteSounds;
using Zenject;

namespace UIManaging.Pages.FavoriteSounds
{
    internal sealed class SavedSoundsPanel : FavoriteSoundsPanelBase<SavedSoundItem, SavedSoundsList>
    {
        [Inject] private PageManager _pageManager;
        [Inject] private MusicLicenseValidator _musicLicenseValidator;

        protected override bool PremiumEnabled => _musicLicenseValidator.IsPremiumSoundsEnabled;
        protected override bool CommercialOnly => false;

        protected override void OnItemInstantiated(SavedSoundItem item)
        {
            // where to unsubscribe?
            item.OpenSoundPageRequested += OpenSoundPage;
        }

        private void OpenSoundPage(UsedSoundItemModel usedSoundItemModel)
        {
            _pageManager.MoveNext(new VideosBasedOnSoundPageArgs(usedSoundItemModel));
        }
    }
}