using System;
using System.Threading.Tasks;
using UnityEngine;

namespace UIManaging.Common
{
    public class PeriodicUpdater : MonoBehaviour
    {
        public Func<Task<bool>> CallUpdateFunc { get; set; }
        public Func<bool> IsRefreshing { get; set; }
        
        private readonly int[] _refreshWaitTimes = { 1, 2, 5, 10, 20, 30, 60 };
        private int _refreshWaitTimeIndex;
        private float _lastRefreshTime;

        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Update()
        {
            CheckForUpdate();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public virtual async void CheckForUpdate()
        {
            if (!IsRefreshing?.Invoke() ?? false)
            {
                return;
            }

            var currentTime = Time.unscaledTime;
            
            if (currentTime - _lastRefreshTime < _refreshWaitTimes[_refreshWaitTimeIndex])
            {
                return;
            }

            _lastRefreshTime = currentTime;
            
            var result = CallUpdateFunc != null && await CallUpdateFunc.Invoke();
            
            _refreshWaitTimeIndex = result ? 0 : Mathf.Min(_refreshWaitTimeIndex + 1, _refreshWaitTimes.Length - 1);
        }
        
        public void ResetRefreshingTime()
        {
            _lastRefreshTime = 0;
            _refreshWaitTimeIndex = 0;
        }
    }
}