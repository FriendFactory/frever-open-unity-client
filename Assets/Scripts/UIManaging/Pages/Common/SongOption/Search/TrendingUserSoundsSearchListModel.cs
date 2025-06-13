using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Bridge.Results;
using UIManaging.Pages.Common.SongOption.Common;
using UIManaging.Pages.Common.SongOption.SongList;

namespace UIManaging.Pages.Common.SongOption.Search
{
    public class TrendingUserSoundsSearchListModel : MusicSearchListModel<PlayableTrendingUserSoundModel, TrendingUserSound>
    {
        public TrendingUserSoundsSearchListModel(IMusicBridge bridge, int searchPageSize = 10) : base(bridge, searchPageSize)
        {
        }

        protected override Task<ArrayResult<TrendingUserSound>> LoadPage(string searchQuery = "", int takeNext = 10, int skip = 0, CancellationToken token = default)
        {
            return _bridge.GetTrendingUserSoundsAsync(searchQuery, takeNext, skip, token: token);
        }

        protected override Task<bool> ProcessPage(ArrayResult<TrendingUserSound> page, CancellationToken token)
        {
            _models.AddRange(page.Models.Select(sound => new PlayableTrendingUserSoundModel(new PlayableTrendingUserSound(sound))));
            return Task.FromResult(true);
        }
    }
}