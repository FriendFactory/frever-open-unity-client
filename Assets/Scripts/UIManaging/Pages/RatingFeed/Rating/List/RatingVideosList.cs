using Common.Abstract;
using UnityEngine;

namespace UIManaging.Pages.RatingFeed.Rating
{
    internal sealed class RatingVideosListView: BaseContextPanel<RatingVideosListModel>
    {
        [SerializeField] private RatingVideosListAdapter _ratingVideosListAdapter;
        
        protected override void OnInitialized()
        {
            _ratingVideosListAdapter.Data.InsertItemsAtEnd(ContextData.ItemModels);
            _ratingVideosListAdapter.ScrollTo(0, 0.25f, 0.5f);
            _ratingVideosListAdapter.PlayFirstItem();
        }
    }
}