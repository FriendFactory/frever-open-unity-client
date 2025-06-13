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
    public class VfxPaginationLoader : AssetSelectorPaginationLoader<VfxInfo>
    {
        private VfxPaginationLoader(PaginationLoaderType type, Func<AssetSelectorModel> selectorModel, long universeId,
            NullableLong categoryId = null, string filter = null, NullableLong taskId = null): 
            base(type, selectorModel, universeId,categoryId, filter, taskId) { }
        
        private string GetCategoryName(VfxInfo vfx)
        {
            return DataFetcher.MetadataStartPack.VfxCategories.First(x => x.Id == vfx.VfxCategoryId).Name;
        }
            
        protected override IList<AssetSelectionItemModel> CreateItemModels(IList<VfxInfo> page)
        {
            var assetData = DataFetcher.DefaultUserAssets.PurchasedAssetsData;
            
            return page.Select(vfxInfo => new AssetSelectionVfxModel(Resolution._128x128,
                                                                     vfxInfo, GetCategoryName(vfxInfo))
            {
                IsPurchased = assetData.GetPurchasedAssetsByType(DbModelType.Vfx).Contains(vfxInfo.Id),
            }).ToList<AssetSelectionItemModel>();
        }

        protected override async Task<ArrayResult<VfxInfo>> GetAssetListAsync(long? targetId, int takeNext, int takePrevious, long? categoryId, string filter,
            CancellationToken token)
        {
            switch (Type)
            {
                case PaginationLoaderType.Category:
                case PaginationLoaderType.Search:
                    var withAnimationOnly = categoryId is < 0;
                    var categoryIdWithAnimations = withAnimationOnly ? null : categoryId;
                    var result = await Bridge.GetVfxListAsync(targetId, takeNext, takePrevious, raceId: 1, filter,  categoryIdWithAnimations, taskId: TaskId, withAnimationOnly: withAnimationOnly, token:token);//todo
                    
                    // we need to override the original VfxInfo in order to be able to distinguish between the bundled and the normal one version
                    // based on the Id + CategoryId
                    if (result.IsSuccess)
                    {
                        foreach (var vfxInfo in result.Models)
                        {
                            if (categoryId.HasValue)
                            {
                                vfxInfo.VfxCategoryId = withAnimationOnly ? -1 : categoryId.Value;
                            }
                            
                            vfxInfo.BodyAnimationAndVfx = withAnimationOnly ? vfxInfo.BodyAnimationAndVfx : null;
                        }
                    }
                    
                    return result;
                case PaginationLoaderType.MyAssets:
                    return await Bridge.GetMyVfxListAsync(targetId, takeNext, takePrevious, token);
                default:
                    Debug.LogError($"Unknown pagination loader type: {Type}");
                    return null;
            }
        }
      
        [UsedImplicitly]
        public class Factory: PaginationLoaderFactoryBase<VfxPaginationLoader, VfxInfo> { }
    }
}