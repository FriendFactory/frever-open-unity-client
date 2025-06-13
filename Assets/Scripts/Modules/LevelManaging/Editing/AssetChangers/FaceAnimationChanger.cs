using Bridge.Models.ClientServer.Assets;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;
using UnityEngine;

namespace Modules.LevelManaging.Editing.AssetChangers
{
    [UsedImplicitly]
    internal sealed class FaceAnimationChanger : BaseChanger
    {
        private readonly IAssetManager _assetManager;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public FaceAnimationChanger(IAssetManager assetManager)
        {
            _assetManager = assetManager;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void Run(FaceAnimationFullInfo faceAnim)
        {
            _assetManager.UnloadAll(DbModelType.FaceAnimation);

            if (faceAnim == null) return;
            
            InvokeAssetStartedUpdating(DbModelType.FaceAnimation, faceAnim.Id);
            _assetManager.Load(faceAnim, OnFaceAnimLoaded, Debug.LogWarning);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnFaceAnimLoaded(IAsset asset)
        {
            asset?.SetActive(true);

            InvokeAssetUpdated(DbModelType.FaceAnimation);
        }
    }
}
