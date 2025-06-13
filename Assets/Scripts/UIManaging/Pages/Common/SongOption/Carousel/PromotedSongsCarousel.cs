using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UIManaging.Common.Carousel;
using UnityEngine;

namespace UIManaging.Pages.Common.SongOption.Carousel
{
    public class PromotedSongsCarousel : CarouselViewBase<PromotedSongsCarouselListModel,
        ClickableCarouselItem<PromotedSongCarouselItemModel>, PromotedSongCarouselItemModel>
    {
        public override float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return _carouselElementCellView.GetComponent<RectTransform>().rect.width;
        }
    }
    
    public class PromotedSongsCarouselListModel: ICarouselListModel<PromotedSongCarouselItemModel>
    {
        public IReadOnlyList<PromotedSongCarouselItemModel> Models { get; }
        
        public PromotedSongsCarouselListModel(IReadOnlyList<PromotedSongCarouselItemModel> models)
        {
            Models = models;
        }
    }
}