using Bridge;
using Bridge.Models.ClientServer.Template;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Modules.CharacterManagement;
using Modules.UniverseManaging;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.CharacterCreation;
using Navigation.Args.Feed;
using Navigation.Core;
using UIManaging.Pages.Common.UserLoginManagement;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.Common.VideoManagement;
using UIManaging.Pages.Loading;
using UnityEngine;

namespace Modules.UserScenarios.Implementation.Onboarding.States
{
    [UsedImplicitly]
    internal sealed class ExitToPostOnboardingPageState : ExitStateBase<ICharacterCreationContext>, IResolvable
    {
        private readonly LocalUserDataHolder _localUser;
        private readonly PageManager _pageManager;
        private readonly VideoManager _videoManager;
        private readonly UserAccountManager _userAccountManager;
        private readonly IBridge _bridge;
        private readonly CharacterManager _characterManager;
        private readonly IDataFetcher _dataFetcher;

        public override ScenarioState Type => ScenarioState.PostOnboardingExit;

        public ExitToPostOnboardingPageState(LocalUserDataHolder localUser, PageManager pageManager, VideoManager videoManager,
            UserAccountManager userAccountManager, IBridge bridge, CharacterManager characterManager,
            IDataFetcher dataFetcher)
        {
            _localUser = localUser;
            _pageManager = pageManager;
            _videoManager = videoManager;
            _userAccountManager = userAccountManager;
            _bridge = bridge;
            _characterManager = characterManager;
            _dataFetcher = dataFetcher;
        }
        
        public override async void Run()
        {
            await _userAccountManager.OnLoggedIn();
            var loadingPageArgs = new LoadingPageArgs(PageId.StartupLoadingPage, true)
                { FetchDefaults = true, WaitForStartPack = true, OnDataFetchedAction = OnLoadingPageFinished };
            _pageManager.MoveNext(loadingPageArgs);
        }
        
        private async void OnLoadingPageFinished()
        {
            var result = await _bridge.CompleteOnboarding();

            if (result.IsError)
            {
                Debug.LogError($"Failed to mark onboarding as completed, reason: {result.ErrorMessage}");
            }
            
            await _dataFetcher.FetchLocalization();

            if (_localUser.UserProfile == null)
            {
                await _localUser.DownloadProfile();
            }
            
            if (_localUser.UserProfile.MainCharacter?.Id != null)
            {
                _characterManager.SetCharacterSilent(_localUser.UserProfile.MainCharacter.Id);
            }

            _pageManager.MoveNext(PageId.Feed, new GeneralFeedArgs(_videoManager));
        }
    }
}