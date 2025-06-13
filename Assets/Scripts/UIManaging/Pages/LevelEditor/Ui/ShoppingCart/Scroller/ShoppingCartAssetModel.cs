using Bridge.Models.Common;
using Bridge.Models.Common.Files;
using Extensions;
using static Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.LevelEditor.Ui.ShoppingCart
{
    internal sealed class ShoppingCartAssetModel : ShoppingCartItemModel
    {
        public long Id { get; }
        public DbModelType ModelType { get; }
        public string Name { get; }
        public int? SoftPrice { get; }
        public int? HardPrice { get; }
        public bool IsConfirmed { get; }

        public IThumbnailOwner ThumbnailOwner { get; }
        public Resolution Resolution { get; }

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public ShoppingCartAssetModel(IPurchasable item, bool isConfirmed = true)
        {
            var offer = item.AssetOffer;
            Id = offer.AssetId;
            ModelType = ((IEntity) item).GetModelType();
            Name = ((INamed) item).Name;
            SoftPrice = offer?.AssetOfferSoftCurrencyPrice;
            HardPrice = offer?.AssetOfferHardCurrencyPrice;
            IsConfirmed = isConfirmed;
            ThumbnailOwner = (IThumbnailOwner) item;
            Resolution = _128x128;
        }
    }
}