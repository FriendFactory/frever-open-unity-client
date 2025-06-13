using System;
using System.Collections;
using AssetBundleLoaders;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Modules.AssetsManaging.LoadArgs;
using Modules.LevelManaging.Assets;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Modules.AssetsManaging.Loaders
{
    internal sealed class BodyAnimationLoader : BundleAssetLoader<BodyAnimationInfo, BodyAnimationLoadArgs>
    {
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public BodyAnimationLoader(IBridge bridge, IAssetBundleLoader assetBundleLoader) : base(bridge, assetBundleLoader)
        {
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override IEnumerator UnpackBundleCoroutine(Action<Object> onUnpacked)
        {
            return AssetBundleLoader.LoadAnimationFromBundle(Bundle, onUnpacked, Debug.LogError);
        }

        protected override void InitAsset()
        {
            var view = new BodyAnimationAsset();
            var animClip = UnpackedObject as AnimationClip;
            view.Init(Model, animClip, Bundle);
            Asset = view;
        }
    }
}
