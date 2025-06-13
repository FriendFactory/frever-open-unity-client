using System.Collections.Generic;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using Common.Abstract;
using Extensions;
using StansAssets.Foundation.Patterns;
using UIManaging.Common.Toggles;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Common.FavoriteSounds
{
    [RequireComponent(typeof(FavoriteSoundToggle))]
    public sealed class FavoriteSoundToggleHandler: BaseContextPanel<SoundItemModel> 
    {
        [SerializeField] private FavoriteSoundToggle _favoriteSoundToggle;
        [SerializeField] private List<ToggleSwapBase> _favoriteStateSwappers;

        public bool IsFavorite => _favoriteSoundToggle.IsFavorite;
        
        [Inject] private SoundsFavoriteStatusCache _favoriteStatusCache;

        protected override void OnInitialized()
        {
            _favoriteSoundToggle.Initialize(ContextData);

            _favoriteSoundToggle.AddedToFavorites += OnAddedToFavorites;
            _favoriteSoundToggle.RemovedFormFavorites += OnRemovedFromFavorites;

            _favoriteStatusCache.SoundFavoriteStatusChanged += OnSoundFavoriteStatusChanged;
        }

        protected override void BeforeCleanUp()
        {
            _favoriteSoundToggle.CleanUp();
            
            _favoriteSoundToggle.AddedToFavorites -= OnAddedToFavorites;
            _favoriteSoundToggle.RemovedFormFavorites -= OnRemovedFromFavorites;
            
            _favoriteStatusCache.SoundFavoriteStatusChanged -= OnSoundFavoriteStatusChanged;
        }
        
        private void OnAddedToFavorites(SoundType soundType, long id)
        {
            _favoriteStatusCache.AddOrUpdate(soundType, id, true);
        }

        private void OnRemovedFromFavorites(SoundType soundType, long id)
        {
            _favoriteStatusCache.AddOrUpdate(soundType, id, false);
        }

        private void OnSoundFavoriteStatusChanged(SoundFavoriteStatusChangedEventArgs args)
        {
            var soundType = ContextData.Sound.GetFavoriteSoundType();
            var id = ContextData.Sound.Id;
            
            if (soundType != args.Type || id != args.Id) return;
            
            ToggleFavorite(args.IsFavorite);
            ContextData.IsFavorite = args.IsFavorite;
        }
        
        private void ToggleFavorite(bool isOn)
        {
            _favoriteSoundToggle.SetWithoutNotify(isOn);
            _favoriteStateSwappers.ForEach(swap => swap.Toggle(isOn));
        }
    }
}