using System.Collections.Generic;

namespace UIManaging.Pages.RatingFeed.Rating
{
    internal sealed class RatingVideosListModel
    {
        public List<RatingVideoItemModel> ItemModels { get; }
        
        public RatingVideosListModel(IEnumerable<RatingVideo> videos)
        {
            ItemModels = new List<RatingVideoItemModel>();
            
            foreach (var video in videos)
            {
                ItemModels.Add(new RatingVideoItemModel(video));
            }
        }
    }
}