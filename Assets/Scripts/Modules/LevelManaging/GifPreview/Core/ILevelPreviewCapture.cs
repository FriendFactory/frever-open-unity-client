using System;
using System.Collections;

namespace Modules.LevelManaging.GifPreview.Core
{
    public interface ILevelPreviewCapture
    {
        LevelPreview LastCaptured { get; set; }
        bool IsRunning { get; set; }
        void StartCapture(int snapshotsCount, int frameRate);
        void StopCapture();

        IEnumerator BakeGif(Action onCompleted, Action onFailed);
    }
}
