using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Results;
using Extensions;
using UnityEngine;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;

namespace UIManaging.Pages.Feed.Remix.Loaders
{
    internal sealed class PaginatedCharactersLoader: IPaginatedCharactersLoader
    {
        private static readonly int MAX_FAILS = 5;
        private static readonly int CATEGORIES_COUNT = Enum.GetNames(typeof(CharacterCollectionType)).Length;
        
        private readonly IBridge _bridge;
        private readonly long _universeId;
        
        //---------------------------------------------------------------------
        // Properties 
        //---------------------------------------------------------------------

        public IList<DownloadedCharacterModel> CharacterModels { get; }
        public bool AwaitingData { get; private set; }
        
        private int DefaultPageSize { get; }
        private int PageSize => LastLoadedItemId != null ? DefaultPageSize + 1 : DefaultPageSize;
        private long? LastLoadedItemId { get; set; }
        private int CurrentCategoryIndex { get; set; }
        
        //---------------------------------------------------------------------
        // Events 
        //---------------------------------------------------------------------
        
        public event Action NewPageAppended;
        public event Action LastPageLoaded;

        //---------------------------------------------------------------------
        //  Ctors
        //---------------------------------------------------------------------

        public PaginatedCharactersLoader(IBridge bridge, long universeId, int pageSize = 10)
        {
            DefaultPageSize = pageSize;
            _bridge = bridge;
            CharacterModels = new List<DownloadedCharacterModel>();
            CurrentCategoryIndex = 0;
            _universeId = universeId;
        }
        
        //---------------------------------------------------------------------
        // Public 
        //---------------------------------------------------------------------
        
        public async Task DownloadNextPageAsync()
        {
            if (CurrentCategoryIndex >= CATEGORIES_COUNT) return;
            
            AwaitingData = true;

            var models = await DownloadNextPageInternal();
            if (models == null)
            {
                return;
            }
            
            CharacterModels.AddRange(models);
            NewPageAppended?.Invoke();
            
            if (CurrentCategoryIndex >= CATEGORIES_COUNT)
            {
                LastPageLoaded?.Invoke();
            }
            
            AwaitingData = false;
        }
        
        //---------------------------------------------------------------------
        // Helpers 
        //---------------------------------------------------------------------
        
        private async Task<DownloadedCharacterModel[]> DownloadNextPageInternal()
        {
            var models = new List<DownloadedCharacterModel>();
            var nextChunkSize = PageSize;
            var failedAttempts= 0;
            
            while (models.Count < DefaultPageSize && CurrentCategoryIndex < CATEGORIES_COUNT && failedAttempts < MAX_FAILS)
            {
                var category = (CharacterCollectionType)CurrentCategoryIndex;
                var characters = await DownloadModelsInternal(category, LastLoadedItemId, nextChunkSize, 0, _universeId);
                if (characters == null)
                {
                    failedAttempts++;
                    continue;
                }
                    
                characters = LastLoadedItemId != null ? characters.Skip(1).ToArray() : characters;
                var loadedModels =
                    characters.Select(characterInfo => new DownloadedCharacterModel(characterInfo, category));
                
                models.AddRange(loadedModels);
               
                var targetLength = LastLoadedItemId != null ? nextChunkSize - 1 : nextChunkSize;
                LastLoadedItemId = models.Count > 0 ? models[models.Count - 1].Id : (long?)null;
                    
                if (characters.Length == 0 || characters.Length < targetLength)
                {
                    CurrentCategoryIndex++;
                    LastLoadedItemId = null;
                }
                
                nextChunkSize = Mathf.Max(0, PageSize - models.Count);
            }

            return models.ToArray();
        }

        private async Task<CharacterInfo[]> DownloadModelsInternal(CharacterCollectionType collectionType, long? targetId, int takeNext, int takePrevious, long universeId, CancellationToken token = default)
        {
            ArrayResult<CharacterInfo> result;
            switch (collectionType)
            {
                case CharacterCollectionType.MyCharacters:
                    result = await _bridge.GetMyCharacters(targetId, takeNext, takePrevious, universeId, token: token);
                    break;
                case CharacterCollectionType.Friends:
                    result = await _bridge.GetFriendsMainCharacters(targetId, takeNext, takePrevious, universeId, token: token);
                    break;
                case CharacterCollectionType.FreverStars:
                    result = await _bridge.GetStarCharacters(targetId, takeNext, takePrevious, universeId, token: token);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(collectionType), collectionType, null);
            }
            
            if (result.IsError)
            {
                Debug.LogError($"[{GetType().Name}] Failed to load characters # {result.ErrorMessage}");
                return null;
            }

            return result.Models;
        }
    }
}