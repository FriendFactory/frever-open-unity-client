using Bridge;
using Extensions;
using StansAssets.Foundation.Patterns;
using UIManaging.Common.Carousel;
using UIManaging.Pages.Common.FavoriteSounds;
using UIManaging.Pages.Common.SongOption.Common;
using UnityEngine;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.Common.SongOption.Carousel
{
    public class PromotedSongCarouselItem : ClickableCarouselItem<PromotedSongCarouselItemModel>
    {
        [Inject] private IBridge _bridge;
        [Inject] private SoundsFavoriteStatusCache _favoriteStatusCache;
        
        protected override async void OnInitialized()
        {
            base.OnInitialized();
            
            if (!gameObject.activeSelf || !gameObject.activeInHierarchy) return;

            var result = await _bridge.GetThumbnailAsync(ContextData.PromotedSong, Resolution._512x512);
            if (result.IsSuccess)
            {
                OnThumbnailLoaded((Texture2D)result.Object);   
            }
            else
            {
                OnThumbnailLoadingFailed(result.ErrorMessage);
            }
        }

        protected override void OnClicked()
        {
            base.OnClicked();

            var isFavorite = _favoriteStatusCache.IsFavorite(ContextData.Playable);
            
            StaticBus<SongSelectedEvent>.Post(new SongSelectedEvent(ContextData.Playable));
        }

        private void OnThumbnailLoaded(Texture2D obj)
        {
            _thumbnail.sprite = obj.ToSprite();
        }

        private void OnThumbnailLoadingFailed(string error)
        {
            Debug.LogWarning(error);
        }
    }
}