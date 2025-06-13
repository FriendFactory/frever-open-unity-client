using Extensions;
using UnityEngine;
using Zenject;

namespace Common.TimeManaging
{
    /// <summary>
    ///    Time source based on unity life cycle loop timing(frame Time.deltaTime)
    /// </summary>
    public interface IUnityTimeBasedTimeSource: ITimeSource
    {
        bool IsRunning { get; }
        void Start(float startFromSec = 0);
        void Stop();
    }
    
    internal sealed class UnityTimeBasedTimeSource: IUnityTimeBasedTimeSource, ITickable
    {
        public long ElapsedMs => ElapsedSeconds.ToMilliseconds();
        public float ElapsedSeconds { get; private set; }
        public bool IsRunning { get; private set; }
            
        public void Start(float startFromSec = 0)
        {
            IsRunning = true;
            ElapsedSeconds = startFromSec;
        }

        public void Stop()
        {
            IsRunning = false;
        }
            
        public void Tick()
        {
            if(!IsRunning) return;
                
            ElapsedSeconds += Time.deltaTime;
        }
    }
}