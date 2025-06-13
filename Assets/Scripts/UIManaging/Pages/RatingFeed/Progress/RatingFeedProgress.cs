using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UIManaging.Pages.RatingFeed.Rating;

namespace UIManaging.Pages.RatingFeed
{
    [UsedImplicitly]
    internal sealed class RatingFeedProgress
    {
        private readonly List<VideoRating> _videoRatings = new();
        private int _ratingsCount;
        private VideoRating _currentVideoRating;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action<int> ProgressChanged;
        public event Action Completed;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public IReadOnlyList<VideoRating> VideoRatings => _videoRatings;
        public int RatingsCount => _ratingsCount;

        public VideoRating CurrentVideoRating
        {
            get => _currentVideoRating;
            private set
            {
                if (_currentVideoRating != null)
                {
                    _currentVideoRating.ScoreChanged -= OnScoreChanged;
                }

                _currentVideoRating = value;

                _currentVideoRating.ScoreChanged += OnScoreChanged;

                ProgressChanged?.Invoke(CurrentProgressIndex);
            }
        }

        public int CurrentProgressIndex { get; private set; }
        public int PreviousProgressIndex { get; private set; }
        public bool IsCompleted { get; private set; }

        //---------------------------------------------------------------------
        // Internal
        //---------------------------------------------------------------------

        internal void InitRatingsList(int count)
        {
            _ratingsCount = count;

            for (var i = 0; i < _ratingsCount; i++)
            {
                _videoRatings.Add(new VideoRating());
            }

            CurrentProgressIndex = 0;
            PreviousProgressIndex = -1;

            if (_videoRatings.Any())
            {
                CurrentVideoRating = _videoRatings[CurrentProgressIndex];
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnScoreChanged(int score)
        {
            if (IsCompleted) return;
            
            PreviousProgressIndex = CurrentProgressIndex;
            CurrentProgressIndex++;
            
            if (CurrentProgressIndex == _ratingsCount)
            {
                IsCompleted = true;
                Completed?.Invoke();
            }
            else
            {
                CurrentVideoRating = _videoRatings[CurrentProgressIndex];
            }
        }
    }
}