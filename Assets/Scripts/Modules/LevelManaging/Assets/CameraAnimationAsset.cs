using System.Collections.Generic;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using Modules.CameraSystem.CameraAnimations;
using UnityEngine;

namespace Modules.LevelManaging.Assets
{
    public interface ICameraAnimationAsset: IAsset<CameraAnimationFullInfo>
    {
        RecordedCameraAnimationClip Clip { get; }
        string Version { get; }
    }
    
    internal sealed class CameraAnimationAsset : RepresentationAsset<CameraAnimationFullInfo>, ICameraAnimationAsset
    {
        public override DbModelType AssetType => DbModelType.CameraAnimation;
        public RecordedCameraAnimationClip Clip { get; private set; }
        public string Version => RepresentedModel.GetVersion();

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Init(CameraAnimationFullInfo model, IDictionary<CameraAnimationProperty, AnimationCurve> animationCurves)
        {
            BasicInit(model);
            Clip = new RecordedCameraAnimationClip(animationCurves);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void SetModelActive(bool value)
        {
        }
    }
}