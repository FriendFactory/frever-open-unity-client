using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.Assets;
using Bridge.Results;
using Extensions;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.PaginationLoaders
{
    public class BodyAnimationPaginationLoader: AssetSelectorPaginationLoader<BodyAnimationInfo>
    {
        public int CharacterCount { get; set; }
        public long? MainMovementTypeId { get; set; }
        public long[] AllMovementTypeIds { get; set; }
        public long RaceId { get; set; }

        private BodyAnimationPaginationLoader(PaginationLoaderType type, Func<AssetSelectorModel> selectorModel, long universeId,
            NullableLong categoryId = null, string filter = null, NullableLong taskId = null) :
            base(type, selectorModel, universeId,categoryId, filter, taskId)
        { }

        private string GetCategoryName(BodyAnimationInfo bodyAnimation)
        {
            return DataFetcher.MetadataStartPack.BodyAnimationCategories.First(x => x.Id == bodyAnimation.BodyAnimationCategoryId).Name;
        }
        
        protected override IList<AssetSelectionItemModel> CreateItemModels(IList<BodyAnimationInfo> page)
        {
            var assetData = DataFetcher.DefaultUserAssets.PurchasedAssetsData;
            var purchasedIds = assetData.GetPurchasedAssetsByType(DbModelType.BodyAnimation);
            
            return page.Where(model => model != null).Select(bodyAnimationInfo =>
                                                                 new AssetSelectionBodyAnimationModel(
                                                                     Resolution._128x128, bodyAnimationInfo,
                                                                     GetCategoryName(bodyAnimationInfo))
                                                                 {
                                                                     IsPurchased = purchasedIds.Contains(bodyAnimationInfo.Id)
                                                                 }).ToList<AssetSelectionItemModel>();
        }

        protected override Task<ArrayResult<BodyAnimationInfo>> GetAssetListAsync(long? targetId, int takeNext, int takePrevious, long? categoryId, string filter,
                                                                                  CancellationToken token)
        {
            switch (Type)
            {
                case PaginationLoaderType.Category:
                case PaginationLoaderType.Search:
                    return Bridge.GetBodyAnimationListAsync(targetId, takeNext, takePrevious, raceId: RaceId, filter, categoryId, TaskId, CharacterCount, movementTypeIds:AllMovementTypeIds, token:token);//todo: provide race id from active race
                case PaginationLoaderType.MyAssets:
                    return Bridge.GetMyBodyAnimationListAsync(targetId, takeNext, takePrevious, token);
                case PaginationLoaderType.Recommended:
                    
                    if (!MainMovementTypeId.HasValue)
                    {
                        Debug.LogError("No movement id specified");
                        return null;
                    }
                    
                    return Bridge.GetRecommendedBodyAnimationListAsync(targetId, takeNext, takePrevious, MainMovementTypeId.Value, 
                                                                       CharacterCount, RaceId, filter, TaskId, token);
                default:
                    Debug.LogError($"Unknown pagination loader type: {Type}");
                    return null;
            }
            
        }

        [UsedImplicitly]
        public class Factory: PaginationLoaderFactoryBase<BodyAnimationPaginationLoader, BodyAnimationInfo> { }
    }
}