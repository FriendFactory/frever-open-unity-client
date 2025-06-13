using System;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using UnityEngine;

namespace Modules.LevelManaging.Assets
{
    public interface IBodyAnimationAsset: IAsset<BodyAnimationInfo>, IUnloadableAsset
    {
        AnimationClip BodyAnimation { get; }
        AssetBundle Bundle { get; }
    }

    internal sealed class BodyAnimationAsset : RepresentationAsset<BodyAnimationInfo>, IBodyAnimationAsset
    {
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override DbModelType AssetType => DbModelType.BodyAnimation;
        public AnimationClip BodyAnimation { get; private set; }
        public AssetBundle Bundle { get; private set; }
        
        //---------------------------------------------------------------------
        // Actions 
        //---------------------------------------------------------------------
        
        public event Action UnloadStarted;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Init(BodyAnimationInfo represent, AnimationClip animationClip, AssetBundle bundle)
        {
            BodyAnimation = animationClip;
            Bundle = bundle;
            BasicInit(represent);
        }

        public void OnUnloadStarted()
        {
            UnloadStarted?.Invoke();
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void SetModelActive(bool value)
        {
        }

    }
}