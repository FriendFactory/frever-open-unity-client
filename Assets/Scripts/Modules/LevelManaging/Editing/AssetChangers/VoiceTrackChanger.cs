using Bridge.Models.ClientServer.Assets;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;
using UnityEngine;

namespace Modules.LevelManaging.Editing.AssetChangers
{
    [UsedImplicitly]
    internal sealed class VoiceTrackChanger : BaseChanger
    {
        private readonly IAssetManager _assetManager;

        public VoiceTrackChanger(IAssetManager assetManager)
        {
            _assetManager = assetManager;
        }
        
        public void Run(VoiceTrackFullInfo voiceTrack)
        {
            Unload(DbModelType.VoiceTrack);
            InvokeAssetStartedUpdating(DbModelType.VoiceTrack, voiceTrack.Id);
            _assetManager.Load(voiceTrack, OnVoiceTrackLoaded, Debug.LogWarning);
        }
        
        private void Unload(DbModelType dbModelType)
        {
            _assetManager.UnloadAll(dbModelType);
        }
        
        private void OnVoiceTrackLoaded(IAsset asset)
        {
            var voiceTrackAsset = asset as IVoiceTrackAsset;
            voiceTrackAsset?.SetActive(true);

            InvokeAssetUpdated(DbModelType.VoiceTrack);
        }
    }
}
