using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common;
using JetBrains.Annotations;
using UnityEngine;

namespace UIManaging.Pages.Feed.Core
{
    [UsedImplicitly]
    internal sealed class FeedLoadingQueue
    {
        private readonly LinkedList<FeedLoadingItem> _loadingQueue = new LinkedList<FeedLoadingItem>();
        
        private int _loadProcessesCount;

        private FeedLoadingItem _currentItem;
        private readonly CompetitiveLoadingManager _competitiveLoadingManager;
        private Coroutine _handleQueueRoutine;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private bool IsLoadingQueueRunning => _handleQueueRoutine != null;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action<FeedLoadingItem> VideoReachedQueue;
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public FeedLoadingQueue()
        {
            _competitiveLoadingManager = new CompetitiveLoadingManager();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void AddFirstToLoadingQueue(FeedLoadingItem item, bool closeMedia = false)
        {
            if (CheckCurrentItem(item)) return;
            if (CheckItemsInQueue(item)) return;

            _loadingQueue.AddFirst(item);

            if (closeMedia) item.View.CloseMedia();
            if (!IsLoadingQueueRunning) StartHandlingQueue();
        }

        public void AddLastToLoadingQueue(FeedLoadingItem item, bool closeMedia = false)
        {
            if (CheckCurrentItem(item)) return;
            if (CheckItemsInQueue(item)) return;

            _loadingQueue.AddLast(item);

            if (closeMedia) item.View.CloseMedia();
            if (!IsLoadingQueueRunning) StartHandlingQueue();
        }

        public void RemoveFromQueue(FeedVideoView targetView)
        {
            if (_currentItem != null && _currentItem.View == targetView)
            {
                UnregisterCurrentLoadingVideo();
            }
            else
            {
                var loadingItemInQueue = _loadingQueue.FirstOrDefault(x => x.View == targetView);
                if (loadingItemInQueue == null) return;
                _loadingQueue.Remove(loadingItemInQueue);
            }
        }
        
        public void Clear()
        {
            StopHandlingQueue();
            foreach (var loadingItem in _loadingQueue)
            {
                loadingItem.View.OnVideoReadyToPlayEvent -= OnVideoLoaded;
            }

            UnregisterCurrentLoadingVideo();
            
            _loadingQueue.Clear();
            _competitiveLoadingManager.Clear();
            _loadProcessesCount = 0;
        }

        private void UnregisterCurrentLoadingVideo()
        {
            if (_currentItem == null) return;
            _currentItem.View.OnVideoReadyToPlayEvent -= OnVideoLoaded;
            _currentItem = null;
            UnregisterLoadingProcess();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private bool CheckCurrentItem(FeedLoadingItem item)
        {
            // if isn't using same view, just continue
            if (_currentItem == null || _currentItem.View != item.View) return false;
            // If same video is already loading, let's skip new one
            if (_currentItem.Model.Video.Id == item.Model.Video.Id) return true;

            // If view is the same but video is different then reuse view
            // and skip current progress. This will yield coroutine for current item
            UnregisterCurrentLoadingVideo();
            return false;
        }

        private bool CheckItemsInQueue(FeedLoadingItem item)
        {
            var itemWithSameView = _loadingQueue.FirstOrDefault(loadingItem => loadingItem.View == item.View);
            // if there is no item with same view, just continue
            if (itemWithSameView == null) return false;

            // Remove item assigned for same view but different video ID and continue
            if (itemWithSameView.Model.Video.Id != item.Model.Video.Id)
            {
                _loadingQueue.Remove(itemWithSameView);
                return false;
            }

            // Replace item with same view and video ID if exists
            _loadingQueue.Find(itemWithSameView).Value = item;
            return true;
        }

        private void StartHandlingQueue()
        {
            if(IsLoadingQueueRunning) return;
            _handleQueueRoutine = CoroutineSource.Instance.StartCoroutine(HandleQueue());
        }

        private void StopHandlingQueue()
        {
            if (!IsLoadingQueueRunning) return;
           
            CoroutineSource.Instance.StopCoroutine(_handleQueueRoutine);
            _handleQueueRoutine = null;
        }

        private IEnumerator HandleQueue()
        {
            while (_loadingQueue.Any())
            {
                if (_loadProcessesCount >= _competitiveLoadingManager.OptimalVideoLoadingProcessesCount)
                {
                    yield return new WaitUntil(()=> _loadProcessesCount < _competitiveLoadingManager.OptimalVideoLoadingProcessesCount);
                }
                
                _currentItem = _loadingQueue.First();
                _loadingQueue.RemoveFirst();

                _competitiveLoadingManager.Register(_currentItem.View);
                
                if (!_currentItem.View.IsVideoLoading)
                {
                    InitializeVideoStreaming(_currentItem);
                }
                
                RegisterLoadingProcess();
                _currentItem.View.OnVideoReadyToPlayEvent += OnVideoLoaded;
                yield return new WaitUntil(() => _currentItem == null || _currentItem.View.IsReady);
                
                _currentItem = null;
            }
            _handleQueueRoutine = null;
        }

        private void RegisterLoadingProcess()
        {
            _loadProcessesCount++;
        }

        private void UnregisterLoadingProcess()
        {
            _loadProcessesCount--;
        }

        private void OnVideoLoaded(FeedVideoView item)
        {
            UnregisterLoadingProcess();
            item.OnVideoReadyToPlayEvent -= OnVideoLoaded;
        }

        private void InitializeVideoStreaming(FeedLoadingItem feedLoadingItem)
        {
            VideoReachedQueue?.Invoke(feedLoadingItem);
        }
    }
}