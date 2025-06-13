using System;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Modules.AssetsManaging.LoadArgs;
using Modules.CameraSystem.CameraAnimations;
using Modules.LevelManaging.Assets;
using UnityEngine;

namespace Modules.AssetsManaging.Loaders
{
    internal sealed class CameraAnimationLoader : FileAssetLoader<CameraAnimationFullInfo, CameraAnimLoadArgs>
    {
        private readonly CameraAnimationConverter _cameraAnimationConverter;
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public CameraAnimationLoader(IBridge bridge, CameraAnimationConverter cameraAnimationConverter) : base(bridge)
        {
            _cameraAnimationConverter = cameraAnimationConverter;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override async Task LoadAsset(CameraAnimLoadArgs args)
        {
            if (!args.LoadFromMemoryImmediate)
            {
                await base.LoadAsset(args);
            }
            else
            {
                if (string.IsNullOrEmpty(args.AnimationString))
                    throw new ArgumentNullException($"Animation string must be assigned for loading CameraAnimation from memory immediate");

                var fileAsset = new TextAsset(args.AnimationString);
                OnFileLoaded(fileAsset);
            }
        }

        protected override void InitAsset()
        {
            var textFile = Target as TextAsset;
            var cameraAnimAsset = CreateCameraAnimation(Model, textFile.text);
            Asset = cameraAnimAsset;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private ICameraAnimationAsset CreateCameraAnimation(CameraAnimationFullInfo model, string animationString)
        {
            var clipData = _cameraAnimationConverter.ConvertToClipData(animationString);
            var cameraAnimAsset = new CameraAnimationAsset();
            cameraAnimAsset.Init(model, clipData);
            return cameraAnimAsset;
        }
    }
}