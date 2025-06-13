using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.WardrobeManaging;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.LevelEditor.Ui.Wardrobe;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow
{
    /// <summary>
    /// Temporary class for shopping cart logic - will be refactored or replaced in the future
    /// </summary>
    [UsedImplicitly]
    internal sealed class ShoppingCartController
    {
        private readonly ILevelManager _levelManager;
        private readonly LocalUserDataHolder _userData;
        private readonly ClothesCabinet _clothesCabinet;
        private readonly IBridge _bridge;
        private readonly UmaLevelEditor _wardrobeUmaEditor;

        public ShoppingCartController(ILevelManager levelManager, LocalUserDataHolder userData, ClothesCabinet clothesCabinet, IBridge bridge, UmaLevelEditor wardrobeUmaEditor)
        {
            _levelManager = levelManager;
            _userData = userData;
            _clothesCabinet = clothesCabinet;
            _bridge = bridge;
            _wardrobeUmaEditor = wardrobeUmaEditor;
        }

        public async Task PurchaseAssetsAsync()
        {
            var items = GetNotOwnedWardrobes();
            
            if (items.Length == 0) return;
            
            await Task.WhenAll(items.Select(PurchaseAsset));
            await UpdateUserAssetsAndBalance();
            return;

            async Task PurchaseAsset(WardrobeFullInfo wardrobeFullInfo)
            {
                var result = await _bridge.PurchaseAsset(wardrobeFullInfo.AssetOffer.Id);

                if (result.IsSuccess && result.Model.Ok)
                {
                    Debug.Log($@"Purchase successful");

                    _wardrobeUmaEditor.UpdateAfterPurchase(wardrobeFullInfo);
                }
                else
                {
                    Debug.Log($@"Purchase failed {result.ErrorMessage}\n{result.Model?.ErrorCode}\n{result.Model?.ErrorMessage}");
                }
            }
        }

        private WardrobeFullInfo[] GetNotOwnedWardrobes()
        {
            var outfit = _levelManager.EditingCharacterController.Outfit;
            return outfit?.Wardrobes.Where(x => !IsWardrobeOwned(x)).ToArray();
        }
    
        private async Task UpdateUserAssetsAndBalance()
        {
            await _userData.UpdatePurchasedAssetsInfo();
            await _userData.UpdateBalance();
        }
        
        private bool IsWardrobeOwned(WardrobeFullInfo wardrobeFull)
        {
            var currentLevel = _userData.LevelingProgress?.Xp?.CurrentLevel;
            var currentLevelValue = currentLevel?.Level ?? 0;

            if (currentLevelValue < wardrobeFull.SeasonLevel) return false;

            if (wardrobeFull.AssetOffer == null) return true;

            var offer = wardrobeFull.AssetOffer;
            var hasSoftPrice = offer.AssetOfferSoftCurrencyPrice.HasValue;
            var hasHardPrice = offer.AssetOfferHardCurrencyPrice.HasValue;

            if (!hasSoftPrice && !hasHardPrice) return true;

            if (_clothesCabinet.IsWardrobePurchased(wardrobeFull.Id)) return true;

            return false;
        }
    }
}