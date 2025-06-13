using System;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Common;
using Common.Abstract;
using Extensions;
using I2.Loc;
using StansAssets.Foundation.Patterns;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Common.FavoriteSounds
{
    public class FavoriteSoundToggle: BaseContextPanel<SoundItemModel>
    {
        private static readonly string SOUND_ADDED_TO_FAVORITES_POPUP_DISPLAYED_ONCE_ID = $"SoundAddedToFavoritesPopupDisplayedOnce";
        
        [SerializeField] private Toggle _toggle;
        [Header("L10N")]
        [SerializeField] private LocalizedString _favoriteAddedMessageLoc; 
        [SerializeField] private LocalizedString _favoriteRemovedMessageLoc; 

        [Inject] private IBridge _bridge;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private PopupManager _popupManager;

        private PlayerPrefsFlag _displayedOnceFlag;

        public event Action<SoundType, long> AddedToFavorites;
        public event Action<SoundType, long> RemovedFormFavorites;

        public bool IsFavorite => _toggle.isOn;

        public void SetWithoutNotify(bool isOn)
        {
            _toggle.SetIsOnWithoutNotify(isOn);
        }

        protected override void OnInitialized()
        {
            _toggle.isOn = ContextData.IsFavorite;

            _displayedOnceFlag = new PlayerPrefsFlag(SOUND_ADDED_TO_FAVORITES_POPUP_DISPLAYED_ONCE_ID);
            
            _toggle.onValueChanged.AddListener(ToggleFavoriteSound);
        }
        
        protected override void BeforeCleanUp()
        {
            _toggle.onValueChanged.RemoveListener(ToggleFavoriteSound);
        }

        private async void ToggleFavoriteSound(bool isOn)
        {
            try
            {
                _toggle.interactable = false;

                var soundType = ContextData.Sound.GetFavoriteSoundType();
                var id = ContextData.Sound.GetSoundId();

                if (isOn)
                {
                    var result = await _bridge.AddSoundToFavouriteList(soundType, id);
                    if (result.IsError)
                    {
                        Debug.LogError($"[{GetType().Name}] Failed to add sound to favorites # {result.ErrorMessage}");
                        return;
                    }

                    StaticBus<SoundAddedToFavoritesEvent>.Post(new SoundAddedToFavoritesEvent(result.Model));
                    
                    AddedToFavorites?.Invoke(soundType, id);
                }
                else
                {
                    await _bridge.RemoveSoundFromFavouriteList(soundType, id);
                    
                    RemovedFormFavorites?.Invoke(soundType, id);
                }

                ContextData.IsFavorite = isOn;
                
                if (isOn && !_displayedOnceFlag.IsSet())
                {
                    _displayedOnceFlag.Set(1);
                    
                    _popupManager.SetupPopup(new DialogPopupConfiguration() {PopupType = PopupType.SoundAddedToFavorites});
                    _popupManager.ShowPopup(PopupType.SoundAddedToFavorites);
                    
                    return;
                }

                var message = isOn ? _favoriteAddedMessageLoc : _favoriteRemovedMessageLoc;
                _snackBarHelper.ShowSuccessDarkSnackBar(message);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                _toggle.interactable = true;
            }
        }
    }

    public class SoundAddedToFavoritesEvent : IEvent
    {
        public FavouriteMusicInfo FavoriteSound { get; }

        public SoundAddedToFavoritesEvent(FavouriteMusicInfo favoriteSound)
        {
            FavoriteSound = favoriteSound;
        }
    }
}