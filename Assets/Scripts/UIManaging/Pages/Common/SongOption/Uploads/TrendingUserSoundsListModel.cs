using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Extensions;
using UIManaging.Pages.Common.SongOption.Common;
using UIManaging.Pages.Common.SongOption.SongList;
using UnityEngine;

namespace UIManaging.Pages.Common.SongOption.Uploads
{
    internal sealed class TrendingUserSoundsListModel : UserSoundsListModelBase<PlayableTrendingUserSound, PlayableTrendingUserSoundModel>
    {
        public TrendingUserSoundsListModel(IBridge bridge) : base(bridge)
        {
        }

        protected override async Task<PlayableTrendingUserSound[]> DownloadModelsInternal(object targetId, int takeNext,
            int takePrevious = 0, CancellationToken token = default)
        {
            var skip = Mathf.Max(0, Models.Count - 1);
            var result = await Bridge.GetTrendingUserSoundsAsync(SearchQuery, takeNext, skip, token);

            if (result.IsError)
            {
                Debug.LogError($"[{GetType().Name}] Failed to get trending user sounds # {result.ErrorMessage}");
                return null;
            }

            if (result.IsRequestCanceled) return null;

            // TrendingUserSound doesn't implement IEntity interface, so, conversion needs to be applied here
            var models = result.Models.Select(model => new PlayableTrendingUserSound(model)).ToArray();

            return models;
        }

        protected override void AddItems(IEnumerable<PlayableTrendingUserSound> page)
        {
            var itemModels = page.Select(model => new PlayableTrendingUserSoundModel(model));

            ItemModels.AddRange(itemModels);
        }
    }
}