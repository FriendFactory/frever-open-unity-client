using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.Common.Files;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.StartPack.UserAssets;
using Bridge.Services.UserProfile;
using Configs;
using Extensions;
using Modules.Amplitude;
using JetBrains.Annotations;
using ModestTree;
using Modules.AssetsStoraging.Core;
using UIManaging.Pages.Common.FollowersManagement;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.Common.UsersManagement.BlockedUsersManaging;
using UMA;
using UnityEngine;
using Zenject;
using FileInfo = Bridge.Models.Common.Files.FileInfo;
using Resolution = Bridge.Models.Common.Files.Resolution;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;

namespace Modules.CharacterManagement
{
    [UsedImplicitly]
    public sealed class CharacterManager
    {
        public event Action<CharacterInfo> CharacterAdded;
        public event Action<CharacterInfo> CharacterDeleted;
        public event Action<CharacterInfo> CharacterSelected;
        public event Action<CharacterInfo> CharacterUpdated;
        public event Action CharactersUpdated;
        public event Action CharactersDataFetched;

        [Inject] private readonly IBridge _bridge;
        [Inject] private readonly CharacterManagerConfig _characterManagerConfig;
        [Inject] private readonly AmplitudeManager _amplitudeManager;
        [Inject] private readonly LocalUserDataHolder _localUserDataHolder;
        [Inject] private readonly UsersManager _usersManager;

        private readonly Dictionary<long, CharacterInfo> _characters = new();
        private readonly IDataFetcher _dataFetcher;
        private readonly FollowersManager _followersManager;
        private readonly IBlockedAccountsManager _blockedAccountsManager;

        private Dictionary<long, CharacterInfo[]>  _cachedStyleCharacters = new();
        private List<CharacterInfo> _cachedFriendsCharacters;
        private readonly Dictionary<long, CharacterFullInfo> _cachedCharacterFullInfos = new();
        private bool _isFetchingStyles;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public CharacterInfo SelectedCharacter 
        {
            get
            {
                if (!_characters.ContainsKey(SelectedCharacterId))
                {
                    SelectedCharacterId = 0;
                    return null;
                }
                
                return _characters[SelectedCharacterId];
            }
            private set => SelectedCharacterId = value.Id;
        }

        public long SelectedCharacterId { get; private set; }
        public Dictionary<long, long> RaceMainCharacters = new();
        
        public int MaxCharactersCount => _characterManagerConfig.MaxCharactersCount;
        public string DefaultMaleRecipe => GetBaseRecipe(_characterManagerConfig.DefaultMaleRecipeName);
        public string DefaultFemaleRecipe => GetBaseRecipe(_characterManagerConfig.DefaultFemaleRecipeName);
        public string DefaultNonBinaryRecipe => GetBaseRecipe(_characterManagerConfig.DefaultNonBinaryRecipeName);
        public CharacterInfo[] UserCharacters { get; private set; }
        public bool IsDataFetched { get; private set; }

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        public CharacterManager(IDataFetcher dataFetcher, FollowersManager followersManager, IBlockedAccountsManager blockedAccountsManager)
        {
            IsDataFetched = false;
            _dataFetcher = dataFetcher;
            _dataFetcher.OnUserAssetsFetched += SetupUserCharacters;
            if (_dataFetcher.DefaultUserAssets != null)
            {
                SetupUserCharacters(_dataFetcher.DefaultUserAssets);
            }
            
            _followersManager = followersManager;
            _followersManager.LocalUserFollowed.OnAddedFriend += AddNewFriendCharacterToCache;
            _followersManager.LocalUserFollowed.OnRemovedFriend += RemoveFriendCharacterFromCache;
            
            _blockedAccountsManager = blockedAccountsManager;
            _blockedAccountsManager.AccountBlocked += OnAccountBlocked;

            CharactersDataFetched += () => IsDataFetched = true;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public async Task<CharacterFullInfo> CreateCharacter(KeyValuePair<string, List<WardrobeFullInfo>> recipeAndWardrobes, KeyValuePair<Resolution, Texture2D>[] thumbnails, long genderId) 
        {
            var character = new CharacterSaveModel
            {
                Name = string.Empty,
                GroupId = _bridge.Profile.GroupId,
                GenderId = genderId,
                CharacterStyleId = 1,
                UniverseId = _dataFetcher.MetadataStartPack.GetUniverseByGenderId(genderId).Id,
                Files = ConvertToFileInfos(thumbnails)
            };
            
            FillUmaRecipeAndWardrobes(character, recipeAndWardrobes);
            
            return await CreateCharacter(character);
        }

        public Task<CharacterFullInfo> UpdateCharacter(CharacterFullInfo character, KeyValuePair<string, List<WardrobeFullInfo>> recipeAndWardrobes, long genderId, KeyValuePair<Resolution, Texture2D>[] thumbnails)
        {
            var modifiedCharacter = new CharacterSaveModel
            {
                Id = character.Id,
                CharacterStyleId = character.CharacterStyleId,
                GroupId = character.GroupId,
                Name = character.Name,
                GenderId = genderId,
                UniverseId = _dataFetcher.MetadataStartPack.GetUniverseByGenderId(genderId).Id,
                Files = ConvertToFileInfos(thumbnails, character.Files)
            };
            FillUmaRecipeAndWardrobes(modifiedCharacter, recipeAndWardrobes);
            
            return UpdateCharacterInternal(modifiedCharacter);
        }

        public void SelectCharacter(CharacterInfo character) 
        {
            if (character == null) return;
            SelectedCharacter = character;
            var race = _dataFetcher.MetadataStartPack.GetRaceByGenderId(character.GenderId);
            RaceMainCharacters[race.Id] = character.Id;
            CharacterSelected?.Invoke(character);
        }

        public void SetCharacterSilent(long characterId)
        {
            if (!_characters.ContainsKey(characterId))
            {
                SelectedCharacterId = characterId;
                return;
            }
            SelectedCharacter = _characters[characterId];
        }

        public void ResetSelectedCharacterId()
        {
            SelectedCharacterId = 0;
        }

        public bool HasCachedThumbnail(CharacterInfo characterInfo, FileInfo fileInfo) =>
            _bridge.HasCached(characterInfo, fileInfo);
        
        public async void FetchCharacterStyles()
        {
            if(_cachedStyleCharacters != null) return;
            _isFetchingStyles = true;
            var universes = _dataFetcher.MetadataStartPack.Universes;
            var tasks = new Task[universes.Length];
            for (var i = 0; i < tasks.Length; i++)
            {
                tasks[0] = DownloadOrGetFromCacheStyleCharacters(universes[i].Id);
            }
            await Task.WhenAll(tasks);
            _isFetchingStyles = false;
        }
        
        public async Task<CharacterInfo[]> GetCharacterStyles(long universeId)
        {
            if (_isFetchingStyles)
            {
                await Task.Delay(30);
            }
            return await DownloadOrGetFromCacheStyleCharacters(universeId);
        }

        public async Task<CharacterFullInfo> FetchCharacterAsync(long id, CancellationToken token = default)
        {
            var result = await _bridge.GetCharacter(id, token);
            if (!result.IsSuccess)
            {
                Debug.LogError($"Error on getting character full info = {result.ErrorMessage}");
                return null;
            }

            _cachedCharacterFullInfos[id] = result.Model;
            if (!_characters.ContainsKey(id))
            {
                _characters[id] = result.Model.ToCharacterInfo();
            }
            return result.Model;
        }
        
        public Task<CharacterFullInfo> GetCharacterAsync(long id, CancellationToken token = default)
        {
            if (_cachedCharacterFullInfos.TryGetValue(id, out var fullInfo) && fullInfo != null)
            {
                return Task.FromResult(fullInfo);
            }

            return FetchCharacterAsync(id, token);
        }

        public Task<CharacterFullInfo> GetSelectedCharacterFullInfo(CancellationToken token = default)
        {
            return GetCharacterAsync(SelectedCharacter.Id, token);
        }

        public async Task<CharacterFullInfo[]> GetCharacterFullInfos(long[] ids, CancellationToken token = default)
        {
            var result = await _bridge.GetCharacters(ids, token);

            if (!result.IsSuccess)
            {
                Debug.LogError($"{result.ErrorMessage}");
                return null;
            }

            return result.Models;
        }
        
        public void RefreshCache(CharacterFullInfo updatedCharacter)
        {
            var characterInfo = updatedCharacter.ToCharacterInfo();
            _characters[updatedCharacter.Id] = characterInfo;

            var cached = UserCharacters.First(x => x.Id == updatedCharacter.Id);
            characterInfo.IsNew = cached.IsNew;
            var index = UserCharacters.IndexOf(cached);
            UserCharacters[index] = characterInfo;
            _cachedCharacterFullInfos[updatedCharacter.Id] = updatedCharacter;
        }
        
        public bool MaxCountReached(long raceId)
        {
            var raceGenders = _dataFetcher.MetadataStartPack.GetRace(raceId).Genders.Select(x => x.Id).ToArray();
            return UserCharacters.Count(x => raceGenders.Contains(x.GenderId)) >= _characterManagerConfig.MaxCharactersCount;
        }

        #region CLIENT-SERVER
        private async Task<CharacterFullInfo> CreateCharacter(CharacterSaveModel character)
        {
            var resp = await _bridge.SaveCharacter(character);
            if (!resp.IsSuccess)
            {
                Debug.LogError(resp.ErrorMessage);
                return null;
            }
            
            OnCharacterCreated(resp.Model);
            return resp.Model;
        }
        
        private async Task<CharacterFullInfo> UpdateCharacterInternal(CharacterSaveModel modifiedCharacter)
        {
            var result = await _bridge.SaveCharacter(modifiedCharacter);
            if (!result.IsSuccess)
            {
                Debug.LogError("One of your assets you are currently wearing have been depublished. Please create a new character or change assets to be able to save.");
                return null;
            }

            RefreshCache(result.Model);
            
            var characterUpdatedMetaData = new Dictionary<string, object>
            {
                {AmplitudeEventConstants.EventProperties.CHARACTER_ID, modifiedCharacter.Id}
            };
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.CHARACTER_UPDATED, characterUpdatedMetaData);
            
            CharacterUpdated?.Invoke(result.Model.ToCharacterInfo());
            return result.Model;
        }

        public async Task<bool> DeleteCharacter(CharacterInfo character)
        {
            var result = await _bridge.DeleteCharacter(character.Id);
            if (result.IsSuccess)
            {
                OnCharacterDeleted(character);
                return true;
            }

            Debug.LogError(result.ErrorMessage);
            return false;
        }

        public async void SetNameForCharacter(CharacterInfo character, string name, Action onSuccess = null)
        {
            var result = await _bridge.UpdateCharacterName(character.Id, name);

            if (result.IsSuccess)
            {
                character.Name = name;
                onSuccess?.Invoke();
            }

            if (result.IsError)
            {
                Debug.LogError($"Failed to update {nameof(CharacterInfo.Name)} to value \"{name}\" for the {nameof(CharacterInfo)} with {nameof(CharacterInfo.Id)}={character.Id}. Reason: {result.ErrorMessage}");
            }
        }
        
        public async Task<Texture2D> GetCharacterThumbnail(CharacterInfo character, Resolution resolution, CancellationToken cancellationToken = default) // TODO: Refactor thumbnail request - https://friendfactory.atlassian.net/browse/FREV-9514
        {
            if (_bridge.HasCached(character, character.Files.First(r => r.Resolution == resolution)))
            {
                var res = _bridge.GetThumbnailFromCacheImmediate(character, resolution);
                if (res.IsSuccess)
                {
                    return res.Model;
                }
            }
            var result = await _bridge.GetThumbnailAsync(character, resolution, true, cancellationToken);

            if (result.IsSuccess)
            {
                return (Texture2D)result.Object;
            }
            else
            {
                Debug.LogWarning($"Failed to download thumbnail of {nameof(CharacterInfo)} with {nameof(CharacterInfo.Id)}={character.Id} with resolution {resolution}. Reason: {result.ErrorMessage}");
                return null;
            }
        }

        public bool IsMainCharacter(long characterId)
        {
            return RaceMainCharacters.Values.Contains(characterId);
        }
        
        #endregion

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private static void FillUmaRecipeAndWardrobes(CharacterSaveModel character, KeyValuePair<string, List<WardrobeFullInfo>> recipeAndWardrobes)
        {
            character.WardrobeIds = recipeAndWardrobes.Value.Select(x=>x.Id).ToArray();
            var umaRecipe = character.UmaRecipe;
            if (umaRecipe == null)
            {
                character.UmaRecipe = new UmaRecipeFullInfo()
                {
                    J = Encoding.ASCII.GetBytes(recipeAndWardrobes.Key)
                };
            }
            else
            {
                umaRecipe.J = Encoding.ASCII.GetBytes(recipeAndWardrobes.Key);
            }
        }

        private List<FileInfo> ConvertToFileInfos(KeyValuePair<Resolution ,Texture2D>[] thumbnails, List<FileInfo> sourceFiles = null)
        {
            var output = sourceFiles ?? new List<FileInfo>();
            
            foreach (var thumbnail in thumbnails)
            {
                AddOrReplaceThumbnail(thumbnail);
            }
            
            void AddOrReplaceThumbnail(KeyValuePair<Resolution, Texture2D> thumbnail)
            {
                var index = output.FindIndex(fileInfo => fileInfo.Resolution == thumbnail.Key);
                var thumbnailInfo = new FileInfo(thumbnail.Value, FileExtension.Png, thumbnail.Key);
                if (index == -1)
                {
                    output.Add(thumbnailInfo);
                }
                else
                {
                    output[index] = thumbnailInfo;
                }
            }
            
            return output;
        }

        private void OnCharacterCreated(CharacterFullInfo character) 
        {
            var characterInfo = character.ToCharacterInfo();
            characterInfo.IsNew = true;
            if (SelectedCharacterId == 0) SelectCharacter(characterInfo);
            character.GroupId = _bridge.Profile.GroupId;
            AddNewUserCharacterCache(characterInfo);
            
            var characterSavedMetaData = new Dictionary<string, object>
            {
                {AmplitudeEventConstants.EventProperties.CHARACTER_ID, character.Id}
            };
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.CHARACTER_SAVED, characterSavedMetaData);
            SetAsMainForRaceIfMissed(character);
            SetAsMainIfNeeded(character);
            CharacterAdded?.Invoke(characterInfo);
        }

        private void OnCharacterDeleted(CharacterInfo character)
        {
            RemoveUserCharacterFromCache(character);
            CharacterDeleted?.Invoke(character);
        }

        private void SetupUserCharacters(DefaultUserAssets userAssets)
        {
            _characters.Clear();
            UserCharacters = GetInitialUserCharacters();
            foreach (var character in UserCharacters)
            {
                _characters.Add(character.Id, character);
            }

            if (_dataFetcher.IsStartPackFetched)
            {
                SetupMainCharacters();
            }
            else
            {
                _dataFetcher.OnStartPackFetched += SetupMainCharacters;
            }
            
            CharactersUpdated?.Invoke();

            void SetupMainCharacters()
            {
                _dataFetcher.OnStartPackFetched -= SetupMainCharacters;
                if(_dataFetcher.DefaultUserAssets.MainCharacterIds == null) return;
                foreach (var characterId in _dataFetcher.DefaultUserAssets.MainCharacterIds)
                {
                    var characterModel = UserCharacters.First(x => x.Id == characterId);
                    var race = _dataFetcher.MetadataStartPack.GetRaceByGenderId(characterModel.GenderId);
                    if (race is not null)
                    {
                        RaceMainCharacters[race.Id] = characterId;
                    }
                }
                CharactersDataFetched?.Invoke();
            }
        }

        private string GetBaseRecipe(string filename)
        {
            var recipe = "";
            var textRecipe = UMAContext.Instance.dynamicCharacterSystem.GetBaseRecipe(filename) as UMATextRecipe;
            if (textRecipe != null)
                recipe = textRecipe.recipeString;
            return recipe;
        }

        private async Task<CharacterInfo[]> DownloadOrGetFromCacheStyleCharacters(long universeId)
        {
            if (_cachedStyleCharacters.TryGetValue(universeId, out var styles))
            {
                return styles;
            }

            var resp = await _bridge.GetStyleCharacters(null, 50, 0, universeId);
            if (!resp.IsSuccess)
            {
                Debug.LogError(resp.ErrorMessage);
            }
            
            _cachedStyleCharacters[universeId] = resp.Models;
            return resp.Models;
        }

        private void AddNewUserCharacterCache(CharacterInfo character)
        {
            _characters.Add(character.Id, character);
            UserCharacters = UserCharacters.Concat(new []{character}).ToArray();
        }
        
        private void RemoveUserCharacterFromCache(CharacterInfo character)
        {
            _characters.Remove(character.Id);
            UserCharacters = UserCharacters.Where(x => x.Id != character.Id).ToArray();
        }

        private CharacterInfo[] GetInitialUserCharacters()
        {
            return _dataFetcher.DefaultUserAssets.UserCharacters == null ? Array.Empty<CharacterInfo>() : _dataFetcher.DefaultUserAssets.UserCharacters.ToArray();
        }

        private void RemoveFriendCharacterFromCache(Profile profile)
        {
            var characterToRemove = _cachedFriendsCharacters?.FirstOrDefault(x => x.Id == profile.MainCharacter.Id);
            _cachedFriendsCharacters?.Remove(characterToRemove);
        }
        
        private void AddNewFriendCharacterToCache(Profile profile)
        {
            _cachedFriendsCharacters?.Add(profile.MainCharacter);
        }

        private void OnAccountBlocked(Profile profile)
        {
            RemoveFriendCharacterFromCache(profile);
        }
        
        private void SetAsMainForRaceIfMissed(CharacterFullInfo character)
        {
            var raceId = _dataFetcher.MetadataStartPack.GetRaceByGenderId(character.GenderId).Id;
            if (RaceMainCharacters.ContainsKey(raceId)) return;

            RaceMainCharacters[raceId] = character.Id;
        }

        private void SetAsMainIfNeeded(CharacterFullInfo newCharacter)
        {
            var currentCharacter = SelectedCharacter;
            var raceId = _dataFetcher.MetadataStartPack.GetRaceByGenderId(currentCharacter.GenderId)?.Id;
            var newUniverse = _dataFetcher.MetadataStartPack.GetUniverseByGenderId(newCharacter.GenderId);
            if (!raceId.HasValue)
            {
                SetNewAsMain();
                return;
            }
            var settings = _dataFetcher.MetadataStartPack.GetUniverseByGenderId(currentCharacter.GenderId)
                .Races.Where(r => r.RaceId == raceId)
                .Select(x => x.Settings).First();

            if (!settings.CanUseCharacters)
            {
                SetNewAsMain();
            }

            void SetNewAsMain()
            {
                SelectedCharacter = _characters[newCharacter.Id];
                _localUserDataHolder.SetMainCharacter(SelectedCharacter);
                _usersManager.UpdateMainCharacterIdForLocalUserOnServer(newCharacter.Id, newUniverse.Id);
            }
        }
    }
}