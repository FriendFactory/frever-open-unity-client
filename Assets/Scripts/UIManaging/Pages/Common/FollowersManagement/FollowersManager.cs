using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Services.UserProfile;
using JetBrains.Annotations;
using UIManaging.Pages.Common.FollowersManagement.UserLists;
using UIManaging.Pages.Common.UsersManagement.BlockedUsersManaging;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Common.FollowersManagement
{
    [UsedImplicitly]
    public sealed class FollowersManager
    {
        private IBridge _bridge;
        private IBlockedAccountsManager _blockedAccountsManager;
        private LocalUserFollowedList _localUserFollowedList;
        private LocalUserFollowerList _localUserFollowerList;
        private IFactory<long, RemoteUserFollowedList> _remoteUserFollowedListFactory;
        private IFactory<long, RemoteUserFollowerList> _remoteUserFollowerListFactory;
        
        private readonly List<RemoteUserFollowedList> _remoteUserFollowedLists = new List<RemoteUserFollowedList>();
        private readonly List<RemoteUserFollowerList> _remoteUserFollowerLists = new List<RemoteUserFollowerList>();

        public LocalUserFollowedList LocalUserFollowed => _localUserFollowedList;
        public LocalUserFollowerList LocalUserFollower => _localUserFollowerList;

        
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action<long> FollowingStarted;
        public event Action<long> UnfollowingStarted;
        public event Action<long> OnRemoteUserFollowerChangedEvent;
        public event Action<long> OnRemoteUserFollowedChangedEvent;
        public event Action<Profile> Followed;
        public event Action<Profile> UnFollowed;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        [Inject, UsedImplicitly]
        public void Construct(IBridge bridge, IBlockedAccountsManager blockedAccountsManager, 
                              LocalUserFollowedList localUserFollowedList, LocalUserFollowerList localUserFollowerList, 
                              IFactory<long, RemoteUserFollowedList> remoteUserFollowedListFactory,
                              IFactory<long, RemoteUserFollowerList> remoteUserFollowerListFactory)
        {
            _bridge = bridge;
            _blockedAccountsManager = blockedAccountsManager;
            _localUserFollowedList = localUserFollowedList;
            _localUserFollowerList = localUserFollowerList;
            _remoteUserFollowedListFactory = remoteUserFollowedListFactory;
            _remoteUserFollowerListFactory = remoteUserFollowerListFactory;
            _blockedAccountsManager.AccountBlocked += OnAccountBlocked;
            _localUserFollowedList.OnStartedFollowingUserEvent += OnFollowing;
            _localUserFollowedList.OnStoppedFollowingUserEvent += OnUnFollowing;
        }

        ~FollowersManager()
        {
            if (_blockedAccountsManager != null)
            {
                _blockedAccountsManager.AccountBlocked -= OnAccountBlocked;
            }
            
            _localUserFollowedList.OnStartedFollowingUserEvent -= OnFollowing;
            _localUserFollowedList.OnStoppedFollowingUserEvent -= OnUnFollowing;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public async Task PrefetchDataForLocalUser()
        {
            await Task.WhenAll(_localUserFollowedList.GetProfilesAsync(), _localUserFollowerList.GetProfilesAsync());
        }
        
        public async Task PrefetchDataForRemoteUser(long groupId)
        {
            await Task.WhenAll(GetRemoteUserFollower(groupId).GetProfilesAsync(), GetRemoteUserFollowed(groupId).GetProfilesAsync());
        }

        public async void DownloadTopProfiles(int count, int skip, Action<Profile[]> onCallback, CancellationToken token = default)
        {
            var response = await _bridge.GetProfiles(count, skip, cancellationToken:token);

            if (response.IsSuccess)
            {
                onCallback?.Invoke(response.Profiles);
            }

            if (response.IsError)
            {
                Debug.LogError($"Failed to download top profiles. Reason: {response.ErrorMessage}");
            }
        }

        public RemoteUserFollowedList GetRemoteUserFollowed(long userGroupId)
        {
            var remoteUserFollower = _remoteUserFollowedLists.FirstOrDefault(x => x.UserGroupId == userGroupId);
            if (remoteUserFollower != null)
            {
                return remoteUserFollower;
            }

            var newRemoteList = _remoteUserFollowedListFactory.Create(userGroupId);
            
            newRemoteList.OnChanged += profiles => OnRemoteUserFollowedChanged(userGroupId);
            
            _remoteUserFollowedLists.Add(newRemoteList);
            return newRemoteList;
        }
        
        public RemoteUserFollowerList GetRemoteUserFollower(long userGroupId)
        {
            var remoteUserFollower = _remoteUserFollowerLists.FirstOrDefault(x => x.UserGroupId == userGroupId);
            if (remoteUserFollower != null)
            {
                return remoteUserFollower;
            }

            var newRemoteList = _remoteUserFollowerListFactory.Create(userGroupId);
            
            newRemoteList.OnChanged += profiles => OnRemoteUserFollowerChanged(userGroupId); // TODO: no unsubscribe, requires more complex logic (saving delegates in dictionary)
            
            _remoteUserFollowerLists.Add(newRemoteList);
            return newRemoteList;
        }

        public void UpdateProfileInRemoteUserLists(long groupId, bool status)
        {
            foreach (var list in _remoteUserFollowedLists.Concat<BaseUserList>(_remoteUserFollowerLists))
            {
                var profile = list.Profiles?.FirstOrDefault(prof => prof.MainGroupId == groupId);

                if (profile == null || profile.YouFollowUser == status)
                {
                    continue;
                }
                
                profile.YouFollowUser = status;
                UpdateProfileInCache(profile, list);
            }
        }

        public void FollowUser(long groupId, Action onSuccess = null, Action onFailure = null)
        {
            FollowingStarted?.Invoke(groupId);
            LocalUserFollowed.FollowUser(groupId, onSuccess, onFailure);
        }
        
        public void UnfollowUser(long groupId, Action onSuccess = null, Action onFailure = null)
        {
            UnfollowingStarted?.Invoke(groupId);
            LocalUserFollowed.UnfollowUser(groupId, onSuccess, onFailure);
        }

        public async Task<bool> IsFriend(long groupId)
        {
            var result = await _bridge.GetProfile(groupId);
            
            return result.Profile.YouFollowUser && result.Profile.UserFollowsYou;
        }
        
        public async Task<bool> IsFollowed(long groupId)
        {
            var result = await _bridge.GetProfile(groupId);
            return result.Profile.YouFollowUser;
        }
        
        public async Task<bool> IsFollower(long groupId)
        {
            var result = await _bridge.GetProfile(groupId);
            return result.Profile.UserFollowsYou;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnRemoteUserFollowerChanged(long userGroupId)
        {
            OnRemoteUserFollowerChangedEvent?.Invoke(userGroupId);
        }
        
        private void OnRemoteUserFollowedChanged(long userGroupId)
        {
            OnRemoteUserFollowedChangedEvent?.Invoke(userGroupId);
        }

        private void OnUnFollowing(Profile profile)
        {
            UpdateProfileInCache(profile, _localUserFollowerList);
            UnFollowed?.Invoke(profile);
        }
        
        private void OnFollowing(Profile profile)
        {
            UpdateProfileInCache(profile, _localUserFollowerList);
            Followed?.Invoke(profile);
        }
        
        private void OnAccountBlocked(Profile blocked)
        {
            _localUserFollowedList.RemoveFromCachedList(blocked.MainGroupId);
            _localUserFollowerList.RemoveFromCachedList(blocked.MainGroupId);
            
            var remoteFollowedList = _remoteUserFollowedLists.FirstOrDefault(x => x.UserGroupId == blocked.MainGroupId);
            remoteFollowedList?.RemoveFromCachedList(_bridge.Profile.GroupId);
            
            var remoteFollowerList = _remoteUserFollowedLists.FirstOrDefault(x => x.UserGroupId == blocked.MainGroupId);
            remoteFollowerList?.RemoveFromCachedList(_bridge.Profile.GroupId);
        }

        private void UpdateProfileInCache(Profile profile, BaseUserList userList)
        {
            if (!userList.ExistInCachedList(profile.MainGroupId)) return;
            userList.UpdateProfileCachedList(profile);
        }
    }
}


