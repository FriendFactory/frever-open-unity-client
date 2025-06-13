using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common;
using JetBrains.Annotations;
using Modules.AssetsManaging;
using Modules.FreverUMA;
using Modules.LevelManaging.Assets;
using UnityEngine;
using Event = Models.Event;

namespace Modules.LevelManaging.Editing.LevelPreview
{
    public interface ILevelPreviewAssetsLoader
    {
        void LoadAssets(Event[] events, bool unloadAllNotUsedAssets, Action<List<IAsset>> onComplete, Action onCancel = null);
        void CancelLoadingAssets();
    }
    
    /// <summary>
    /// Responsible for loading assets for level preview
    /// </summary>
    [UsedImplicitly]
    internal sealed class LevelPreviewAssetsLoader: EventAssetsLoaderBase, ILevelPreviewAssetsLoader
    {
        private ICollection<Event> _loadingEvents;

        public LevelPreviewAssetsLoader(IAssetManager assetManager, AvatarHelper avatarHelper, ReusedAssetsAlgorithm reusedAssetsAlgorithm, IConcreteAssetTypeLoader[] levelPreviewAwaitableAssetLoaders, IConcreteAssetTypeLoader[] levelPreviewAssetLoaders) : 
            base(assetManager, avatarHelper, reusedAssetsAlgorithm, levelPreviewAwaitableAssetLoaders, levelPreviewAssetLoaders)
        {
        }
        
        public void LoadAssets(Event[] events, bool unloadAllNotUsedAssets, Action<List<IAsset>> onComplete, Action onCancel = null)
        {
            _loadingEvents = events;
            LoadAssetsInternal(events, unloadAllNotUsedAssets, onComplete, onCancel);
        }

        protected override void OnCompleted()
        {
            LeaveActiveAssetsOnlyFromFirstEvent();
            CoroutineSource.Instance.StartCoroutine(FinalizeLoading());
        }

        private void LeaveActiveAssetsOnlyFromFirstEvent()
        {
            var firstEvent = _loadingEvents.OrderBy(x => x.LevelSequence).First();
            var firstEventAssets = ReusedAssetsAlgorithm.GetAlreadyLoadedAssetsUsedBy(firstEvent);
            AssetManager.DeactivateAllExceptFor(firstEventAssets);
        }

        private IEnumerator FinalizeLoading()
        {
            CleanupMemory();
            //waiting a frame for potential GC spike
            yield return null;
            base.OnCompleted();
        }
        
        private void CleanupMemory()
        {
            AvatarHelper.UnloadAllUmaBundles();
            GC.Collect();
            Resources.UnloadUnusedAssets();
        }
    }
}