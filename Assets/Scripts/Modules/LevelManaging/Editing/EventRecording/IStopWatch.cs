using Common.TimeManaging;

namespace Modules.LevelManaging.Editing.EventRecording
{
    internal interface IStopWatch: ITimeSource
    {
        long PreviousFrameMs { get; }
        void Start();
        void Reset();
        void Stop();
    }
}