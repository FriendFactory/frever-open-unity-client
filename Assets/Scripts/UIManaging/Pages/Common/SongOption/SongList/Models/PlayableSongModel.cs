using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;

namespace UIManaging.Pages.Common.SongOption.SongList
{
    public sealed class PlayableSongModel: PlayableItemModel
    {
        public override IPlayableMusic Music => SongInfo;
        public SongInfo SongInfo { get; }
        
        public override long Id => SongInfo.Id;
        public override string Title => SongInfo.Name;
        public override string ArtistName => SongInfo.Artist.Name;

        public PlayableSongModel(SongInfo songInfo)
        {
            SongInfo = songInfo;
        }
    }
}