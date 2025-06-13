using System.Collections.Generic;
using System.Diagnostics;

namespace Common
{
    public sealed class StopWatchProvider
    {
        private readonly Stack<Stopwatch> _stopWatches = new Stack<Stopwatch>();
        
        public Stopwatch GetStopWatch()
        {
            var stopWatch = _stopWatches.Count != 0 ? _stopWatches.Pop() : CreateStopWatch();
            return stopWatch;
        }

        public void Dispose(Stopwatch stopwatch)
        {
            if (stopwatch == null) return;
            
            _stopWatches.Push(stopwatch);
        }
        
        private Stopwatch CreateStopWatch()
        {
            return new Stopwatch();
        }
    }
}
