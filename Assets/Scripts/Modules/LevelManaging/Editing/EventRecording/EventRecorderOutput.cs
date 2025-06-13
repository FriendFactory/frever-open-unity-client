using Bridge.Models.ClientServer.Assets;

namespace Modules.LevelManaging.Editing.EventRecording
{
    public struct EventRecorderOutput
    {
        public FaceAnimationFullInfo FaceAnimation;
        public VoiceTrackFullInfo VoiceTrack;
        public CameraAnimationFullInfo CameraAnimation;
        public long Length;
    }
}
