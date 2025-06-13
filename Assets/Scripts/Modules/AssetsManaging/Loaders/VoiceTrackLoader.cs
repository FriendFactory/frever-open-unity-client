using Bridge;
using Bridge.Models.ClientServer.Assets;
using Modules.AssetsManaging.LoadArgs;
using Modules.LevelManaging.Assets;
using UnityEngine;

namespace Modules.AssetsManaging.Loaders
{
    internal sealed class VoiceTrackLoader : FileAssetLoader<VoiceTrackFullInfo, VoiceTrackLoadArgs>
    {
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public VoiceTrackLoader(IBridge bridge) : base(bridge)
        {
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void InitAsset()
        {
            var view = new VoiceTrackAsset();
            var voiceClip = Target as AudioClip;
            voiceClip.LoadAudioData(); 
            view.Init(Model, voiceClip);
            Asset = view;
        }
    }
}