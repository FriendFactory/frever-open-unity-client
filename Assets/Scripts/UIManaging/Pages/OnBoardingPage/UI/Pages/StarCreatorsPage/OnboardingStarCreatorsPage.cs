using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Services.UserProfile;
using Extensions;
using Navigation.Args;
using Navigation.Args.Feed;
using Navigation.Core;
using TMPro;
using UIManaging.Common.Args.Views.Profile;
using UIManaging.Localization;
using UIManaging.Pages.Common.VideoManagement;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.OnBoardingPage.UI.Pages
{
    internal sealed class OnboardingStarCreatorsPage : GenericPage<OnboardingStarCreatorsPageArgs>
    {
        private const int CREATOR_COUNT = 5;

        [Inject] private ISocialBridge _bridge;
        [Inject] private PageManager _pageManager;
        [Inject] private VideoManager _videoManager;
        [Inject] private OnBoardingLocalization _localization;


        [SerializeField] private Button _continueButton;
        [SerializeField] private TMP_Text _buttonLabel;
        [SerializeField] private LoadingIndicator _loadingIndicator;
        [SerializeField] private List<FollowStarCreatorView> _starCreatorViews;

        private readonly List<FollowStarCreatorModel> _starCreatorModels = new List<FollowStarCreatorModel>();
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public override PageId Id => PageId.OnboardingStarCreatorsPage;

        private void Awake()
        {
            _continueButton.onClick.AddListener(ContinueButtonClick);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _continueButton.onClick.RemoveAllListeners();
        }

        protected override void OnInit(PageManager pageManager)
        {
            
        }

        protected override async void OnDisplayStart(OnboardingStarCreatorsPageArgs args)
        {
            var result = await _bridge.GetStarCreatorsInYourCountry(_tokenSource.Token);
            if (result.IsError)
            {
                Debug.LogError(result.ErrorMessage);
                return;
            }

            if (result.IsRequestCanceled) return;

            var profiles = result.Profiles.Take(CREATOR_COUNT);
            CreateStarCreatorModels(profiles);
            InitializeCreatorViews();
        }

        private void CreateStarCreatorModels(IEnumerable<Profile> profiles)
        {
            foreach (var profile in profiles)
            {
                if (profile?.MainCharacter == null) continue;
                
                var groupId = profile.MainGroupId;
                var nickname = profile.NickName;
                var followers = profile.KPI.FollowersCount;
                var portraitModel = new UserPortraitModel
                {
                    UserGroupId = profile.MainGroupId,
                    UserMainCharacterId = profile.MainCharacter.Id,
                    MainCharacterThumbnail = profile.MainCharacter.Files,
                    Resolution = Resolution._128x128
                };

                var model = new FollowStarCreatorModel(groupId, nickname, followers, portraitModel);
                _starCreatorModels.Add(model);
            }
        }

        private void InitializeCreatorViews()
        {
            for (var i = 0; i < _starCreatorModels.Count; i++)
                _starCreatorViews[i].Initialize(_starCreatorModels[i]);
        }

        private async void ContinueButtonClick()
        {
            _continueButton.onClick.RemoveAllListeners();
            _buttonLabel.SetActive(false);
            _loadingIndicator.SetActive(true);
            
            var followTasks = new List<Task>();
            foreach (var starCreatorModel in _starCreatorModels)
            {
                if (!starCreatorModel.IsMarkedToFollow) continue;

                followTasks.Add(_bridge.StartFollow(starCreatorModel.GroupId));
            }

            await Task.WhenAll(followTasks);

            OpenPageArgs.OnContinueButtonClick?.Invoke();
        }
    }
}