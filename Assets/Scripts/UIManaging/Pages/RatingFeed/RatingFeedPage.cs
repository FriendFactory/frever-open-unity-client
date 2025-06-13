using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Pages.Common.VideoRating;
using UIManaging.Pages.RatingFeed;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.PublishPage.RatingFeedPage
{
    public class RatingFeedPage : GenericPage<RatingFeedPageArgs>
    {
        [SerializeField] private RatingFeedBoardingView _boardingView;
        [SerializeField] private RatingFeedView _ratingFeedView;

        private RatingFeedPresenter _ratingFeedPresenter;
        private RatingFeedViewModel _ratingFeedViewModel;
        private RatingFeedPageModel _ratingFeedPageModel;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject, UsedImplicitly]
        private void Construct(RatingFeedViewModel ratingFeedViewModel, RatingFeedPageModel pageModel, VideoRatingStatusModel videoRatingStatusModel)
        {
            _ratingFeedViewModel = ratingFeedViewModel;
            _ratingFeedPresenter = new RatingFeedPresenter(_ratingFeedViewModel.RatingFeedProgress, videoRatingStatusModel);
            _ratingFeedPageModel = pageModel;
        }

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public override PageId Id => PageId.RatingFeed;

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInit(PageManager pageManager)
        {
        }

        protected override async void OnDisplayStart(RatingFeedPageArgs args)
        {
            base.OnDisplayStart(args);
            
            _boardingView.Initialize(_ratingFeedViewModel);
            _boardingView.Show();
            
            var cts = new TaskCompletionSource<bool>();
            _boardingView.BoardingDone += () => cts.SetResult(true);
            
            await Task.WhenAll(_ratingFeedViewModel.InitializeAsync(args), cts.Task);
            
            _ratingFeedPageModel.OnRatingStarted();
            
            _ratingFeedPresenter.Initialize(_ratingFeedViewModel, _ratingFeedView);
            _ratingFeedView.Initialize(_ratingFeedViewModel);

            await _boardingView.FadeOutAsync();
            _boardingView.CleanUp();
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            base.OnHidingBegin(onComplete);
            
            if (_ratingFeedViewModel.IsInitialized) _ratingFeedViewModel.CleanUp();
            if (_ratingFeedView.IsInitialized) _ratingFeedView.CleanUp();
        }
    }
}