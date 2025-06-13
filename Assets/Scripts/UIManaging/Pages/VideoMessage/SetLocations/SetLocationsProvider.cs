using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using JetBrains.Annotations;

namespace UIManaging.Pages.VideoMessage.SetLocations
{
    [UsedImplicitly]
    internal sealed class SetLocationsProvider
    {
        private const int PAGE_SIZE = 20;
        
        private readonly IBridge _bridge;
        private readonly List<SetLocationFullInfo> _cache = new List<SetLocationFullInfo>();
        private int _currentPageIndex;
        
        public bool AwaitingData { get; private set; }

        public SetLocationsProvider(IBridge bridge)
        {
            _bridge = bridge;
        }

        public Task<SetLocationFullInfo[]> GetFirstPage()
        {
            _currentPageIndex = 0;
            return GetNextPage();
        }
        
        public async Task<SetLocationFullInfo[]> GetNextPage()
        {
            if (AwaitingData)
            {
                await WaitWhileEndPreviousRequest();
            }
            var nextPage = await GetFromCacheOrDownload(_currentPageIndex);
            _currentPageIndex++;
            return nextPage;
        }

        private async Task WaitWhileEndPreviousRequest()
        {
            while (AwaitingData)
            {
                await Task.Delay(25);
            }
        }

        private async Task<SetLocationFullInfo[]> GetFromCacheOrDownload(int pageIndex)
        {
            var output = new List<SetLocationFullInfo>();
            output.AddRange(_cache.Skip(pageIndex * PAGE_SIZE));

            if (output.Count == PAGE_SIZE) return output.ToArray();

            AwaitingData = true;
            
            var leftToDownload = PAGE_SIZE - output.Count;
            var loadFromId = _cache.Any() ? _cache.Last().Id : null as long?;
            var resp = await _bridge.GetVideoMessageSetLocationListAsync(loadFromId, leftToDownload, 0, 1); //todo: provide race id from active race
            if (resp.IsError)
            {
                AwaitingData = false;
                return output.ToArray();
            }

            var excludedPreviousPageLastItem = resp.Models.Where(x => x.Id != loadFromId).ToArray();
            _cache.AddRange(excludedPreviousPageLastItem);
            output.AddRange(excludedPreviousPageLastItem);
            AwaitingData = false;
            return output.ToArray();
        }
    }
}