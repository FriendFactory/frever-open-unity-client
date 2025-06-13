using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using UIManaging.Pages.Feed.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Modules.VideoStreaming.Feed
{
    public class FeedScrollController
    {
        public const int VIEWS_POOL_CAPACITY = 4;
        public const int VISIBLE_POSITION = 1;
        public const int LAST_POSITION = VIEWS_POOL_CAPACITY - 1;

        private List<FeedVideoView> _videoViews = new List<FeedVideoView>();
        private List<FeedVideoView> _recycledViews = new List<FeedVideoView>();

        internal IReadOnlyList<FeedVideoView> VideoViews => _videoViews;
        public int Amount { get; set; }
        public int Index { get; set; }
        public bool BlockScrolling { get; set; }
        
        private FeedVideoView Prefab { get; }
        private Action<int> OnPointerDown { get; set; }
        private Action<int> OnPointerUp { get; set; }

        internal FeedScrollController(FeedVideoView prefab)
        {
            Prefab = prefab;
        }

        internal void Setup(int amount, int index)
        {
            Amount = amount;
            Index = index;
        }

        public void AssignPointerCallbacks(Action<int> onPointerDown, Action<int> onPointerUp)
        {
            OnPointerDown = onPointerDown;
            OnPointerUp = onPointerUp;
        }


        public bool AllowToScrollDown()
        {
            if (BlockScrolling) return false;
            return _videoViews[VISIBLE_POSITION] == null 
                || _videoViews[VISIBLE_POSITION].IsReady
                || _videoViews.Count > VISIBLE_POSITION + 1 && (_videoViews[VISIBLE_POSITION + 1] == null 
                                                             || _videoViews[VISIBLE_POSITION + 1].IsReady);
        }
        
        public bool AllowToScrollUp()
        {
            if (BlockScrolling) return false;
            return _videoViews[VISIBLE_POSITION] == null
                || _videoViews[VISIBLE_POSITION].IsReady 
                || _videoViews[VISIBLE_POSITION - 1] == null
                || _videoViews[VISIBLE_POSITION - 1].IsReady;
        }

        public RectTransform GetInstanceByIndex(int index, RectTransform parent)
        {
            FeedVideoView instance;

            if (_recycledViews.Count > 0)
            {
                instance = _recycledViews.Last();
                instance.gameObject.SetActive(true);
                _recycledViews.Remove(instance);
            }
            else
            {
                instance = Object.Instantiate(Prefab, Vector2.zero, Quaternion.identity, parent);
            }

            instance.PointerUp += OnPointerUp;
            instance.PointerDown += OnPointerDown;

            if (index < _videoViews.Count)
            {
                _videoViews.Insert(index, instance);
            }
            else
            {
                _videoViews.Add(instance);
            }

            var instanceRect = instance.GetComponent<RectTransform>();

            instanceRect.FillParent();
            instanceRect.anchoredPosition = (VISIBLE_POSITION - index) * Vector2.up * instanceRect.rect.size.y;

            return instanceRect;
        }

        public void RecycleByIndex(int index)
        {
            if (index >= _videoViews.Count || index < 0)
            {
                return;
            }
            
            var removedElement = _videoViews[index];
            if (removedElement == null)
            {
                _videoViews.RemoveAt(index);
                return;
            }
            removedElement.PointerUp -= OnPointerUp;
            removedElement.PointerDown -= OnPointerDown;

            removedElement.gameObject.SetActive(false);
            _recycledViews.Add(removedElement);
            _videoViews.RemoveAt(index);
        }

        public override string ToString()
        {
            return $"Amount: {Amount}, Index: {Index}";
        }
    }
}