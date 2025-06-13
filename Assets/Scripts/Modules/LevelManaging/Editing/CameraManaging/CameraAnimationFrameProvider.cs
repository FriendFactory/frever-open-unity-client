using System.IO;
using System.Linq;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using Modules.AssetsManaging;
using Modules.CameraSystem.CameraAnimations;
using Modules.LevelManaging.Assets;

namespace Modules.LevelManaging.Editing.CameraManaging
{
    /// <summary>
    ///     Extract camera animation one frame in most efficient way.
    ///     If we have already camera animation in RAM, it will use this as source.
    ///     Otherwise, it will read and parse only last frame from txt file
    /// </summary>
    internal sealed class CameraAnimationFrameProvider
    {
        private readonly IAssetManager _assetManager;
        private readonly CameraAnimationConverter _cameraAnimationConverter;
        private readonly IBridgeCache _bridgeCache;

        public CameraAnimationFrameProvider(IAssetManager assetManager, CameraAnimationConverter cameraAnimationConverter, IBridgeCache bridgeCache)
        {
            _assetManager = assetManager;
            _cameraAnimationConverter = cameraAnimationConverter;
            _bridgeCache = bridgeCache;
        }

        public CameraAnimationFrame GetLastFrame(CameraAnimationFullInfo cameraAnimation)
        {
            if (HasLoaded(cameraAnimation, out var asset))
            {
                return asset.Clip.GetFrame(asset.Clip.Length);
            }

            var fileInfo = cameraAnimation.Files.First();
            var filePath = fileInfo.FilePath ?? _bridgeCache.GetFilePath(cameraAnimation, fileInfo);

            var cameraAnimTxt = File.ReadAllText(filePath);
            return _cameraAnimationConverter.ExtractLastFrame(cameraAnimTxt);
        }

        private bool HasLoaded(CameraAnimationFullInfo cameraAnimation, out ICameraAnimationAsset asset)
        {
            var version = cameraAnimation.GetVersion();
            var allLoaded = _assetManager.GetAllLoadedAssets<ICameraAnimationAsset>();
            asset = allLoaded.FirstOrDefault(x => x.Id == cameraAnimation.Id && x.Version == version);
            return asset != null;
        }
    }
}