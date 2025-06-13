using Com.ForbiddenByte.OSA.Core;
using Com.ForbiddenByte.OSA.DataHelpers;
using Zenject;

namespace UIManaging.Pages.RatingFeed.Rating
{
    internal sealed class RatingVideosListAdapter: OSA<RatingVideosListParams, RatingVideosItemViewsHolder>
    {
        public SimpleDataHelper<RatingVideoItemModel> Data { get; private set; }

        [Inject] private RatingFeedProgress _ratingFeedProgress;

        private int _currentProgressIndex;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void PlayFirstItem()
        {
            var firstItem = GetItemViewsHolderIfVisible(0);
            firstItem?.View.Play();
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected override void Start()
        {
            Data = new SimpleDataHelper<RatingVideoItemModel>(this);
            
            _ratingFeedProgress.ProgressChanged += OnProgressChanged;
            _ratingFeedProgress.Completed += OnCompleted;
            
            base.Start();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            _ratingFeedProgress.ProgressChanged -= OnProgressChanged;
            _ratingFeedProgress.Completed -= OnCompleted;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override RatingVideosItemViewsHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new RatingVideosItemViewsHolder();
            instance.Init(Parameters.ItemPrefab, Parameters.Content, itemIndex);

            return instance;
        }

        protected override void UpdateViewsHolder(RatingVideosItemViewsHolder newOrRecycled)
        {
            var itemView = newOrRecycled.View;
            var itemIndex = newOrRecycled.ItemIndex;

            itemView.Initialize(Data[itemIndex]);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnProgressChanged(int currentProgressIndex)
        {
            if (currentProgressIndex > 0)
            {
                var previousItem = GetItemViewsHolderIfVisible(_currentProgressIndex);
                previousItem?.View.CleanUp();
            }

            _currentProgressIndex = currentProgressIndex;
			SmoothScrollTo(currentProgressIndex, .4f, 0.25f, 0.5f, onDone: OnScrollingDone);
        }

        private void OnScrollingDone()
        {
            var nextItem = GetItemViewsHolderIfVisible(_currentProgressIndex);
            nextItem?.View.Play();
        }

        private void OnCompleted()
        {
            GetItemViewsHolderIfVisible(_currentProgressIndex)?.View.CleanUp();
        }
    }
}