using Extensions;
using UnityEngine;
using Zenject;

namespace Modules.LevelManaging.Editing.EventRecording
{
    internal sealed class StopWatch: IStopWatch, ITickable
    {
        public long ElapsedMs => ElapsedSeconds.ToMilliseconds();

        public float ElapsedSeconds { get; private set; }

        private int _startRunningFrameNumber;
        private bool _isRunning;

        public long PreviousFrameMs { get; private set; }

        public void Start()
        {
            _startRunningFrameNumber = Time.frameCount;
            _isRunning = true;
        }
        
        public void Reset()
        {
            ElapsedSeconds = 0;
        }

        public void Stop()
        {
            _isRunning = false;
        }

        public void Tick()
        {
            if(!_isRunning) return;
            
            if (Time.frameCount == _startRunningFrameNumber)//don't include time passed since previous frame
                return;

            PreviousFrameMs = ElapsedSeconds.ToMilliseconds();
            ElapsedSeconds += Time.deltaTime;
        }
    }
}