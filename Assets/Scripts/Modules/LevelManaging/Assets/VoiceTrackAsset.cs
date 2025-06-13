using Bridge.Models.ClientServer.Assets;
using Extensions;
using Modules.FaceAndVoice.Voice.Recording.Core;
using UnityEngine;

namespace Modules.LevelManaging.Assets
{
    public interface IVoiceTrackAsset: IAsset<VoiceTrackFullInfo>
    {
        VoiceClip VoiceClip { get; }
    }
    
    internal sealed class VoiceTrackAsset : RepresentationAsset<VoiceTrackFullInfo>, IVoiceTrackAsset
    {
        public override DbModelType AssetType => DbModelType.VoiceTrack;

        public VoiceClip VoiceClip { get; } = new VoiceClip();

        private AudioSource _audioSource;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Init(VoiceTrackFullInfo represent, AudioClip clip)
        {
            VoiceClip.AudioClip = clip;
            BasicInit(represent);
        }

        public override void CleanUp()
        {
            if (VoiceClip.AudioClip != null)
            {
                Object.Destroy(VoiceClip.AudioClip);
            }
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void SetModelActive(bool value)
        {
        }
    }
}



