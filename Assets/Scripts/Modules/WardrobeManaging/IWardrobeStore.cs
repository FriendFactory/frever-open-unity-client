using System.Threading.Tasks;
using Bridge;
using Bridge.ClientServer.InAppPurchases;
using Bridge.Models.ClientServer.Assets;
using Bridge.Results;
using JetBrains.Annotations;
using UIManaging.Pages.Common.UsersManagement;

namespace Modules.WardrobeManaging
{
    public interface IWardrobeStore
    {
        bool IsProcessingPurchase { get; }
        bool UserHasFreeAccessToAllAssets { get; }

        Task<Result<AssetPurchaseResult>> Purchase(long wardrobeId);
        bool IsOwned(WardrobeFullInfo wardrobe);
    }

    [UsedImplicitly]
    internal sealed class WardrobeStore : IWardrobeStore
    {
        private readonly IBridge _bridge;
        private readonly LocalUserDataHolder _localUserDataHolder;
        private readonly ClothesCabinet _clothesCabinet;

        public WardrobeStore(IBridge bridge, LocalUserDataHolder localUserDataHolder, ClothesCabinet clothesCabinet)
        {
            _bridge = bridge;
            _localUserDataHolder = localUserDataHolder;
            _clothesCabinet = clothesCabinet;
        }

        public bool IsProcessingPurchase { get; private set; }

        public bool UserHasFreeAccessToAllAssets => _localUserDataHolder.IsStarCreator || _localUserDataHolder.IsStarCreatorCandidate;

        public async Task<Result<AssetPurchaseResult>> Purchase(long wardrobeId)
        {
            IsProcessingPurchase = true;
            var res = await _bridge.PurchaseAsset(wardrobeId);
            IsProcessingPurchase = false;
            return res;
        }

        public bool IsOwned(WardrobeFullInfo wardrobe)
        {
            var currentLevel = _localUserDataHolder.LevelingProgress?.Xp?.CurrentLevel;
            var currentLevelValue = currentLevel?.Level ?? 0;

            if (currentLevelValue < wardrobe.SeasonLevel) return false;

            if (wardrobe.AssetOffer == null) return true;

            var offer = wardrobe.AssetOffer;
            var hasSoftPrice = offer.AssetOfferSoftCurrencyPrice.HasValue;
            var hasHardPrice = offer.AssetOfferHardCurrencyPrice.HasValue;

            if (!hasSoftPrice && !hasHardPrice) return true;

            if (_clothesCabinet.IsWardrobePurchased(wardrobe.Id)) return true;

            return false;
        }
    }
}