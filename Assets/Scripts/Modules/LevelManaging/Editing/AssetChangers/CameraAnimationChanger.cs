using System;
using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using Modules.AssetsManaging;
using Modules.AssetsManaging.LoadArgs;
using Modules.LevelManaging.Assets;
using UnityEngine;

namespace Modules.LevelManaging.Editing.AssetChangers
{
    internal sealed class CameraAnimationChanger : BaseChanger
    {
        private readonly IAssetManager _assetManager;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public CameraAnimationChanger(IAssetManager assetManager)
        {
            _assetManager = assetManager;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Run(CameraAnimationFullInfo target, Action<IAsset> onCompleted)
        {
            InvokeAssetStartedUpdating(DbModelType.CameraAnimation, target.Id);
            _assetManager.Load(target, asset =>
            {
                OnAssetUpdated();
                onCompleted?.Invoke(asset);
            }, Debug.LogError);
        }

        public void Run(CameraAnimationFullInfo model, string animationString)
        {
            var loadingArgs = new CameraAnimLoadArgs()
            {
                LoadFromMemoryImmediate = true,
                AnimationString = animationString,
                DeactivateOnLoad = false
            };
            
            _assetManager.Load(model, loadingArgs, asset =>OnAssetUpdated());
        }

        private void OnAssetUpdated()
        {
            InvokeAssetUpdated(DbModelType.CameraAnimation);
        }

        public void Unload(CameraAnimationFullInfo model)
        {
            _assetManager.Unload(model);
        }
    }
}