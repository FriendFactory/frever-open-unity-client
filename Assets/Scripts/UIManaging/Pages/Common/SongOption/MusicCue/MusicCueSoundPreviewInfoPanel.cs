using Common.Abstract;
using Extensions;
using TMPro;
using UIManaging.Pages.Common.FavoriteSounds;
using UIManaging.Pages.Common.SongOption.Common;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Common.SongOption.MusicCue
{
    public sealed class MusicCueSoundPreviewInfoPanel: BaseContextPanel<SoundPreviewInfoPanelModel>
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private FavoriteSoundToggleHandler _favoriteSoundToggleHandler;
        
        [Inject] private SoundsFavoriteStatusCache _favoriteStatusCache;

        protected override bool IsReinitializable => true;

        protected override void OnInitialized()
        {
            var sound = ContextData.Sound is PlayableTrendingUserSound trendingUserSound
                ? trendingUserSound.UserSound
                : ContextData.Sound;
            _title.text = sound.GetSoundName();
            
            _favoriteStatusCache.AddToCacheIfNeeded(ContextData.Sound);

            var isFavorite = _favoriteStatusCache.IsFavorite(ContextData.Sound);
            
            _favoriteSoundToggleHandler.Initialize(new SoundItemModel(sound, isFavorite));
        }

        protected override void BeforeCleanUp()
        {
            _favoriteSoundToggleHandler.CleanUp();
        }
    }
}