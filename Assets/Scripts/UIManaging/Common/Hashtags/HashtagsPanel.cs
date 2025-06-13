using System;
using System.Linq;
using System.Threading;
using Bridge;
using Bridge.Models.VideoServer;
using Extensions;
using UnityEngine;
using Zenject;
using static System.StringComparison;

namespace UIManaging.Common.Hashtags
{
    public sealed class HashtagsPanel : MonoBehaviour
    {
        public const string HASHTAG_TAG = "<link=\"hashtag:{1}\"><b>{0}</b></link>";
        private const int MAX_HASHTAG_LENGTH = 25;

        [Inject] private IBridge _bridge;

        [SerializeField] private HashtagsListView _hashtagsList;
        [SerializeField] private bool _suggestAddItem = true;
        [SerializeField] private bool _invokeClickedOnAutoComplete  = true;
        public bool IsActive
        {
            get => _isActive;
            private set
            {
                _isActive = value;
                gameObject.SetActive(value);
            }
        }

        private CancellationTokenSource _cancellationTokenSource;
        private string _filter;
        private HashtagInfo[] _hashtagInfos;
        private bool _isActive;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action<HashtagInfo> HashtagItemClicked;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _hashtagsList.ItemClicked += OnHashtagItemClicked;
        }

        private void OnDestroy()
        {
            _hashtagsList.ItemClicked -= OnHashtagItemClicked;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public async void Show(string filter)
        {
            var endsWithSpace = !string.IsNullOrEmpty(filter) && filter.EndsWith(" ");
            _filter = endsWithSpace ? filter.TrimEnd(' ') : filter;
            
            CancelHashtagsLoading();
            _cancellationTokenSource = new CancellationTokenSource();
            var result = await _bridge.GetHashtags(_filter, 0, 20, _cancellationTokenSource.Token);

            if (result.IsSuccess)
            {
                _hashtagInfos = result.Models;

                if (_suggestAddItem &&
                    !string.IsNullOrEmpty(_filter) &&
                    _hashtagInfos.All(info => !string.Equals(info.Name, _filter, OrdinalIgnoreCase)))
                {
                    var filterInfo = new HashtagInfo()
                    {
                        Name = _filter,
                        ViewsCount = -1
                    };

                    AddInfo(ref _hashtagInfos, filterInfo);
                }

                var listModel = new HashtagsListModel(_hashtagInfos);
                _hashtagsList.Initialize(listModel);
                _hashtagsList.Reload();

                IsActive = true;
                
                // completing hashtag if space has pressed or max hashtag length has reached
                if (endsWithSpace || _filter?.Length >= MAX_HASHTAG_LENGTH)
                {
                    if (!string.IsNullOrEmpty(_filter))
                    {
                        ApplyLatestFilteringResult();
                    }
                    Hide();
                }
            }
            else if (result.IsError)
            {
                Debug.LogWarning($"Cannot get hashtags: {result.ErrorMessage}");
            }
        }

        public void Hide()
        {
            _filter = string.Empty;
            
            CancelHashtagsLoading();
            IsActive = false;
        }

        public void ApplyLatestFilteringResult()
        {
            // case when user presses Mentions button right after activating hashtag panel
            if (string.IsNullOrEmpty(_filter) || _hashtagInfos.IsNullOrEmpty())
            {
                HashtagItemClicked?.Invoke(null);
                return;
            }
            
            var hashtagInfo = _hashtagInfos.SingleOrDefault(info => string.Equals(info.Name, _filter, OrdinalIgnoreCase));
            
            if (hashtagInfo == null) return;

            if (_invokeClickedOnAutoComplete)
            {
                HashtagItemClicked?.Invoke(hashtagInfo);
            }

            _filter = string.Empty;
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnHashtagItemClicked(HashtagInfo info)
        {
            HashtagItemClicked?.Invoke(info);
        }

        private void CancelHashtagsLoading()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
        }

        private static void AddInfo(ref HashtagInfo[] currentInfos, HashtagInfo newInfo)
        {
            var lenght = currentInfos.Length;
            var newInfos = new HashtagInfo[lenght + 1];
            newInfos[0] = newInfo;
            Array.Copy(currentInfos, 0, newInfos, 1, lenght);
            currentInfos = newInfos;
        }
    }
}