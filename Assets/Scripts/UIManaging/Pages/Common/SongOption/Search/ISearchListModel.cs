using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UIManaging.Pages.Common.SongOption.SongList;

namespace UIManaging.Pages.Common.SongOption.Search
{
    public interface ISearchListModel<out TModel> where TModel: PlayableItemModel
    {
        IReadOnlyList<TModel> Models { get; }

        bool AwaitingData { get; }
        bool FirstPageLoaded { get; }
        bool LastPageLoaded { get; }
        string SearchQuery { get; }

        event Action DataChanged; 
        event Action FetchFailed;

        Task SearchNextPageAsync(CancellationToken token);
    }
}