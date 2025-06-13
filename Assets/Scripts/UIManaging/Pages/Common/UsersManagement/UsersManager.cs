using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Services.UserProfile;
using JetBrains.Annotations;
using Models;
using UnityEngine;

namespace UIManaging.Pages.Common.UsersManagement
{
    [UsedImplicitly]
    public sealed class UsersManager
    {
        private readonly List<Profile> _cachedUsers;
        private readonly IBridge _bridge;
        
        public UsersManager(IBridge bridge)
        {
            _bridge = bridge;
            _cachedUsers = new List<Profile>();
        }

        public Task<Profile> DownloadRemoteUser(long userGroupId, bool useCache = true, Action<string> onFailed = null, CancellationToken cancellationToken = default)
        {
            return DownloadUserInternal(userGroupId, useCache, onFailed, cancellationToken);
        }
        
        private async Task<Profile> DownloadUserInternal(long userMainGroupId, bool useCache, Action<string> onFailed, CancellationToken cancellationToken = default)
        {
            if (useCache)
            {
                var cachedUser = GetCachedUserByGroupId(userMainGroupId);
                if (cachedUser != null) return cachedUser;
            }
            
            var result = await _bridge.GetProfile(userMainGroupId, cancellationToken);

            if (result.IsSuccess)
            {
                var user = result.Profile;
                AddToCache(user);
                return user;
            }

            if (result.IsRequestCanceled)
            {
                Debug.Log($"Downloading canceled for user with MainGroupId = {userMainGroupId}");
            }
            else
            {
                onFailed?.Invoke($"Failed to download user with  MainGroupId = {userMainGroupId}. Reason: {result.ErrorMessage}");
            }

            return null;
        }
        
        private Profile GetCachedUserByGroupId(long groupId)
        {
            return _cachedUsers.FirstOrDefault(us => us.MainGroupId == groupId);
        }

        private void AddToCache(Profile user)
        {
            var sameUser = _cachedUsers.FirstOrDefault(us => us.MainGroupId == user.MainGroupId);

            if (sameUser != null)
            {
                _cachedUsers.Remove(sameUser);
            }
            
            _cachedUsers.Add(user);
        }

        private void RemoveFromCache(Profile user)
        {
            if (user != null)
            {
                _cachedUsers.Remove(user);
            }
        }

        private void RemoveFromCache(long userId)
        {
            var user = _cachedUsers.FirstOrDefault(us => us.MainGroupId == userId);
            RemoveFromCache(user);
        }

        public async void UpdateMainCharacterIdForLocalUserOnServer(long characterId, long universeId)
        {
            var updateUserMainCharacterIdResult = await _bridge.UpdateUserMainCharacter(characterId, universeId);

            if (updateUserMainCharacterIdResult.IsSuccess)
            {
                RemoveFromCache(_bridge.Profile.Id);
            }
            
            if (updateUserMainCharacterIdResult.IsError)
            {
                Debug.LogError($"Failed to update {nameof(User.MainCharacterId)} to value \"{characterId}\" for the local {nameof(User)}. Reason: {updateUserMainCharacterIdResult.ErrorMessage}");
            }
        }
    } 
}

