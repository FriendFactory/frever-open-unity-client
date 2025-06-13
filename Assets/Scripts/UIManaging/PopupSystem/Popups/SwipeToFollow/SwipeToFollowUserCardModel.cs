using System;
using System.Threading.Tasks;
using Bridge.Services.UserProfile;
using Laphed.Rx;
using Stateless;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups.SwipeToFollow
{
    internal sealed class SwipeToFollowUserCardModel
    {
        private readonly Profile _profile;
        private readonly StateMachine<UserCardState, UserCardTrigger> _stateMachine;

        public Profile Profile { get; private set; }
        public ReactiveProperty<UserCardState> State { get; } = new();
        public SwipeDirection SwipeDirection { get; set; }
        
        public event Action<UserCardState, UserCardState> StateChanged;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        public SwipeToFollowUserCardModel(Profile profile)
        {
            Profile = profile;
            _stateMachine = new StateMachine<UserCardState, UserCardTrigger>(UserCardState.InDeck);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public async Task InitializeAsync()
        {
            InitializeStateMachine();
            // Swipe gesture doesn't initialize immediately for some reason
            await Task.Delay(30);
        }

        public void Fire(UserCardTrigger trigger)
        {
            if (!_stateMachine.CanFire(trigger))
            {
                Debug.LogError($"Failed to fire trigger {trigger} in state {_stateMachine.State}");
                return;
            }
            
            _stateMachine.Fire(trigger);
        }
        
        private void InitializeStateMachine()
        {
            _stateMachine.Configure(UserCardState.InDeck)
                         .Permit(UserCardTrigger.MoveOnTop, UserCardState.OnTop);

            _stateMachine.Configure(UserCardState.OnTop)
                         .Permit(UserCardTrigger.Swipe, UserCardState.Swiping)
                         .Permit(UserCardTrigger.ManualSwipe, UserCardState.Released);
            
            _stateMachine.Configure(UserCardState.Swiping)
                         .Permit(UserCardTrigger.Release, UserCardState.Released);
            
            _stateMachine.OnTransitioned(transition =>
            {
                State.Value = transition.Destination;
                StateChanged?.Invoke(transition.Source, transition.Destination);
            });
        }
    }

    //---------------------------------------------------------------------
    // Nested
    //---------------------------------------------------------------------

    internal enum UserCardState
    {
        InDeck,
        OnTop,
        Swiping,
        Released,
    }

    internal enum UserCardTrigger
    {
        MoveOnTop,
        Swipe,
        ManualSwipe,
        Release,
    }

    internal enum SwipeDirection
    {
        Left,
        Right,
    }
}