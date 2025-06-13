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
    public class CameraFilterPaginationLoader : AssetSelectorPaginationLoader<CameraFilterInfo>
    {
        private CameraFilterPaginationLoader(PaginationLoaderType type, Func<AssetSelectorModel> selectorModel, long universeId,
            NullableLong categoryId = null, string filter = null, NullableLong taskId = null): base(type, selectorModel, universeId, categoryId, filter, taskId) { }

        private string GetCategoryName(CameraFilterInfo cameraFilterInfo)
        {
            return DataFetcher.MetadataStartPack.CameraFilterCategories.First(x => x.Id == cameraFilterInfo.CameraFilterCategoryId).Name;
        }

        protected override IList<AssetSelectionItemModel> CreateItemModels(IList<CameraFilterInfo> page)
        {
            var assetData = DataFetcher.DefaultUserAssets.PurchasedAssetsData;
            var purchasedIds = assetData.GetPurchasedAssetsByType(DbModelType.CameraFilter);
            
            return page
                  .Where(model => model != null)
                  .Select(cameraFilterInfo => new AssetSelectionCameraFilterModel(
                              Resolution._128x128, 
                              cameraFilterInfo, 
                              GetCategoryName(cameraFilterInfo))
                          {
                              IsPurchased = purchasedIds.Contains(cameraFilterInfo.Id)
                          })
                  .ToList<AssetSelectionItemModel>();
        }

        protected override Task<ArrayResult<CameraFilterInfo>> GetAssetListAsync(long? targetId, int takeNext, int takePrevious, long? categoryId, string filter,
                                                                                 CancellationToken token)
        {
            switch (Type)
            {
                case PaginationLoaderType.Category:
                case PaginationLoaderType.Search:
                    return Bridge.GetCameraFilterListAsync(targetId, takeNext, takePrevious, filter, categoryId,
                                                           taskId: TaskId,
                                                           token: token);
                case PaginationLoaderType.MyAssets:
                    return Bridge.GetMyCameraFilterListAsync(targetId, takeNext, takePrevious, token);
                default:
                    Debug.LogError($"Unknown pagination loader type: {Type}");
                    return null;
            }
        }
       
        [UsedImplicitly]
        public class Factory: PaginationLoaderFactoryBase<CameraFilterPaginationLoader, CameraFilterInfo> { }
    }
}