using AssetBundleLoaders;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Modules.AssetsManaging.LoadArgs;
using Modules.LevelManaging.Assets;
using UnityEngine;

namespace Modules.AssetsManaging.Loaders
{
    internal sealed class VfxLoader : InstantiatableBundleAssetLoader<VfxInfo, VfxLoadArgs, VfxAsset>
    {
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public VfxLoader(IBridge bridge, IAssetBundleLoader assetBundleLoader, ISceneObjectHelper sceneObjectHelper) 
            : base(bridge, assetBundleLoader, sceneObjectHelper)
        {
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void InitAsset()
        {
            var gameObject = Target as GameObject;
            
            var view = new VfxAsset();
            view.Init(Model, gameObject, Bundle, Model.BodyAnimationAndVfx != null);
            MoveAssetToPersistentScene(view);

            view.EnableAudio(!LoadArgs.StopVfxAudioOnLoad);
            Asset = view;
        }
    }
}