using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Extensions;
using UIManaging.Common.Args.Views.LastLevelsPanelArgs;
using UIManaging.Common.Loaders;
using UnityEngine;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;

namespace UIManaging.Pages.Feed.Remix.Loaders
{
    internal sealed class PaginatedFriendsCharactersLoader: GenericPaginationLoader<CharacterInfo>, IPaginatedCharactersLoader
    {
        private readonly IBridge _bridge;
        private readonly string _filter;
        private readonly long _universeId;

        public IList<DownloadedCharacterModel> CharacterModels { get; }
        
        public async Task DownloadNextPageAsync()
        {
            DownloadNextPage();
            
            while (AwaitingData)
            {
                await Task.Delay(25);
            }
        }

        protected override int DefaultPageSize { get; }
        
        public PaginatedFriendsCharactersLoader(IBridge bridge, int defaultPageSize, string filter, long universeId)
        {
            _bridge = bridge;
            _filter = filter;
            _universeId = universeId;
            CharacterModels = new List<DownloadedCharacterModel>();
            DefaultPageSize = defaultPageSize;
        }

        protected override void OnNextPageLoaded(CharacterInfo[] page)
        {
            var models = page.Select(character => new DownloadedCharacterModel(character, CharacterCollectionType.Friends));
            
            CharacterModels.AddRange(models);
        }

        protected override void OnFirstPageLoaded(CharacterInfo[] page)
        {
            var models = page.Select(character => new DownloadedCharacterModel(character, CharacterCollectionType.Friends));
            
            CharacterModels.AddRange(models);
        }

        protected override async Task<CharacterInfo[]> DownloadModelsInternal(object targetId, int takeNext, int takePrevious = 0, CancellationToken token = default)
        {
            var result = await _bridge.GetFriendsMainCharacters((long?)targetId, takeNext, takePrevious, _universeId, _filter, token: token);
            
            if (result.IsError)
            {
                Debug.LogError($"[{GetType().Name}] Failed to load characters # {result.ErrorMessage}");
                return null;
            }

            return result.Models;
        }
    }
}