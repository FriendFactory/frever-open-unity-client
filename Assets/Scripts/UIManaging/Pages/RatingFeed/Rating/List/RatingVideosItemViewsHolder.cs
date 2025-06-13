using Com.ForbiddenByte.OSA.Core;

namespace UIManaging.Pages.RatingFeed.Rating
{
    internal class RatingVideosItemViewsHolder : BaseItemViewsHolder
    {
        private RatingVideoItemView _ratingVideoItemView;

        internal RatingVideoItemView View => _ratingVideoItemView;

        public override void CollectViews()
        {
            base.CollectViews();
            
            _ratingVideoItemView = root.GetComponent<RatingVideoItemView>();
        }
    }
}