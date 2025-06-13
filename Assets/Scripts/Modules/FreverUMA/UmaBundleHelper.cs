using System;
using Bridge.Models.ClientServer.Assets;
using Modules.AssetsStoraging.Core;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UMA.AssetBundles;
using Zenject;

namespace Modules.FreverUMA
{
    [UsedImplicitly]
    public sealed class UmaBundleHelper
    {
        [Inject] private readonly IDataFetcher _fetcher;

        private readonly Dictionary<string, AssetBundleIndex.AssetBundleIndexList> _indexItemCache =
            new Dictionary<string, AssetBundleIndex.AssetBundleIndexList>();

        public void InitializeIndexer()
        {
            AssetBundleManager.UseDynamicIndexer = true;
            var indexer = UnityEngine.ScriptableObject.CreateInstance<AssetBundleIndex>();
            indexer.bundlesWithVariant = Array.Empty<string>();
            indexer.ownBundleHash = "";
            AssetBundleManager.AssetBundleIndexObject = indexer;
            AssetBundleManager.Initialize();
        }

        public List<UmaBundleFullInfo> GetGlobalBundles()
        {
            return _fetcher.MetadataStartPack.GlobalUmaBundles;
        }

        public void AddBundlesToIndex(IEnumerable<UmaBundleFullInfo> umaBundles)
        {
            foreach (var item in umaBundles)
            {
                AddBundleToIndex(item);
            }
        }

        public void AddBundleToIndex(UmaBundleFullInfo bundle)
        {
            if (AssetBundleManager.AssetBundleIndexObject.bundlesIndex.Exists(x => x.assetBundleName == bundle.Name)) return;

            AssetBundleIndex.AssetBundleIndexList listItem = null;
            if (_indexItemCache.ContainsKey(bundle.Name))
            {
                listItem = _indexItemCache[bundle.Name];
            }
            else
            {
                listItem = new AssetBundleIndex.AssetBundleIndexList(bundle.Name);
                foreach (var asset in bundle.UmaAssets)
                {
                    if (asset.UmaAssetFiles.Count == 0) continue;
                    var file = asset.UmaAssetFiles.FirstOrDefault();
                    var typeId = file.UnityAssetTypesIds.FirstOrDefault();
                    var type = _fetcher.MetadataStartPack.UnityAssetTypes.Find(x => x.Id == typeId);

                    var indexItem = new AssetBundleIndex.AssetBundleIndexItem();
                    indexItem.filename = file.Name;
                    indexItem.assetName = asset.Name;
                    indexItem.assetHash = (int)asset.Hash;
                    indexItem.assetType = type.Name;
                    indexItem.assetWardrobeSlot = asset.SlotId != null ? asset.SlotName : string.Empty;

                    listItem.assetBundleAssets.Add(indexItem);
                }
                listItem.allDependencies = bundle.DependentUmaBundles.Select(x => x.Name).ToArray();
                _indexItemCache[bundle.Name] = listItem;
            }
            
            AssetBundleManager.AssetBundleIndexObject.bundlesIndex.Add(listItem);
        }
        
        public void RemoveBundleFromIndex(string bundleName)
        {
            var bundleIndexModel = AssetBundleManager.AssetBundleIndexObject.bundlesIndex.FirstOrDefault(
                    x => x.assetBundleName == bundleName);
            if (bundleIndexModel != null)
            {
                AssetBundleManager.AssetBundleIndexObject.bundlesIndex.Remove(bundleIndexModel);
            }
        }
    }
}