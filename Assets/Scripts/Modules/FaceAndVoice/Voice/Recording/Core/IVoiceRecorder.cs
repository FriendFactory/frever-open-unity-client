using UnityEngine;

namespace Modules.FaceAndVoice.Voice.Recording.Core
{
    public interface IVoiceRecorder
    {
        AudioClip AudioClip { get; }
        bool IsRecording { get; set; }
        void StartRecording();
        VoiceClip StopRecording();
        void CancelRecording();
        void SetMicrophone(string microphoneUniqueId);
    }
}
