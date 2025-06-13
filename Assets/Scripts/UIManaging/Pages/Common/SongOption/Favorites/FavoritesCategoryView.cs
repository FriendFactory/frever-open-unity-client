using I2.Loc;
using UnityEngine;

namespace UIManaging.Pages.Common.SongOption.Favorites
{
    internal sealed class FavoritesCategoryView: MusicViewBase<MusicViewModel>
    {
        [SerializeField] private FavoriteSoundsPanel _favoriteSoundsPanel;
        [Header("L10N")]
        [SerializeField] private LocalizedString _headerLoc;
        
        protected override string Name => _headerLoc;

        protected override void OnInitialized(MusicViewModel _)
        {
            _favoriteSoundsPanel.Initialize();
        }

        protected override void OnCleanUp()
        {
            _favoriteSoundsPanel.CleanUp();
        }
    }
}