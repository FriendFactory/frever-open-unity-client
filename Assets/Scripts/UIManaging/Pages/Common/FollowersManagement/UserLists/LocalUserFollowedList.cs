using System;
using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Bridge.Services.UserProfile;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Common.FollowersManagement.UserLists
{
    [UsedImplicitly]
    public sealed class LocalUserFollowedList: BaseUserList
    {
        public event Action<Profile> OnStartedFollowingUserEvent; 
        public event Action<Profile> OnStoppedFollowingUserEvent;
        public event Action<Profile> OnAddedFriend; 
        public event Action<Profile> OnRemovedFriend;
        
        [Inject] private IBridge _bridge;
        [Inject] private FollowersManager _followersManager;

        public async void FollowUser(long groupId, Action onSuccess = null, Action onFailure = null)
        {
            var followResponse = await _bridge.StartFollow(groupId);

            if (followResponse.IsSuccess)
            {
                Debug.Log($"User {groupId} is now followed!");
                
                var getLocalProfileResp = await _bridge.GetMyProfile();

                var followedProfile = followResponse.Profile;
                var alreadyExistingProfile = Profiles.FirstOrDefault(x => x.MainGroupId == followedProfile.MainGroupId);
                if (alreadyExistingProfile != null) Profiles.Remove(alreadyExistingProfile);
                
                Profiles.Add(followedProfile);

                if (getLocalProfileResp.IsSuccess)
                {
                    (await _followersManager.GetRemoteUserFollower(groupId).GetProfilesAsync()).Add(getLocalProfileResp.Profile);
                }
                
                _followersManager.UpdateProfileInRemoteUserLists(groupId, true);
                    
                OnStartedFollowingUserEvent?.Invoke(followedProfile);
                InvokeOnChanged();

                if (followedProfile.UserFollowsYou)
                {
                    OnAddedFriend?.Invoke(followedProfile);
                }

                if (getLocalProfileResp.IsError)
                {
                    Debug.LogError($"Failed to loaded profile for group with id {_bridge.Profile.GroupId}. Reason: {getLocalProfileResp.ErrorMessage}");
                    return;
                }
                
                onSuccess?.Invoke();
            }

            if (followResponse.IsError)
            {
                Debug.LogError($"Failed to follow user {groupId}! Reason: " + followResponse.ErrorMessage);
                onFailure?.Invoke();
            }
        }

        public async void UnfollowUser(long groupId, Action onSuccess = null, Action onFailure = null)
        {
            var unfollowResponse = await _bridge.StopFollow(groupId);

            if (unfollowResponse.IsSuccess)
            {
                Debug.Log($"User {groupId} is now unfollowed!");

                var remoteUserList = _followersManager.GetRemoteUserFollower(groupId);
                var profilesInRemoteList = await remoteUserList.GetProfilesAsync();
                var localProfile = profilesInRemoteList.FirstOrDefault(prof => prof.MainGroupId == _bridge.Profile.GroupId);

                var profileResult = await _bridge.GetProfile(groupId);
                var unfollowedProfile = profileResult.Profile;
                
                var remoteProfile = Profiles.FirstOrDefault(prof => prof.MainGroupId == groupId);
                Profiles.Remove(remoteProfile);

                if (localProfile != null)
                {
                    remoteUserList.Profiles.Remove(localProfile);
                }
                
                _followersManager.UpdateProfileInRemoteUserLists(groupId, false);
                    
                OnStoppedFollowingUserEvent?.Invoke(unfollowedProfile);
                InvokeOnChanged();

                if (remoteProfile != null && remoteProfile.UserFollowsYou)
                {
                    OnRemovedFriend?.Invoke(unfollowedProfile);
                }
                
                onSuccess?.Invoke();
            }

            if (unfollowResponse.IsError)
            {
                Debug.LogError($"Failed to unfollow user {groupId}! Reason: " + unfollowResponse.ErrorMessage);
                onFailure?.Invoke();
            }
        }

        protected override Task<ProfilesResult<Profile>> GetProfiles()
        {
            return _bridge.GetFollowedByCurrentUser(REQUEST_USER_COUNT, 0);
        }
        
        public override Task<ProfilesResult<Profile>> GetNextProfilesAsync(int take = 20, int skip = 0)
        {
            return _bridge.GetFollowedByCurrentUser(take, skip); 
        }
    }
}