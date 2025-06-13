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
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.PaginationLoaders
{
    public class SetLocationPaginationLoader: AssetSelectorPaginationLoader<SetLocationFullInfo>
    {
        public long RaceId { get; set; } = 1; //todo: provide race id from active race
        private SetLocationPaginationLoader(PaginationLoaderType type, Func<AssetSelectorModel> selectorModel, long universeId,
            NullableLong categoryId = null, string filter = null, NullableLong taskId = null): 
            base(type, selectorModel, universeId,categoryId, filter, taskId) { }
           
        private string GetCategoryName(SetLocationFullInfo setLocation)
        {
            return DataFetcher.MetadataStartPack.SetLocationCategories.First(x => x.Id == setLocation.CategoryId).Name;
        }
            
        protected override IList<AssetSelectionItemModel> CreateItemModels(IList<SetLocationFullInfo> page)
        {
            var assetData = DataFetcher.DefaultUserAssets.PurchasedAssetsData;
            var purchasedIds = assetData.GetPurchasedAssetsByType(DbModelType.SetLocation);

            return page
                  .Where(model => model != null)
                  .Select(setLocationFullInfo => new AssetSelectionSetLocationModel(
                              Resolution._128x128,
                              setLocationFullInfo,
                              GetCategoryName(setLocationFullInfo))
                          {
                              IsPurchased = purchasedIds.Contains(setLocationFullInfo.Id)
                          })
                  .ToList<AssetSelectionItemModel>();
        }

        protected override Task<ArrayResult<SetLocationFullInfo>> GetAssetListAsync(long? targetId, int takeNext, int takePrevious, long? categoryId, string filter,
            CancellationToken token)
        {
            switch (Type)
            {
                case PaginationLoaderType.Category:
                case PaginationLoaderType.Search:
                    return Bridge.GetSetLocationListAsync(targetId, takeNext, takePrevious, RaceId, categoryId, filter, taskId: TaskId, token);
                case PaginationLoaderType.MyAssets:
                    return Bridge.GetMySetLocationListAsync(targetId, takeNext, takePrevious, token);
                default:
                    Debug.LogError($"Unknown pagination loader type: {Type}");
                    return null;
            }
        }
        
        [UsedImplicitly]
        public class Factory: PaginationLoaderFactoryBase<SetLocationPaginationLoader, SetLocationFullInfo> { }
    }
}