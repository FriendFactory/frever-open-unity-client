using System;

namespace UIManaging.Common.Carousel
{
    public class CarouselItemModel
    {
        public Action OnItemClicked { get; set; }

        public CarouselItemModel(Action onItemClicked = null)
        {
            OnItemClicked = onItemClicked;
        }
    }
}