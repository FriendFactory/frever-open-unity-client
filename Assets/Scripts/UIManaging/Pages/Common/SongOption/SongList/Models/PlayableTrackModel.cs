using Bridge.Models.Common;
using Bridge.Services._7Digital.Models.TrackModels;

namespace UIManaging.Pages.Common.SongOption.SongList
{
    public sealed class PlayableTrackModel: PlayableItemModel
    {
        public ExternalTrackInfo TrackInfo { get; }
        
        public override long Id  => TrackInfo.Id;
        public override string Title => TrackInfo.Title;
        public override string ArtistName => TrackInfo.ArtistName;

        public override IPlayableMusic Music => TrackInfo;

        public PlayableTrackModel(ExternalTrackInfo trackInfo, bool isFavorite)
        {
            TrackInfo = trackInfo;
        }
    }
}