using System.Collections.Generic;

namespace UIManaging.Common.Carousel
{
    public interface ICarouselListModel<TItemModel> where TItemModel: CarouselItemModel
    {
        IReadOnlyList<TItemModel> Models { get; }
    }
}