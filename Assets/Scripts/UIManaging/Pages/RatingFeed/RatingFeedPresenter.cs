using UIManaging.Common.Abstract;
using UIManaging.Pages.Common.VideoRating;

namespace UIManaging.Pages.RatingFeed
{
    internal sealed class RatingFeedPresenter: IGenericPresenter<RatingFeedViewModel, RatingFeedView>
    {
        private readonly RatingFeedProgress _ratingFeedProgress;
        private readonly VideoRatingStatusModel _videoRatingStatusModel;
        
        public bool IsInitialized { get; private set; }
        
        private RatingFeedViewModel _model;
        private RatingFeedView _view;
        
        public RatingFeedPresenter(RatingFeedProgress ratingFeedProgress, VideoRatingStatusModel videoRatingStatusModel)
        {
            _ratingFeedProgress = ratingFeedProgress;
            _videoRatingStatusModel = videoRatingStatusModel;
        }
        
        public void Initialize(RatingFeedViewModel model, RatingFeedView view)
        {
            _model = model;
            _view = view;

            _view.SkipRequested += OnSkipRequested;
            _view.MoveNextRequested += MoveNext;

            _ratingFeedProgress.Completed += OnRatingCompleted;

            _videoRatingStatusModel.LevelId = model.Level.Id;
            _videoRatingStatusModel.IsCompleted = false;
            
            IsInitialized = true;
        }

        public void CleanUp()
        {
            _view.SkipRequested -= OnSkipRequested;
            _view.MoveNextRequested -= MoveNext;
            
            _ratingFeedProgress.Completed -= MoveNext;
            
            IsInitialized = false;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnSkipRequested() => _model.SkipRatingRequested?.Invoke();
        private void OnRatingCompleted()
        {
            _videoRatingStatusModel.IsCompleted = true; 
            
            _view.ShowRewardPanel();
        }

        private void MoveNext() => _model?.MoveNextRequested?.Invoke();


    }
}