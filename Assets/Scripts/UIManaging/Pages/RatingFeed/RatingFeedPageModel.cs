using System;
using JetBrains.Annotations;

namespace UIManaging.Pages.RatingFeed
{
    [UsedImplicitly]
    internal sealed class RatingFeedPageModel
    {
        public event Action RatingStarted;
        
        public void OnRatingStarted()
        {
            RatingStarted?.Invoke();
        }
        
    }
}