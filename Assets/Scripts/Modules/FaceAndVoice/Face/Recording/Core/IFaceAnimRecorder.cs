using System.Threading.Tasks;
using Common.TimeManaging;
using Modules.FaceAndVoice.Face.Playing.Core;
using UnityEngine;

namespace Modules.FaceAndVoice.Face.Recording.Core
{
    public interface IFaceAnimRecorder
    {
        bool IsRecording { get;}
        FaceAnimationClip AnimationClip { get; }
        void Init(SkinnedMeshRenderer faceMesh);
        void StartRecording(ITimeSource timeSource, AudioSource syncSource = null);
        Task StopRecordingAsync();
        void CancelRecording();
    }
}
