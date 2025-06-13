using Bridge.Services.UserProfile;
using DG.Tweening;
using Extensions;
using UIManaging.Localization;
using UIManaging.Pages.Common.FollowersManagement;
using UIManaging.Pages.Feed.Core;
using UIManaging.SnackBarSystem;
using Zenject;

namespace Modules.VideoStreaming.Feed
{
    internal sealed class FeedVideoFollowToggle : VideoFollowToggleBase
    {
        [Inject] private FollowersManager _followersManager;
        [Inject] private IProfilesProvider _profilesProvider;
        [Inject] private SnackBarHelper _snackbarHelper;
        [Inject] private FeedLocalization _localization;
        
        private Profile _creatorProfile;
        private bool _isFollowing;
        

        protected override void OnEnable()
        {
            base.OnEnable();
            
            if (_followersManager != null)
            {
                _followersManager.Followed += OnUserStartToFollow;
                _followersManager.UnFollowed += OnUserStopToFollow;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            _creatorProfile = null;
            
            if (_followersManager != null)
            {
                _followersManager.Followed -= OnUserStartToFollow;
                _followersManager.UnFollowed -= OnUserStopToFollow;
            }
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public override void Init(long videoGroupId)
        {
            base.Init(videoGroupId);
            
            VideoGroupId = videoGroupId;
            Hide();
            DownloadProfileAndRefresh();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void Hide()
        {
            _followToggle.interactable = false;
            _notFollowingCanvasGroup.SetEnable(false);
        }
        
        private void OnUserStartToFollow(Profile followed)
        {
            if (followed.MainGroupId != VideoGroupId) return;
            
            Hide();
            _creatorProfile.YouFollowUser = true;
        }
        
        private void OnUserStopToFollow(Profile followed)
        {
            if (followed.MainGroupId != VideoGroupId) return;
            
            _notFollowingCanvasGroup.SetEnable(true);
            _followToggle.interactable = true;
            _creatorProfile.YouFollowUser = false;
        }

        protected override void OnFollowToggleValueChanged(bool value)
        {
            if (_isFollowing && !value)
            {
                _followersManager.UnfollowUser(VideoGroupId, null, OnFollowChangeFailed);
                _isFollowing = false;
            }
            else if(!_isFollowing && value)
            {
                _followersManager.FollowUser(VideoGroupId, null, OnFollowChangeFailed);
                _isFollowing = true;
                ToggleTween();
            }
        }

        private void OnFollowChangeFailed()
        {
            _isFollowing = !_isFollowing;
            Refresh();
        }

        private async void DownloadProfileAndRefresh()
        {
            _creatorProfile = await _profilesProvider.GetProfile(VideoGroupId, TokenSource.Token);
            
            if (_creatorProfile == null) return;
            _isFollowing = _creatorProfile.YouFollowUser;
            Refresh();
        }
        
        private void Refresh()
        {
            if(_followToggle.isOn != _isFollowing)
            {
                _followToggle.isOn = _isFollowing;
            }

            if (ToggleSequence != null && ToggleSequence.IsPlaying()) return;
            
            _notFollowingCanvasGroup.SetEnable(!_followToggle.isOn);
            _followToggle.interactable = !_isFollowing;
        }
    }
}