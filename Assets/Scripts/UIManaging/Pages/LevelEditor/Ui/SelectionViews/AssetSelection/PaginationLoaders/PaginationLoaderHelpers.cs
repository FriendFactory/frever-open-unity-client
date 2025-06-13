namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.PaginationLoaders
{
    public static class PaginationLoaderHelpers
    {
        public static string LoaderTypeToString(PaginationLoaderType type, long? categoryId = null, string filter = null)
        {
            switch (type)
            {
                case PaginationLoaderType.Category:
                    return $"CategoryId:{categoryId}";
                case PaginationLoaderType.Search:
                    return filter;
                case PaginationLoaderType.MyAssets:
                    return "MyAssetsCategory";
                case PaginationLoaderType.Recommended:
                    return "RecommendedCategory";
                default:
                    return null;
            }
        }
    }
}