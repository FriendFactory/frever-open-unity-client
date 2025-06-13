using System;
using System.Threading;
using System.Threading.Tasks;
using Abstract;
using Bridge;
using Bridge.Services.UserProfile;
using Common.Collections;
using TMPro;
using UIManaging.Common.Args.Views.Profile;
using UIManaging.PopupSystem.Popups.SwipeToFollow.States;
using UnityEngine;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.PopupSystem.Popups.SwipeToFollow
{
    internal sealed class SwipeToFollowUserCard : BaseContextDataView<SwipeToFollowUserCardModel>
    {
        [SerializeField] private UserCardStackElement _userCardStackElement;
        [Header("Thumbnails")]
        [SerializeField] private UserPortrait _userPortrait;
        [SerializeField] private UserPortrait _userCoverPortrait;
        [Header("User Info")] 
        [SerializeField] private ProfileKPIView _profileKPIView;
        [SerializeField] private RankBadgeView _rankBadgeView;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _bioText;
        [SerializeField] private Color _bioTextColor;
        [SerializeField] private string _bioTextPlaceholder;
        [SerializeField] private Color _bioTextPlaceholderColor;
        [Header("View States")]
        [SerializeField] private UserCardViewStateMap  _viewStatesMap;

        [Inject] private IBridge _bridge;
        
        public UserCardStackElement UserCardStackElement => _userCardStackElement;
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override async void OnInitialized()
        {
            try
            {
                if (ContextData.Profile == null) return;
                
                UpdateBio(ContextData.Profile);
                UpdateName(ContextData.Profile);
                
                _profileKPIView.Initialize(ContextData.Profile.KPI);
                _rankBadgeView.Initialize(ContextData.Profile.CreatorScoreBadge);
                
                _userCardStackElement.Initialize(ContextData);
                
                await LoadUserPortraitsAsync(ContextData.Profile);
                
                foreach (var viewState in _viewStatesMap.Values)
                {
                    viewState.Initialize(ContextData);
                }
                
                if (_viewStatesMap.TryGetValue(ContextData.State.Value, out var currentViewState))
                {
                    currentViewState.Enter();
                }

                ContextData.StateChanged += OnStateChanged;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        protected override void BeforeCleanup()
        {
            if (!IsInitialized) return;
            
            ContextData.StateChanged -= OnStateChanged;

            foreach (var viewState in _viewStatesMap.Values)
            {
                viewState.CleanUp();
            }
                
            _userPortrait.CleanUp();
            _profileKPIView.CleanUp();
            _rankBadgeView.CleanUp();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private async Task LoadUserPortraitsAsync(Profile profile, CancellationToken token = default)
        {
            await _userPortrait.InitializeAsync(profile, Resolution._128x128, token);
            await _userCoverPortrait.InitializeAsync(profile, Resolution._256x256, token);
            
            _userPortrait.ShowContent();
            _userCoverPortrait.ShowContent();
        }

        private void UpdateName(Profile profile)
        {
            _nameText.text = profile.NickName;
        }

        private void UpdateBio(Profile profile)
        {
            if (string.IsNullOrWhiteSpace(profile.Bio))
            {
                _bioText.text = _bioTextPlaceholder;
                _bioText.color = _bioTextPlaceholderColor;
            }
            else
            {
                _bioText.text = profile.Bio;
                _bioText.color = _bioTextColor;
            }
        }

        private void OnStateChanged(UserCardState source, UserCardState destination)
        {
            if (_viewStatesMap.TryGetValue(source, out var sourceState))
            {
                sourceState.Exit();
            }

            if (_viewStatesMap.TryGetValue(destination, out var destinationState))
            {
                destinationState.Enter();
            }
        }
    }

    //---------------------------------------------------------------------
    // Nested
    //---------------------------------------------------------------------

    [Serializable]
    internal class UserCardViewStateMap : SerializedDictionary<UserCardState, UserCardViewStateBase>
    {
    }
}