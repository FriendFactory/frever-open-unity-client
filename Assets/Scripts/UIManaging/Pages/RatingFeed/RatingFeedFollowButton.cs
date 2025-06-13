using System;
using Abstract;
using Bridge;
using Bridge.Models.VideoServer;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.RatingFeed
{
    internal sealed class RatingFeedFollowButton:  BaseContextDataButton<Video>
    {
        [SerializeField] private TMP_Text _friendStatus;
       
        private IBridge _bridge;
        private RatingFeedFollowStatusCache _followStatusCache;

        private UserFollowStatus _followingStatus;
        private RectTransform _rectTransform;
        
        [Inject, UsedImplicitly]
        private void Construct(IBridge bridge, RatingFeedFollowStatusCache followStatusCache)
        {
            _bridge = bridge;
            _followStatusCache = followStatusCache;
        }

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        protected override void OnInitialized()
        {
            _followingStatus = GetUserFollowStatus(ContextData);
            
            UpdateFollowingStatusText();
            
            _button.interactable = !ContextData.IsFriend && !ContextData.IsFollowed;
        }

        protected override async void OnUIInteracted()
        {
            try
            {
                base.OnUIInteracted();

                var result = await _bridge.StartFollow(ContextData.GroupId);
                if (result.IsError)
                {
                    Debug.LogError($"[{GetType().Name}] Failed to follow user: {result.ErrorMessage}");
                    return;
                }
                
                _followingStatus = ContextData.IsFollower ? UserFollowStatus.Friend : UserFollowStatus.Follower;
                
                if (_followStatusCache.TryGet(ContextData.GroupId, out _)) return;
                
                _followStatusCache.Add(ContextData.GroupId, _followingStatus);
                
                UpdateFollowingStatusText();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void UpdateFollowingStatusText()
        {
            _friendStatus.text = GetFollowingStatusText(_followingStatus);

            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
        }

        private UserFollowStatus GetUserFollowStatus(Video video)
        {
            if (_followStatusCache.TryGet(video.GroupId, out var status)) return status;
            
            // TODO: add l10n
            if (video.IsFriend)
            {
                return UserFollowStatus.Friend;
            }

            return video.IsFollower ? UserFollowStatus.Follower : UserFollowStatus.NotFollowed;
        }

        private string GetFollowingStatusText(UserFollowStatus status)
        {
            return status switch
            {
                UserFollowStatus.Friend => "Friend",
                UserFollowStatus.Follower => "Followed",
                UserFollowStatus.NotFollowed => "Follow",
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };
        }
    }
}