using BrunoMikoski.AnimationSequencer;
using Common.Abstract;
using Extensions;
using UIManaging.Pages.Feed.GamifiedFeed;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Feed
{
    public class GamifiedFeedLikesToggle : BaseContextToggle<GamifiedFeedLikesModel>
    {
        [SerializeField] private VideoKPICount _likesCount;
        [SerializeField] private AnimationSequencerController _animationSequencer;

        public Toggle Toggle => _toggle;

        protected override void OnInitialized()
        {
            _likesCount.Initialize(ContextData);
            
            _toggle.SetIsOnWithoutNotify(ContextData.LikedByCurrentUser);

            if (!ContextData.LikedByCurrentUser) return;

            if (!_animationSequencer.IsPlaying)
            {
                _animationSequencer.Play();
            }
            _animationSequencer.Complete();
        }

        protected override void BeforeCleanup()
        {
            _likesCount.CleanUp();
            
            _animationSequencer.Kill();
            _animationSequencer.ResetToInitialState();
        }

        protected override void OnValueChanged(bool isOn)
        {
            if (isOn)
            {
                _animationSequencer.PlayForward();
            }
            else
            {
                _animationSequencer.PlayBackwards();
            }

            ContextData.Count += isOn ? 1 : -1;
            ContextData.LikedByCurrentUser = isOn;
            
            base.OnValueChanged(isOn);
        }
    }
}