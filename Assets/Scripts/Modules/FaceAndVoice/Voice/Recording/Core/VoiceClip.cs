using Extensions;
using UnityEngine;

namespace Modules.FaceAndVoice.Voice.Recording.Core
{
    public class VoiceClip
    {
        public DbModelType AssetType => DbModelType.VoiceTrack;
        public AudioClip AudioClip { get; set; }
        public float Pitch { get; set; }
        public string RelativePath { get; set; }
    }
}
