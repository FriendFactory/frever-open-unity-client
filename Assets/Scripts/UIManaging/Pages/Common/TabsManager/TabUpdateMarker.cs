using System.Collections;
using Bridge;
using Common;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Common.TabsManager
{
    public class TabUpdateMarker : MonoBehaviour
    {
        private const float UPDATE_INTERVAL = 30f;

        [SerializeField] private GameObject _markerBody;
        [Inject] private IBridge _bridge;

        private long _lastSeenFeedVideoId = -1;
        private long _lastNewFeedVideoId = -1;
        private Coroutine _updateCoroutine;
        private WaitForSeconds _updateDelay;
        private bool _isOpened;

        private void Awake()
        {
            _updateDelay = new WaitForSeconds(UPDATE_INTERVAL);
        }

        public void SetIsOpened(bool value)
        {
            _isOpened = value;
            UpdateMarker();
        }

        private void OnEnable()
        {
            _updateCoroutine = CoroutineSource.Instance.StartCoroutine(PeriodicUpdate());
        }

        private IEnumerator PeriodicUpdate()
        {
            while (gameObject.activeSelf)
            {
                UpdateMarker();
                yield return _updateDelay;
            }
        }

        private async void UpdateMarker()
        {
            if (_bridge == null) return;
            
            var newestFeedVideoResult = await _bridge.GetNewestFeedVideoIdAsync();
            _lastNewFeedVideoId = newestFeedVideoResult.VideoId;

            if (_isOpened)
            {
                _lastSeenFeedVideoId = _lastNewFeedVideoId;
            }
            
            var hasSeenNewestVideo = _lastSeenFeedVideoId != _lastNewFeedVideoId　&& _lastSeenFeedVideoId != -1;
            _markerBody.SetActive(hasSeenNewestVideo);
        }

        private void OnDisable()
        {
            if (_updateCoroutine != null)
            {
                CoroutineSource.Instance?.StopCoroutine(_updateCoroutine);
            }
        }
    }
}
