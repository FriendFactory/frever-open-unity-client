using StansAssets.Foundation.Patterns;
using UIManaging.Pages.Common.FavoriteSounds;
using UIManaging.Pages.Common.SongOption.MusicLicense;
using Zenject;

namespace UIManaging.Pages.Common.SongOption.Favorites
{
    internal sealed class FavoriteSoundsPanel: FavoriteSoundsPanelBase<FavoriteSoundItem, FavoriteSoundsList>
    {
        [Inject] private MusicLicenseManager _musicLicenseManager;

        protected override bool PremiumEnabled => _musicLicenseManager.PremiumSoundsEnabled;
        protected override bool CommercialOnly => _musicLicenseManager.ActiveLicenseType == MusicLicenseType.CommercialSounds;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            StaticBus<SoundAddedToFavoritesEvent>.Subscribe(OnSoundAddedToFavorites);
        }

        protected override void BeforeCleanUp()
        {
            base.BeforeCleanUp();
            
            StaticBus<SoundAddedToFavoritesEvent>.Unsubscribe(OnSoundAddedToFavorites);
        }

        private void OnSoundAddedToFavorites(SoundAddedToFavoritesEvent @event)
        {
            if (!FavoriteSoundsListModel.TryInsertFirst(@event.FavoriteSound)) return;
            
            if (FavoriteSoundsListModel.ItemModels.Count == 1)
            {
                OnFirstPageLoaded();
            }
                
            FavoriteSoundsList.Reload();
        }
    }
}