using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge;
using JetBrains.Annotations;
using Models;
using Navigation.Args;
using UIManaging.Pages.RatingFeed.Amplitude;
using UIManaging.Pages.RatingFeed.Rating;
using UIManaging.SnackBarSystem;
using Zenject;
using static UIManaging.Pages.RatingFeed.VideosForRatingListProvider.StatusCode;

namespace UIManaging.Pages.RatingFeed
{
    [UsedImplicitly]
    internal sealed class RatingFeedViewModel
    {
        [Inject] private SnackBarHelper _snackBarHelper;

        private readonly VideosForRatingListProvider _videosForRatingListProvider;

        //--------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public bool IsInitialized { get; private set; }

        public Level Level { get; private set; }
        public List<RatingVideo> RatingVideos { get; private set; }
        public RatingFeedProgress RatingFeedProgress { get; }
        public RatingVideosListModel RatingVideosListModel { get; private set; }
        public Action MoveNextRequested { get; private set; }
        public Action SkipRatingRequested { get; private set; }
        
        //--------------------------------------------------------------------
        // Events 
        //---------------------------------------------------------------------
        
        public event Action Initialized;
        public event Action<VideoRatingCancellationReason> RatingCancelled;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public RatingFeedViewModel(IBridge bridge, RatingFeedProgress ratingFeedProgress)
        {
            _videosForRatingListProvider = new VideosForRatingListProvider(bridge);
            RatingFeedProgress = ratingFeedProgress;
        }

        public async Task InitializeAsync(RatingFeedPageArgs args)
        {
            Level = args.LevelData;
            MoveNextRequested = args.MoveNextRequested;
            SkipRatingRequested = args.SkipRatingRequested;
            
            await _videosForRatingListProvider.InitializeAsync(Level);

            if (_videosForRatingListProvider.Status != Success)
            {
                HandleProviderErrorStatus();
                return;
            }
           
            // not the best option to initialize the list here, but it's the only way to preserve order of initialization
            RatingFeedProgress.InitRatingsList(_videosForRatingListProvider.Videos.Count);
            
            RatingVideos = _videosForRatingListProvider.Videos.Select((video, index) => new RatingVideo(video, RatingFeedProgress.VideoRatings[index])).ToList();
            RatingVideosListModel = new RatingVideosListModel(RatingVideos);
            
            IsInitialized = true;
            Initialized?.Invoke();
        }
        
        public void CleanUp()
        {
            // clear provider and other components here
            
            IsInitialized = false;
        }
        
        public void CancelRating(VideoRatingCancellationReason reason)
        {
            RatingCancelled?.Invoke(reason);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void HandleProviderErrorStatus()
        {
            if (_videosForRatingListProvider.Status == AlreadyRated)
            {
                _snackBarHelper.ShowFailSnackBar("This video has already participated in the rating process.", 2);
                RatingCancelled?.Invoke(VideoRatingCancellationReason.AlreadyRated);
                SkipRatingRequested?.Invoke();
            }
            else
            {
                _snackBarHelper.ShowFailSnackBar("Unable to load videos for rating at this time.", 2);
                RatingCancelled?.Invoke(VideoRatingCancellationReason.VideosUnavailable);
                SkipRatingRequested?.Invoke();
            }
        }
    }
}