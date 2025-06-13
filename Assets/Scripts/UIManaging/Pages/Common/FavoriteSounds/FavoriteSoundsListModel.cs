using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge.ClientServer.Assets.Music;
using Bridge.Models.ClientServer.Assets;
using UIManaging.Common.Loaders;
using UIManaging.Pages.Common.FavoriteSounds;
using UnityEngine;

namespace UIManaging.Pages.FavoriteSounds
{
    public sealed class FavoriteSoundsListModel: GenericPaginationLoader<FavouriteMusicInfo>
    {
        private readonly IFavouriteMusicService _favouriteMusicService;
        private readonly bool _premiumEnabled;
        private readonly bool _commercialOnly;
        private readonly List<UsedSoundItemModel> _itemModels;

        private FavouriteMusicInfo _localFavoriteSound;
        
        public IList<UsedSoundItemModel> ItemModels => _itemModels;
        
        protected override int DefaultPageSize => 10;

        public FavoriteSoundsListModel(IFavouriteMusicService favouriteMusicService, bool premiumEnabled, bool commercialOnly)
        {
            _favouriteMusicService = favouriteMusicService;
            _premiumEnabled = premiumEnabled;
            _commercialOnly = commercialOnly;
            _itemModels = new List<UsedSoundItemModel>();
        }

        public bool TryInsertFirst(FavouriteMusicInfo favoriteSound)
        {
            if (favoriteSound == null) return false;
            
            if (IsAlreadyFavorite()) return false;

            _localFavoriteSound = favoriteSound;
            var itemModel = new UsedSoundItemModel(_localFavoriteSound, true, _localFavoriteSound.UsageCount);
            
            Models.Insert(0, _localFavoriteSound);
            ItemModels.Insert(0, itemModel);
            
            return true;

            bool IsAlreadyFavorite() => Models.Any(model => model.Id == favoriteSound.Id);
        }

        protected override void OnNextPageLoaded(FavouriteMusicInfo[] page) => AddItems(page);
        protected override void OnFirstPageLoaded(FavouriteMusicInfo[] page) => AddItems(page);

        protected override async Task<FavouriteMusicInfo[]> DownloadModelsInternal(object targetId, int takeNext, int takePrevious = 0, CancellationToken token = default)
        {
            var skip = Mathf.Max(0, Models.Count - 1);
            var result = await _favouriteMusicService.GetFavouriteSoundList(takeNext, skip, _commercialOnly, token);
            
            if (result.IsError)
            {
                Debug.LogError($"[{GetType().Name}] Failed to get favorite sounds # {result.ErrorMessage}");
                return null;
            }

            if (result.IsRequestCanceled) return null;

            return result.IsRequestCanceled ? null : result.Models;
        }

        private void AddItems(IEnumerable<FavouriteMusicInfo> page)
        {
            var itemModels = page
                            .Where(favoriteSound => (_premiumEnabled || favoriteSound.Type != SoundType.ExternalSong) && favoriteSound.Id != _localFavoriteSound?.Id)
                            .Select(favoriteSound => new UsedSoundItemModel(favoriteSound, true, favoriteSound.UsageCount));
            
            _itemModels.AddRange(itemModels);
        }
    }
}