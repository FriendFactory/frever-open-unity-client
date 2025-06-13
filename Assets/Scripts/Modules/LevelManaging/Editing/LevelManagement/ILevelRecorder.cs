using System;
using System.Threading.Tasks;
using Modules.LevelManaging.Editing.RecordingModeSelection;

namespace Modules.LevelManaging.Editing.LevelManagement
{
    public interface ILevelRecorder
    {
        event Action EventForRecordingSetup;
        event Action RecordingStarted;
        event Action RecordingEnded;
        event Action RecordingCancelled;
        event Action FaceRecordingStateChanged;
        
        RecordingMode CurrentRecordingMode { get; }
        bool IsFaceRecordingEnabled { get; }
        
        void StartRecordingEvent();
        Task StopRecordingEventAsync();
        void CancelRecordingEvent();
        void SetFaceRecording(bool isEnabled);
        void ChangeRecordingMode(RecordingMode mode);
    }
}