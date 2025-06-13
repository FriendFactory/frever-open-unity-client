using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Modules.CharacterManagement;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;
using Object = UnityEngine.Object;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Common.Args.Views.Profile
{
    [UsedImplicitly]
    internal sealed class CharacterThumbnailProvider
    {
        private readonly CharacterManager _characterManager;
        private readonly UsersManager _usersManager;
        
        private readonly Dictionary<long, Texture2D> _characterIdToTexture = new Dictionary<long, Texture2D>();
        private readonly Dictionary<long, long> _usageCounter = new Dictionary<long, long>();
        private readonly List<long> _loadingProcesses = new List<long>();

        public CharacterThumbnailProvider(CharacterManager characterManager, UsersManager usersManager)
        {
            _characterManager = characterManager;
            _usersManager = usersManager;
        }

        public async Task<Texture2D> GetCharacterThumbnail(CharacterInfo characterInfo, Resolution resolution)
        {
            if (!_usageCounter.ContainsKey(characterInfo.Id))
            {
                _usageCounter[characterInfo.Id] = 0;
            }
            _usageCounter[characterInfo.Id]++;
            while (_loadingProcesses.Contains(characterInfo.Id))
            {
                await Task.Delay(20);
            }
            
            if (_characterIdToTexture.ContainsKey(characterInfo.Id)) return _characterIdToTexture[characterInfo.Id];
            
            _loadingProcesses.Add(characterInfo.Id);
            var texture = await _characterManager.GetCharacterThumbnail(characterInfo, resolution);
            _loadingProcesses.Remove(characterInfo.Id);
            if (texture != null)
            {
                _characterIdToTexture[characterInfo.Id] = texture;
            }

            return texture;
        }

        public async void GetThumbnailByUserGroupId(long groupId, Resolution resolution, Action<Texture2D> onSuccess, Action onFailed)
        {
            var profile = await _usersManager.DownloadRemoteUser(groupId, false);
            var texture = await GetCharacterThumbnail(profile.MainCharacter, resolution);
            if (texture != null)
            {
                onSuccess?.Invoke(texture);
            }
            else
            {
                onFailed?.Invoke();
            }
        }

        public void ReleaseIfNotUsed(long characterId)
        {
            if (!_usageCounter.ContainsKey(characterId)) return;
            _usageCounter[characterId]--;
            if (_usageCounter[characterId] != 0) return;
            _usageCounter.Remove(characterId);
            if (!_characterIdToTexture.ContainsKey(characterId)) return;
            Object.Destroy(_characterIdToTexture[characterId]);
            _characterIdToTexture.Remove(characterId);
        }

        public void ReleaseIfNotUsed(Texture2D texture2D)
        {
            var characterAndTexture = _characterIdToTexture.First(x => x.Value == texture2D);
            ReleaseIfNotUsed(characterAndTexture.Key);
        }

        public void ClearCache()
        {
            foreach (var idAndTexture in _characterIdToTexture)
            {
                Object.Destroy(idAndTexture.Value);
            }
            _characterIdToTexture.Clear();
            _usageCounter.Clear();
        }
    }
}
