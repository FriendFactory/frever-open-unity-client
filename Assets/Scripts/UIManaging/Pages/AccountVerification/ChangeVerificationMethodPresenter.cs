using System.Threading.Tasks;
using JetBrains.Annotations;
using Modules.AccountVerification;
using Modules.AccountVerification.LoginMethods;
using Navigation.Core;
using UIManaging.Pages.Common.UsersManagement;

namespace UIManaging.Pages.AccountVerification
{
    [UsedImplicitly]
    public sealed class ChangeVerificationMethodPresenter : VerificationMethodPresenterBase
    {
        public ChangeVerificationMethodPresenter(PageManager pageManager, VerificationCodePageArgsFactory codePageArgsFactory,
            AccountVerificationService accountVerificationService, AccountVerificationLocalization localization,
            LoginMethodsProvider loginMethodsProvider, LocalUserDataHolder localUserDataHolder)
            : base(pageManager, codePageArgsFactory, accountVerificationService, localization, loginMethodsProvider, localUserDataHolder)
        {
        }
        
        protected override VerificationMethodOperationType OperationType => VerificationMethodOperationType.Change;

        protected override async Task<VerificationResult> UpdateVerificationMethodAsync(IVerificationMethod method)
        {
            return await AccountVerificationService.ChangeVerificationMethodAsync(method);
        }
    }
}