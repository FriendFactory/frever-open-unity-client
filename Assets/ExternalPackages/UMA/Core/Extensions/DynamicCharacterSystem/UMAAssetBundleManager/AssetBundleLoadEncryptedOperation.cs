using System.IO;
using Bridge;
using Bridge.ExternalPackages.AsynAwaitUtility;
using UnityEngine;

namespace UMA.AssetBundles
{
    public class AssetBundleLoadEncryptedOperation : AssetBundleDownloadOperation
    {
        private readonly string _decompressedBundlePath;

        private bool _decrypted;
        private AssetBundle _assetBundle;
        private bool _initialized;
        private bool _isDone;
        private readonly IEncryptionBridge _encryptionService;

        public AssetBundleLoadEncryptedOperation(string assetBundleName, string assetBundlePath, IEncryptionBridge encryptionService): base(assetBundleName)
        {
            _encryptionService = encryptionService;
            _decompressedBundlePath = assetBundlePath;
        }

        public override bool Update()
        {
            if (!_initialized)
            {
                Initialize();
            }
            
            if (!_assetBundle) return true;
            
            FinishDownload();
            return false;
        }

        public override bool IsDone() => _initialized && _assetBundle;
        public override string GetSourceURL() => "";
        protected override bool downloadIsDone => true;

        protected override void FinishDownload()
        {
			bundle = _assetBundle;
			if (bundle == null)
			{
				if (Debug.isDebugBuild)
					Debug.LogWarning("[AssetBundleLoadOperation.AssetBundleLoadDecrypted] could not create bundle from decrypted bytes for " + assetBundleName);
			}
			else
			{
				assetBundle = new LoadedAssetBundle(bundle);
			}
        }

        private async void Initialize()
        {
            _initialized = true;
            
            if (!File.Exists(_decompressedBundlePath))
            {
                Debug.LogError($"[{GetType().Name}] DWC File does not exists - operation terminated # {_decompressedBundlePath}");
                return;
            }

            Debug.Log("DWC _encryptionService.DecryptFileToMemoryAsync " + _decompressedBundlePath);
            var data = await _encryptionService.DecryptFileToMemoryAsync(_decompressedBundlePath);
            _assetBundle = await AssetBundle.LoadFromMemoryAsync(data);
        }
    }
}