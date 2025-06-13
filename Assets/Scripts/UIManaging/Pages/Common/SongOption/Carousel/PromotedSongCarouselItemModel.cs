using Bridge.Models.Common;
using Bridge.Services.Advertising;
using UIManaging.Common.Carousel;

namespace UIManaging.Pages.Common.SongOption.Carousel
{
    public class PromotedSongCarouselItemModel: CarouselItemModel
    {
        public IPlayableMusic Playable { get;}
        public PromotedSong PromotedSong { get; }

        public PromotedSongCarouselItemModel(IPlayableMusic playable, PromotedSong promotedSong)
        {
            Playable = playable;
            PromotedSong = promotedSong;
        }
    }
}