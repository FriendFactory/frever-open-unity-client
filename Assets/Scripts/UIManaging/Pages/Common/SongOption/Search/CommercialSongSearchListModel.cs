using Bridge;

namespace UIManaging.Pages.Common.SongOption.Search
{
    internal sealed class CommercialSongSearchListModel: SongSearchListModel
    {
        protected override bool CommercialOnly => true;

        public CommercialSongSearchListModel(IMusicBridge bridge, int searchPageSize = 10) : base(bridge, searchPageSize)
        {
        }
    }
}