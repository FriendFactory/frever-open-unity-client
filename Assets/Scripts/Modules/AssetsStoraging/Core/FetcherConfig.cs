using UnityEditor;
using UnityEngine;

namespace Modules.AssetsStoraging.Core
{
    [CreateAssetMenu(fileName = "FetcherConfig.asset", menuName = "Friend Factory/Configs/Fetcher Config", order = 1)]
    public class FetcherConfig : ScriptableObject
    { 
        [SerializeField]
        private long _globalBundlesTypeId = 3;
        [SerializeField]
        private int _startPackConcurrentCount = 5;
        [SerializeField]
        private int _maxConcurrentRequestsCount = 50;

        public long GlobalBundlesTypeId => _globalBundlesTypeId;
        public int StartPackConcurrentCount => _startPackConcurrentCount;
        public int MaxConcurrentRequestsCount => _maxConcurrentRequestsCount;

    }
}