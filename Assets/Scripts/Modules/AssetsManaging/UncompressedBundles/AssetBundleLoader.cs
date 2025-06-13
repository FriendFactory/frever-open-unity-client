using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Exceptions;
using Bridge.ExternalPackages.AsynAwaitUtility;
using UnityEngine;
using UnityEngine.Networking;

namespace Modules.AssetsManaging.UncompressedBundles
{
    internal sealed class AssetBundleLoader
    {
        private readonly IEncryptionBridge _encryptionService;
        
        public AssetBundleLoader(IEncryptionBridge encryptionService)
        {
            _encryptionService = encryptionService;
        }

        public async Task<AssetBundle> Load(string bundlePath, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(bundlePath)) return null;

            var assetBundle = bundlePath.EndsWith(_encryptionService.TargetExtension)
                ? await LoadEncryptedBundleAsync(bundlePath, cancellationToken)
                : await DownloadBundleAsync(bundlePath, cancellationToken);

            return assetBundle;
        }

        private async Task<AssetBundle> DownloadBundleAsync(string bundlePath, CancellationToken cancellationToken = default)
        {
            using (var webRequest = UnityWebRequestAssetBundle.GetAssetBundle($"file://{bundlePath}"))
            {

                var req = webRequest.SendWebRequest();
                while (!req.isDone)
                {
                    await Task.Delay(25, cancellationToken);
                }
                if (webRequest.result == UnityWebRequest.Result.ProtocolError || webRequest.result == UnityWebRequest.Result.ConnectionError) 
                    return null;
                cancellationToken.ThrowIfCancellationRequested();
                return DownloadHandlerAssetBundle.GetContent(webRequest);
            }
        }

        private async Task<AssetBundle> LoadEncryptedBundleAsync(string bundlePath, CancellationToken cancellationToken = default)
        {
            try
            {
                var bundleData = await _encryptionService.DecryptFileToMemoryAsync(bundlePath, cancellationToken);

                return await AssetBundle.LoadFromMemoryAsync(bundleData);
            }
            catch (FileEncryptionException e)
            {
                Debug.LogError(FileEncryptionException.BuildErrorMessage($"Failed to decrypt bundle file: {e.Message}"));
                return null;
            }
        }
    }
}