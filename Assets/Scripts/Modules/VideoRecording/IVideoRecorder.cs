using System;

namespace Modules.VideoRecording
{
    public interface IVideoRecorder
    {
        event Action VideoReady;
        event Action RecordingStarted;
        event Action RecordingEnded;
        event Action RecordingCanceled;
        
        bool IsCapturing { get; }
        bool IsRendering { get; }
        string FilePath { get; }
        int AudioDelayFrames { get; set; }

        void StartCapture(bool saveToGallery, bool isLandscapeMode, bool isUltraHD);
        void PauseCapture();
        void ResumeCapture();
        void StopCapture();
        void CancelCapture();
    }
}