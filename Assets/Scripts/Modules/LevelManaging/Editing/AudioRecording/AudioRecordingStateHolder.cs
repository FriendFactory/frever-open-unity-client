using JetBrains.Annotations;

namespace Modules.LevelManaging.Editing
{
    [UsedImplicitly]
    internal class AudioRecordingStateHolder : IAudioRecordingStateHolder
    {
        public AudioRecordingState State { get; private set; }
        
        public void UpdateState(AudioRecordingState state)
        {
            State = state;
        }
    }
}