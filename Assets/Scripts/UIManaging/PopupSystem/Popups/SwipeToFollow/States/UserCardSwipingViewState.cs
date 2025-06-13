using BrunoMikoski.AnimationSequencer;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups.SwipeToFollow.States
{
    internal sealed class UserCardSwipingViewState: UserCardViewStateBase
    {
        [SerializeField] private AnimationSequencerController _animationSequencerLike;
        [SerializeField] private AnimationSequencerController _animationSequencerDislike;

        protected override void OnEnter()
        {
            SwipeOut();
        }

        protected override void OnExit() { }

        private void SwipeOut()
        {
            if (ContextData.SwipeDirection == SwipeDirection.Right)
            {
                _animationSequencerLike.Play(OnCompleted);
            }
            else
            {
                _animationSequencerDislike.Play(OnCompleted);
            }

            void OnCompleted()
            {
                ContextData.Fire(UserCardTrigger.Release);
            }
        }
    }
}