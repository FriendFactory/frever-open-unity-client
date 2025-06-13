using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bridge.Models.Common.Files;
using Bridge.Services.UserProfile;
using Modules.CharacterManagement;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;
using Object = UnityEngine.Object;
using Resolution = Bridge.Models.Common.Files.Resolution;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;

namespace UIManaging.Pages.Common.Helpers
{
    public sealed class CharacterThumbnailsDownloader
    {
        private readonly CharacterManager _umaCharactersManager;
        private readonly UsersManager _usersManager;
        private readonly Dictionary<ThumbnailMeta, Texture2D> _cachedThumbnails = new Dictionary<ThumbnailMeta, Texture2D>();

        public CharacterThumbnailsDownloader(CharacterManager umaCharactersManager, UsersManager usersManager)
        {
            _umaCharactersManager = umaCharactersManager;
            _usersManager = usersManager;
        }

        public void ClearCachedCharacterThumbnails()
        {
            foreach (var thumbnail in _cachedThumbnails.Values)
            {
                Object.Destroy(thumbnail);
            }
            
            _cachedThumbnails.Clear();
        }
        
        public async Task GetCachedCharacterThumbnail(CharacterInfo character, Resolution resolution,
            Action<Texture2D> onSuccess = null, Action<string> onFailure = null, CancellationToken cancellationToken = default)
        {
            
            var thumbnailMeta = new ThumbnailMeta { ID = character.Id, Resolution = resolution };
            if (_cachedThumbnails.ContainsKey(thumbnailMeta))
            {
                await Task.Yield(); // Ensure the method is always asynchronous
                onSuccess?.Invoke(_cachedThumbnails[thumbnailMeta]);
                return;
            }
            
            await GetCharacterThumbnail(character, resolution, thumbnail =>
            {
                _cachedThumbnails[thumbnailMeta] = thumbnail;
                onSuccess?.Invoke(thumbnail);
            }, onFailure, cancellationToken);
        }
        
        public async Task GetCharacterThumbnail(CharacterInfo character, Resolution resolution,
                                                Action<Texture2D> onSuccess = null, Action<string> onFailure = null, CancellationToken cancellationToken = default)
        {
            var thumbnail = await _umaCharactersManager.GetCharacterThumbnail(character, resolution, cancellationToken);
            HandleThumbnailResponse(character.Id, resolution, onSuccess, onFailure, cancellationToken, thumbnail);
        }

        public bool HasCachedThumbnail(CharacterInfo characterInfo, FileInfo fileInfo) =>
            _umaCharactersManager is {} ? _umaCharactersManager.HasCachedThumbnail(characterInfo, fileInfo) : false;

        private void HandleThumbnailResponse(
            long characterId,
            Resolution resolution,
            Action<Texture2D> onSuccess,
            Action<string> onFailure,
            CancellationToken cancellationToken,
            Texture2D thumbnail)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                Object.Destroy(thumbnail);
                return;
            }

            if (thumbnail == null)
            {
                onFailure?.Invoke($"$Failed to download thumbnail for character {characterId}");
                return;
            }

            if (onSuccess != null)
            {
                var thumbnailMeta = new ThumbnailMeta { ID = characterId, Resolution = resolution };
                _cachedThumbnails[thumbnailMeta] = thumbnail;
                onSuccess(thumbnail);
            }
            else
            {
                Object.Destroy(thumbnail);
            }
        }

        public async void GetCharacterThumbnailByUserGroupId(long userGroupId, Resolution resolution, 
                                                             Action<Texture2D> onSuccess = null, Action<string> onFailure = null, CancellationToken cancellationToken = default)
        {
             var profile = await _usersManager.DownloadRemoteUser(userGroupId, false, onFailure, cancellationToken);
             GetCharacterThumbnailByProfile(profile, resolution, onSuccess, onFailure, cancellationToken);
        }

        public async void GetCharacterThumbnailByProfile(Profile profile, Resolution resolution, Action<Texture2D> onSuccess = null, 
                                                         Action<string> onFailure = null, CancellationToken cancellationToken = default)
        {
            if (profile?.MainCharacter?.Id == null) return;
            
            await GetCharacterThumbnail(profile.MainCharacter, resolution, onSuccess, onFailure, cancellationToken);
        }
    }
}


