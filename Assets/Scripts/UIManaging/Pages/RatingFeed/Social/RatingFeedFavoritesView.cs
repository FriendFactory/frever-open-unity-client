using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Bridge.Services.UserProfile;
using BrunoMikoski.AnimationSequencer;
using Common.Abstract;
using Common.UserBalance;
using TMPro;
using UIManaging.Common.Args.Views.Profile;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.RatingFeed.Social
{
    internal sealed class RatingFeedFavoritesView: BaseContextView<List<Profile>>
    {
        [SerializeField] private Button _skipButton;
        [SerializeField] private Button _continueButton;
        [Space] 
        [SerializeField] private UserPortraitViewSpawner _userPortraitViewSpawner;
        [SerializeField] private TMP_Text _amountText;
        [Space]
        [SerializeField] private UserBalanceView _userBalanceView;
        [Space]
        [SerializeField] private AnimationSequencerController _animationSequencer;

        [Inject] private IBridge _bridge;
        [Inject] private LocalUserDataHolder _localUserDataHolder;
        
        public event Action SkipFavorites;
        public event Action ShowFavorites;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public async Task FadeInAsync(bool showOnStart = true)
        {
            if (showOnStart) Show();
            
            var tcs = new TaskCompletionSource<bool>();
            
            _animationSequencer.Play(() => {
                tcs.SetResult(true);
            });

            await tcs.Task;
        }
        
        //---------------------------------------------------------------------
        // Protected 
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            _skipButton.onClick.AddListener(OnSkipButtonClicked);
            _continueButton.onClick.AddListener(OnContinueButtonClicked);
            
            _amountText.text = $"{ContextData.Count.ToString()}x";
            
            var userPortraitModels = ContextData.Select(profile =>
                new UserPortraitModel()
                {
                    UserGroupId = profile.MainGroupId,
                    UserMainCharacterId = profile.MainCharacter.Id,
                    MainCharacterThumbnail = profile.MainCharacter.Files,
                    Resolution = Resolution._128x128
                }
            ).ToList();
            
            _userPortraitViewSpawner.Initialize(userPortraitModels);

            var userBalanceModel = new StaticUserBalanceModel(_localUserDataHolder);
            _userBalanceView.Initialize(userBalanceModel);
        }
        
        protected override void BeforeCleanUp()
        {
            _skipButton.onClick.RemoveListener(OnContinueButtonClicked);
            _userBalanceView.CleanUp();
        }
        
        //---------------------------------------------------------------------
        // Helpers 
        //---------------------------------------------------------------------

        private void OnSkipButtonClicked()
        {
            _skipButton.onClick.RemoveListener(OnSkipButtonClicked);
            _continueButton.onClick.RemoveListener(OnContinueButtonClicked);

            SkipFavorites?.Invoke();
        }

        private void OnContinueButtonClicked()
        {
            _skipButton.onClick.RemoveListener(OnSkipButtonClicked);
            _continueButton.onClick.RemoveListener(OnContinueButtonClicked);

            ShowFavorites?.Invoke();
        }
    }
}