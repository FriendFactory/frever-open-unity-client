using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Abstract;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups.SwipeToFollow.CardSwipe.Core
{
    internal sealed class UserCardStack2D : BaseContextPanel<List<UserCardStackElement>>
    {
        [SerializeField] private int _visibleCardsCount = 3;
        [SerializeField] private Vector2 _offset = new(0f, 80f);
        [SerializeField] private Vector2 _scaleOffset = new (0.05f, 0.05f);

        private RectTransform _rectTransform;
        private List<Vector2> _initialPositions;
        private int _topCardIndex;
        
        public async Task MoveNextAsync()
        {
            if (_topCardIndex == 0)
            {
                return;
            }

            _topCardIndex--;
            
            await UpdateStackAsync(true);
        }

        protected override async void OnInitialized()
        {
            try
            {
                _topCardIndex = ContextData.Count - 1;
                
                CacheInitialPositions();
                
                await UpdateStackAsync();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        protected override void BeforeCleanUp()
        {
            for (var i = 0; i < ContextData.Count; i++)
            {
                var card = ContextData[i];
                var initialPosition = _initialPositions[i];
                var initialScale = Vector3.one;
                
                card.RectTransform.anchoredPosition = initialPosition;
                card.RectTransform.localScale = initialScale;
                card.RectTransform.rotation = Quaternion.identity;
                
                card.CleanUp();
            }
        }

        private void CacheInitialPositions()
        {
            _initialPositions = ContextData.Select(card => card.RectTransform.anchoredPosition).ToList();
        }

        private async Task UpdateStackAsync(bool withAnimation = false)
        {
            var animationTasks = new List<Task>();
            
            for (var i = _topCardIndex; i >= 0; i--)
            {
                var card = ContextData[i];
                var indexFactor = _topCardIndex - i;
                var initialPosition = _initialPositions[i];
                var positionOffset = indexFactor * _offset;
                var scale = Mathf.Clamp01(1f - indexFactor * _scaleOffset.x);
                
                var targetPosition = initialPosition + positionOffset;
                var targetScale = new Vector3(scale, scale, 1);
                
                var visible = indexFactor < _visibleCardsCount;
                
                if (withAnimation && visible)
                {
                    animationTasks.Add(card.MoveAsync(targetPosition, targetScale, indexFactor));
                    
                    card.gameObject.SetActive(true);
                }
                else
                {
                    card.RectTransform.anchoredPosition = targetPosition;
                    card.RectTransform.localScale = targetScale;
                    
                    card.gameObject.SetActive(visible);
                }
                
                card.UpdateOverlay(indexFactor);
            }

            await Task.WhenAll(animationTasks);
        }
    }
}