using System.Threading.Tasks;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Modules.SignUp;
using Modules.UserScenarios.Common;
using Navigation.Core;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.OnBoardingPage.UI.Args;

namespace Modules.UserScenarios.Implementation.Onboarding.States
{
    [UsedImplicitly]
    internal class SignInVerificationCodeState : StateBase<ISignupContext>, IResolvable
    {
        private readonly PageManager _pageManager;
        private readonly ISignInService _signInService;
        private readonly LocalUserDataHolder _localUser;
        private readonly IDataFetcher _dataFetcher;

        private VerificationCodeArgs _args;
        
        public ITransition MoveBack;
        public ITransition MoveNext;

        public override ScenarioState Type => ScenarioState.SignInVerificationCode;
        public override ITransition[] Transitions => new[] { MoveBack, MoveNext }.RemoveNulls();

        public SignInVerificationCodeState(PageManager pageManager, ISignInService signInService,
            LocalUserDataHolder localUser, IDataFetcher dataFetcher)
        {
            _pageManager = pageManager;
            _signInService = signInService;
            _localUser = localUser;
            _dataFetcher = dataFetcher;
        }
        
        public override async void Run()
        {
            await _dataFetcher.FetchLocalization();
            
            await _signInService.RequestVerificationCode();
            
            _args = new VerificationCodeArgs
            {
                Description = _signInService.VerificationCodeDescription(),
                MoveBackRequested = OnMoveBackRequested,
                MoveNextRequested = OnMoveNextRequested,
                OnValueChanged = OnValueChanged,
                NewVerificationCodeRequested = OnNewVerificationCodeRequested,
            };
            
            _pageManager.MoveNext(_args);
        }

        private async void OnMoveBackRequested()
        {
            await MoveBack.Run();
        }

        private async void OnMoveNextRequested()
        {
            var result = await _signInService.LoginUser();
            
            if (result.IsSuccess)
            {
                if (_localUser.UserProfile == null)
                {
                    await _localUser.DownloadProfile();
                }
                
                await MoveNext.Run();
                return;
            }

            _args.MoveNextFailed?.Invoke();
        }

        private void OnValueChanged(string value)
        {
            _signInService.SetVerificationCode(value);
        }

        private Task OnNewVerificationCodeRequested()
        {
            return _signInService.RequestVerificationCode();
        }
    }
}