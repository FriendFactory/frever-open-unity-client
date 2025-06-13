namespace Modules.LevelManaging.Editing
{
    public interface IAudioRecordingStateHolder
    {
        AudioRecordingState State { get; }

        void UpdateState(AudioRecordingState state);
    }
}