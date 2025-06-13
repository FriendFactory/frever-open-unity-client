using System;
using Bridge.Models.Common;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.PaginationLoaders
{
    public class PaginationLoaderFactoryBase<TLoader, TModel>: 
        PlaceholderFactory<PaginationLoaderType, Func<AssetSelectorModel>, long, NullableLong, string, NullableLong, TLoader>
        where TLoader: AssetSelectorPaginationLoader<TModel>
        where TModel: class, IEntity
    {
        public TLoader CreateCategoryLoader(long categoryId, Func<AssetSelectorModel> selectorModel, long universeId, long? taskId = null)
        {
            return Create(PaginationLoaderType.Category, selectorModel, universeId, categoryId, null, taskId);
        }

        public TLoader CreateSearchLoader(string filter, Func<AssetSelectorModel> selectorModel, long universeId, long? categoryId, long? taskId = null)
        {
            return Create(PaginationLoaderType.Search, selectorModel, universeId, categoryId, filter, taskId);
        }

        public TLoader CreateMyAssetsLoader(Func<AssetSelectorModel> selectorModel, long universeId, long? categoryId, long? taskId = null)
        {
            return Create(PaginationLoaderType.MyAssets, selectorModel, universeId, categoryId, null, taskId);
        }

        public TLoader CreateRecommendedLoader(Func<AssetSelectorModel> selectorModel, long universeId, long? categoryId, long? taskId = null)
        {
            return Create(PaginationLoaderType.Recommended, selectorModel, universeId, categoryId, null, taskId);
        }
    }
}