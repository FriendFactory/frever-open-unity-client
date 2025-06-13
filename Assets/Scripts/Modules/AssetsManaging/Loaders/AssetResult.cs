using Modules.LevelManaging.Assets;

namespace Modules.AssetsManaging.Loaders
{
    public sealed class AssetResult
    {
        public bool IsSuccess { get; }
        public bool IsError { get; }
        public bool IsCancelled { get; }
        public string ErrorMessage { get; }
        public IAsset Asset { get; }

        public AssetResult(bool isCancelled)
        {
            IsCancelled = isCancelled;
        }

        public AssetResult(string errorMessage)
        {
            IsError = true;
            ErrorMessage = errorMessage;
        }

        public AssetResult(IAsset asset)
        {
            IsSuccess = true;
            Asset = asset;
        }
    }
}
