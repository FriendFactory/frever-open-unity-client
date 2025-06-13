using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Bridge.Services.UserProfile;
using Common.Abstract;
using Components;
using Laphed.Rx;
using QFSW.QC;
using UIManaging.Pages.Common.FollowersManagement;
using UIManaging.PopupSystem.Popups.SwipeToFollow.CardSwipe.Core;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.PopupSystem.Popups.SwipeToFollow
{
    internal sealed class SwipeToFollowCardsController : BaseContextPanel<List<Profile>>
    {
        [SerializeField] private ViewSpawner _viewSpawner;
        [SerializeField] private UserCardStack2D _cardStack2D;
        [Header("Buttons")] [SerializeField] private Button _ignoreButton;
        [SerializeField] private Button _likeButton;

        [Inject] private IBridge _bridge;
        [Inject] private FollowersManager _followersManager;

        private List<SwipeToFollowUserCardModel> _cardModels;
        private List<SwipeToFollowUserCard> _cards;
        private int _usersCount;
        private SwipeToFollowUserCard _activeUserCard;

        public ReactiveProperty<int> CurrentUserIndex { get; } = new();

        public event Action Completed;

        protected override async void OnInitialized()
        {
            try
            {
                _cardModels = ContextData.Select(x => new SwipeToFollowUserCardModel(x)).ToList();

                await Task.WhenAll(_cardModels.Select(card => card.InitializeAsync()));

                _cards = _viewSpawner.Spawn<SwipeToFollowUserCardModel, SwipeToFollowUserCard>(_cardModels, false)
                                     .ToList();
                _usersCount = _cardModels.Count;

                _cardStack2D.Initialize(_cards.Select(card => card.UserCardStackElement).ToList());

                _cards.Reverse();

                CurrentUserIndex.Value = 0;
                MoveNext();

                _ignoreButton.onClick.AddListener(OnIgnoreButtonClick);
                _likeButton.onClick.AddListener(OnLikeButtonClick);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        protected override void BeforeCleanUp()
        {
            _viewSpawner.CleanUp<SwipeToFollowUserCardModel, SwipeToFollowUserCard>(false);
            _cardStack2D.CleanUp();

            _ignoreButton.onClick.RemoveListener(OnIgnoreButtonClick);
            _likeButton.onClick.RemoveListener(OnLikeButtonClick);
        }

        private void OnIgnoreButtonClick()
        {
            ShowNextUserCard(false);
        }

        private void OnLikeButtonClick()
        {
            _followersManager.FollowUser(_activeUserCard.ContextData.Profile.MainGroupId);
            ShowNextUserCard(true);
        }

        private void ShowNextUserCard(bool like)
        {
            _activeUserCard.ContextData.SwipeDirection = like ? SwipeDirection.Right : SwipeDirection.Left;
            _activeUserCard.ContextData.Fire(UserCardTrigger.Swipe);
        }

        private void OnCardStateChanged(UserCardState source, UserCardState destination)
        {
            if (destination is not UserCardState.Released) return;

            _activeUserCard.ContextData.StateChanged -= OnCardStateChanged;

            CurrentUserIndex.Value++;
            MoveNext();
        }

        private async void MoveNext()
        {
            try
            {
                if (CurrentUserIndex.Value == _usersCount)
                {
                    Completed?.Invoke();
                    return;
                }

                _activeUserCard = _cards[CurrentUserIndex.Value];

                _activeUserCard.ContextData.StateChanged += OnCardStateChanged;

                if (CurrentUserIndex.Value > 0)
                {
                    await _cardStack2D.MoveNextAsync();
                }
                else
                {
                    _activeUserCard.ContextData.Fire(UserCardTrigger.MoveOnTop);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}