using Bridge;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using Modules.AssetsManaging.LoadArgs;
using Modules.FaceAndVoice.Face.Recording.Core;
using Modules.LevelManaging.Assets;
using UnityEngine;

namespace Modules.AssetsManaging.Loaders
{
    internal sealed class FaceAnimationLoader : FileAssetLoader<FaceAnimationFullInfo, FaceAnimationLoadArgs>
    {
        private readonly FaceAnimationConverter _faceAnimationConverter;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public FaceAnimationLoader(IBridge bridge, FaceAnimationConverter faceAnimationConverter) : base(bridge)
        {
            _faceAnimationConverter = faceAnimationConverter;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void InitAsset()
        {
            var view = new FaceAnimationAsset();
            var textFile = Target as TextAsset;
            var parsedAnim = _faceAnimationConverter.ConvertToFaceAnimationData(textFile?.text);
            var faceAnimDuration = Model.Duration.ToSeconds();
            view.Init(Model, parsedAnim, faceAnimDuration);
            Asset = view;
        }
    }
}