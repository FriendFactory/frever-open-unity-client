using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Modules.SignUp;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.CharacterCreation;
using Navigation.Core;
using UIManaging.Pages.Loading;

namespace Modules.UserScenarios.Implementation.Onboarding.States
{
    [UsedImplicitly]
    internal sealed class OnboardingLoadingState: StateBase<ICharacterCreationContext>, IResolvable
    {
        private readonly PageManager _pageManager;
        private readonly ISignUpService _signUpService;
        private readonly IDataFetcher _dataFetcher;

        public ITransition MoveNext;
        
        public override ScenarioState Type => ScenarioState.OnboardingLoadingState;
        public override ITransition[] Transitions => new[] { MoveNext };

        public OnboardingLoadingState(PageManager pageManager, ISignUpService signUpService, IDataFetcher dataFetcher)
        {
            _pageManager = pageManager;
            _signUpService = signUpService;
            _dataFetcher = dataFetcher;
        }

        public override async void Run()
        {
            await _signUpService.CreateTemporaryAccount();
            
            var loadingPageArgs = new LoadingPageArgs(PageId.StartupLoadingPage)
            {
                FetchDefaults = true,
                WaitForStartPack = true,
                OnDataFetchedAction = DataFetched
            };

            loadingPageArgs.OnDataFetchedAction = DataFetched;
            
            _pageManager.MoveNext(loadingPageArgs);
           
            async void DataFetched()
            {
                await _dataFetcher.FetchLocalization();
                await MoveNext.Run();
            }
        }
    }
}