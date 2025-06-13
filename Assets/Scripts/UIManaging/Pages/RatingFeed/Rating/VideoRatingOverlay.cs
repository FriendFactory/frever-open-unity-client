using BrunoMikoski.AnimationSequencer;
using UIManaging.Pages.Common.SongOption;
using UnityEngine;

namespace UIManaging.Pages.RatingFeed.Rating
{
    internal sealed class VideoRatingOverlay: BaseContextlessView 
    {
        [SerializeField] private AnimationSequencerController _animationSequencer;
        
        protected override void OnInitialized() { }

        public override void Show()
        {
            if (!IsInitialized)
            {
                Initialize();
            }
            
            base.Show();
        }

        protected override void OnShow()
        {
            base.OnShow();
            
            _animationSequencer.Play();
        }
    }
}