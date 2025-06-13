using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abstract;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using Modules.CharacterManagement;
using UIManaging.Pages.Common.SongOption.SongList;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;
using Object = UnityEngine.Object;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.Common.SongOption.Uploads
{
        internal sealed class UserSoundsListModel : UserSoundsListModelBase<UserSoundFullInfo, PlayableUserSoundModel>, IAsyncInitializable
    {
        private readonly LocalUserDataHolder _localUserDataHolder;
        private readonly CharacterManager _characterManager;

        private Texture2D _ownerThumbnail;
        private List<UserSoundFullInfo> _localUserSounds;

        public bool IsInitialized { get; set; }

        public UserSoundsListModel(IBridge bridge, LocalUserDataHolder localUserDataHolder, CharacterManager characterManager): base(bridge)
        {
            _localUserDataHolder = localUserDataHolder;
            _characterManager = characterManager;
        }

        protected override async Task<UserSoundFullInfo[]> DownloadModelsInternal(object targetId, int takeNext,
            int takePrevious = 0, CancellationToken token = default)
        {
            var skip = Mathf.Max(0, Models.Count - 1);
            var result = await Bridge.GetUserSoundsAsync(takeNext, skip, token);

            if (result.IsError)
            {
                Debug.LogError($"[{GetType().Name}] Failed to get user sounds # {result.ErrorMessage}");
                return null;
            }

            return result.IsRequestCanceled ? null : result.Models;
        }

        protected override void AddItems(IEnumerable<UserSoundFullInfo> page)
        {
            ItemModels.AddRange(page.Where(model => !IsAddedLocally(model))
                                    .Select(model => new PlayableUserSoundModel(model, _ownerThumbnail)));
        }

        public async Task InitializeAsync(CancellationToken token)
        {
            _localUserSounds = new List<UserSoundFullInfo>();
            
            var character = _localUserDataHolder.UserProfile.MainCharacter;
            _ownerThumbnail = await _characterManager.GetCharacterThumbnail(character, Resolution._128x128, token);
            
            // do not interrupt init flow, because view list could be shown lately at least with empty thumbnails
            if (!_ownerThumbnail)
            {
                Debug.LogError($"[{GetType().Name}] Failed to download local user thumbnail");
            }

            if (token.IsCancellationRequested)
            {
                Object.Destroy(_ownerThumbnail);
            }

            IsInitialized = true;
        }

        public void CleanUp()
        {
            if (_ownerThumbnail)
            {
                Object.Destroy(_ownerThumbnail);
            }

            IsInitialized = false;
        }
        
        public void Insert(int index, UserSoundFullInfo userSound)
        {
            if (index < 0 || index > ItemModels.Count) return;
            
            if (IsAddedLocally(userSound)) return;

            _localUserSounds.Add(userSound);
            
            Models.Add(userSound);
            ItemModels.Insert(index, new PlayableUserSoundModel(userSound, _ownerThumbnail));
        }

        public void ReplaceOrAdd(UserSoundFullInfo userSound)
        {
            if (userSound == null)
            {
                throw new ArgumentNullException(nameof(userSound));
            }

            var itemIndex = ItemModels
                           .Select((item, index) => new { Item = item, Index = index })
                           .FirstOrDefault(x => x.Item.Id == userSound.Id)?.Index ?? -1;
            var model = new PlayableUserSoundModel(userSound, _ownerThumbnail);
            
            if (itemIndex >= 0)
            {
                ItemModels[itemIndex] = model;
            }
            else
            {
                ItemModels.Add(model);
            }
        }

        private bool IsAddedLocally(UserSoundFullInfo userSound) => !_localUserSounds.IsNullOrEmpty()
                                                                 && _localUserSounds.Any(sound => sound.Id == userSound.Id);
    }
}