using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.ExternalPackages.AsynAwaitUtility;
using UnityEngine;
using ThreadPriority = UnityEngine.ThreadPriority;

namespace Modules.AssetsManaging.UncompressedBundles
{
    internal sealed class BundleDecompressor
    {
        private readonly IEncryptionBridge _encryptionService;

        public BundleDecompressor(IEncryptionBridge encryptionService)
        {
            _encryptionService = encryptionService;
        }

        public async Task DecompressAsync(string sourceBundlePath, string destinationPath, CancellationToken token)
        {
            var dir = Path.GetDirectoryName(destinationPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            if (_encryptionService.EncryptionEnabled)
            {
                await RecompressEncryptedAsync(sourceBundlePath, destinationPath, token);
                return;
            }
            await AssetBundle.RecompressAssetBundleAsync(sourceBundlePath, destinationPath, BuildCompression.LZ4Runtime, 0, ThreadPriority.High);
        }

        public async Task EncryptDecompressedFileAsync(string targetBundlePath, CancellationToken token)
        {
            // storage path may contains encrypted version extension, but actual file on disk is non encrypted version
            var decompressedBundlePath = targetBundlePath;
            var extension = Path.GetExtension(targetBundlePath);
            if (extension.Equals(_encryptionService.TargetExtension))
            {
                decompressedBundlePath = Path.ChangeExtension(targetBundlePath, null);
            }
            await _encryptionService.EncryptFileAsync(decompressedBundlePath, token);

            if (!string.Equals(decompressedBundlePath, targetBundlePath))
            {
                File.Move(decompressedBundlePath, targetBundlePath);
            }
        }

        private async Task RecompressEncryptedAsync(string sourceBundlePath, string destinationPath, CancellationToken token)
        {
            // recompression only supports reading from file, so, while it is possible to decrypt w/o copy operation,
            // it is better to leave source file untouched
            var tempFilePath = GetTemporaryFilePath();
            using (var tempFileStream = File.OpenWrite(tempFilePath))
            using (var decryptedStream = await _encryptionService.DecryptFileToMemoryStreamAsync(sourceBundlePath, token))
            {
                await decryptedStream.CopyToAsync(tempFileStream);
            }

            if (File.Exists(destinationPath)) File.Delete(destinationPath);
            
            await AssetBundle.RecompressAssetBundleAsync(tempFilePath, destinationPath, BuildCompression.LZ4Runtime, 0, ThreadPriority.High);
            await _encryptionService.EncryptFileAsync(destinationPath, token);
            
            if (File.Exists(tempFilePath)) File.Delete(tempFilePath);

            string GetTemporaryFilePath()
            {
                var tempDirectory = Path.GetDirectoryName(destinationPath);
                var tempFileName = Path.GetFileName(sourceBundlePath);
                
                return Path.Combine(tempDirectory, tempFileName);
            }
        }
    }
}