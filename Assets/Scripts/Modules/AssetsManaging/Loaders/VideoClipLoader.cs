using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Modules.AssetsManaging.LoadArgs;
using Modules.LevelManaging.Assets;

namespace Modules.AssetsManaging.Loaders
{
    internal sealed class VideoClipLoader : FileAssetLoader<VideoClipFullInfo, VideoClipLoadArgs>
    {
        private string _filePath;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public VideoClipLoader(IBridge bridge) : base(bridge)
        {
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void InitAsset()
        {
            var view = new VideoClipAsset();
            view.Init(Model, _filePath);
            Asset = view;
        }

        protected override async Task Download(CancellationToken cancellationToken = default)
        {
            var result = await Bridge.GetAssetAsync(Model, true, cancellationToken);
            
            if (IsCancellationRequested)
            {
                OnCancelled();
                return;
            }
            
            if (result.IsSuccess)
            {
                _filePath = result.FilePath;
                OnFileLoaded(result.Object);
            }
            else if (result.IsError)
            {
                OnFailed(GetErrorResult(result.ErrorMessage));
            }
        }
    }
}