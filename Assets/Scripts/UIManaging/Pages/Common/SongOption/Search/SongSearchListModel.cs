using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Bridge.Results;
using UIManaging.Pages.Common.SongOption.SongList;

namespace UIManaging.Pages.Common.SongOption.Search
{
    internal class SongSearchListModel: MusicSearchListModel<PlayableSongModel, SongInfo>
    {
        protected virtual bool CommercialOnly => false;
        
        public SongSearchListModel(IMusicBridge bridge, int searchPageSize = 10) : base(bridge, searchPageSize)
        {
        }

        protected override Task<ArrayResult<SongInfo>> LoadPage(string searchQuery = "", int takeNext = 10, int skip = 0, CancellationToken token = default)
        {
            return _bridge.GetSongsAsync(takeNext, skip, searchQuery, commercialOnly: CommercialOnly, token: token);
        }

        protected override Task<bool> ProcessPage(ArrayResult<SongInfo> page, CancellationToken token)
        {
            _models.AddRange(page.Models.Select(item => new PlayableSongModel(item)));
            return Task.FromResult(true);
        }
    }
}