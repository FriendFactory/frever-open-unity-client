using UIManaging.Pages.Common.FavoriteSounds;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Common.SongOption.Favorites
{
    internal sealed class FavoriteSoundItem: SoundItemBase<UsedSoundItemModel>
    {
        [SerializeField] private FavoriteSoundToggleHandler _favoriteSoundToggleHandler;

        [Inject] private SoundsFavoriteStatusCache _favoriteStatusCache;
        
        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            ContextData.IsFavorite = _favoriteStatusCache.IsFavorite(ContextData.Sound);
            _favoriteSoundToggleHandler.Initialize(ContextData);
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            
            _favoriteSoundToggleHandler.CleanUp();
        }
    }
}