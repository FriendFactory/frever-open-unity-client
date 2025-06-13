using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Models;
using Modules.AssetsManaging;
using Modules.FreverUMA;
using Modules.LevelManaging.Assets;

namespace Modules.LevelManaging.Editing.LevelPreview
{
    public interface ILevelEditorEventAssetsLoader
    {
        void LoadAssets(Event ev, bool unloadAllNotUsedAssets, Action<List<IAsset>> onComplete);
    }

    /// <summary>
    /// Responsible for set event in level editor or post record editor
    /// </summary>
    [UsedImplicitly]
    internal sealed class LevelEditorEventAssetsLoader : EventAssetsLoaderBase, ILevelEditorEventAssetsLoader
    {
        public LevelEditorEventAssetsLoader(IAssetManager assetManager, AvatarHelper avatarHelper, ReusedAssetsAlgorithm reusedAssetsAlgorithm, IConcreteAssetTypeLoader[] levelPreviewAwaitableAssetLoaders, IConcreteAssetTypeLoader[] levelPreviewAssetLoaders) :
            base(assetManager, avatarHelper, reusedAssetsAlgorithm, levelPreviewAwaitableAssetLoaders, levelPreviewAssetLoaders)
        {
        }
        
        public void LoadAssets(Event ev, bool unloadAllNotUsedAssets, Action<List<IAsset>> onComplete)
        {
            LoadAssetsInternal(new[]{ev}, unloadAllNotUsedAssets, onComplete);
        }

        protected override void OnCompleted()
        {
            AssetManager.DeactivateAllExceptFor(LoadedAssets.ToArray());
            foreach (var asset in LoadedAssets)
            {
                asset.SetActive(true);
            }
            base.OnCompleted();
        }
    }
}