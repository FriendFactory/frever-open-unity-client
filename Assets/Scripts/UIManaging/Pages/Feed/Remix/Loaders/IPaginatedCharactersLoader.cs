using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UIManaging.Pages.Feed.Remix.Loaders
{
    internal interface IPaginatedCharactersLoader
    {
        IList<DownloadedCharacterModel> CharacterModels { get; }
        bool AwaitingData { get; }
        event Action NewPageAppended;
        event Action LastPageLoaded;
        Task DownloadNextPageAsync();
    }
}