using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Common.Abstract;
using UIManaging.Pages.Common.FavoriteSounds;
using UIManaging.Pages.Common.SongOption.Carousel;
using UIManaging.Pages.Common.SongOption.MusicLicense;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Common.SongOption
{
    internal class PromotedSongsPanel : BaseContextlessPanel
    {
        [SerializeField] private PromotedSongsCarousel _carousel;

        [Inject] private MusicDataProvider _musicDataProvider;
        [Inject] private IBridge _bridge;
        [Inject] private MusicLicenseManager _musicLicenseManager;
        [Inject] private SoundsFavoriteStatusCache _favoriteStatusCache;

        private List<PromotedSongCarouselItemModel> _itemModels;
        private CancellationTokenSource _tokenSource;

        protected override void OnInitialized()
        {
            _tokenSource = new CancellationTokenSource();
            
            SetupCarousel();
        }

        protected override void BeforeCleanUp()
        {
            _tokenSource?.Cancel();
        }

        private async Task SetupPromotedSongsAsync()
        {
            foreach (var promotedSong in _musicDataProvider.PromotedSongs)
            {
                if (promotedSong.SongId != null)
                {
                    var song = _musicDataProvider.Songs.FirstOrDefault(x => x.Id == promotedSong.SongId);
                    if (song == null) continue;
                    _itemModels.Add(new PromotedSongCarouselItemModel(song, promotedSong));
                }
                else if (_musicLicenseManager.PremiumSoundsEnabled && promotedSong.ExternalSongId.HasValue)
                {
                    var result = await _bridge.GetTrackDetails(promotedSong.ExternalSongId.Value, _tokenSource.Token);
                    if (result.IsError)
                    {
                        Debug.LogWarning($"[{GetType().Name}] Failed to get track details # {result.ErrorMessage}");
                        continue;
                    }

                    if (result.IsRequestCanceled) continue;

                    _itemModels.Add(new PromotedSongCarouselItemModel(result.Model, promotedSong));
                }
            }

            _itemModels.ForEach(model => _favoriteStatusCache.AddToCacheIfNeeded(model.Playable));
        }

        private async void SetupCarousel()
        {
            _itemModels = new List<PromotedSongCarouselItemModel>();
            
            await SetupPromotedSongsAsync();
            
            var model = new PromotedSongsCarouselListModel(_itemModels);

            _carousel.Initialize(model);
        }
    }
}