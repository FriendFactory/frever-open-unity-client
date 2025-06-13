using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace UIManaging.Pages.Feed.Core
{
    /// <summary>
    ///     Provides optimal value for parallel video downloading based on network speed
    /// </summary>
    internal sealed class CompetitiveLoadingManager
    {
        private const int VIDEO_TRACK_COUNT = 10;
        private const int DEFAULT_OPTIMAL_PROCESSES_COUNT = 3;

        private readonly Dictionary<FeedVideoView, Stopwatch> _loadingTime = new Dictionary<FeedVideoView, Stopwatch>();
        public int OptimalVideoLoadingProcessesCount { get; private set; } = DEFAULT_OPTIMAL_PROCESSES_COUNT;

        public void Register(FeedVideoView videoView)
        {
            _loadingTime[videoView] = new Stopwatch();
            videoView.OnStartedLoadingVideoEvent += StartTrackTime;
            videoView.OnVideoReadyToPlayEvent += StopTrackTime;
            videoView.OnVideoNotAvailableEvent += CancelTimeTracking;
        }

        public void Clear()
        {
            foreach (var videoView in _loadingTime.Keys)
            {
                videoView.OnStartedLoadingVideoEvent -= StartTrackTime;
                videoView.OnVideoReadyToPlayEvent -= StopTrackTime;
                videoView.OnVideoNotAvailableEvent -= CancelTimeTracking;
            }
            _loadingTime.Clear();
        }

        private void StartTrackTime(FeedVideoView view)
        {
            view.OnStartedLoadingVideoEvent -= StartTrackTime;
            _loadingTime[view].Start();
        }

        private void StopTrackTime(FeedVideoView view)
        {
            view.OnVideoReadyToPlayEvent -= StopTrackTime;
            view.OnVideoNotAvailableEvent -= CancelTimeTracking;
            
            _loadingTime[view].Stop();
            RecalculateAverageLoadingTime();
        }

        private void CancelTimeTracking(FeedVideoView view)
        {
            view.OnVideoReadyToPlayEvent -= StopTrackTime;
            view.OnVideoNotAvailableEvent -= CancelTimeTracking;
            
            _loadingTime[view].Stop();
            _loadingTime[view].Reset();
        }
        
        private void RecalculateAverageLoadingTime()
        {
            ClearDeprecatedData();
            
            var loadedVideoStopwatches = _loadingTime.Values.Where(x => !x.IsRunning && x.ElapsedMilliseconds != 0);
            var videoStopwatches = loadedVideoStopwatches as Stopwatch[] ?? loadedVideoStopwatches.ToArray();
            if (!videoStopwatches.Any())
            {
                OptimalVideoLoadingProcessesCount = DEFAULT_OPTIMAL_PROCESSES_COUNT;
                return;
            }

            var averageLoadingTime = videoStopwatches.Select(x => x.ElapsedMilliseconds).Average();
            OptimalVideoLoadingProcessesCount = GetOptimalCompetitiveLoadingProcessesCount(averageLoadingTime);
        }

        private int GetOptimalCompetitiveLoadingProcessesCount(double averageLoadingTime)
        {
            if (averageLoadingTime < 800)
            {
                return 4;
            }
            if (averageLoadingTime < 1000)
            {
                return 3;
            }
            return averageLoadingTime < 1200 ? 2 : 1;
        }

        private void ClearDeprecatedData()
        {
            if(_loadingTime.Count <= VIDEO_TRACK_COUNT) return;

            do
            {
                var oldestData = _loadingTime.First();
                _loadingTime.Remove(oldestData.Key);
            } while (_loadingTime.Count > VIDEO_TRACK_COUNT);
        }
    }
}