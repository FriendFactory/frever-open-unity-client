using Modules.AccountVerification;
using Modules.AccountVerification.LoginMethods;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.AccountVerification
{
    internal sealed class VerificationMethodPageInstaller: MonoInstaller
    {
        [SerializeField] private AccountVerificationLocalization _accountVerificationLocalization;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<VerificationMethodsProvider>().AsSingle();
            Container.Bind<AccountVerificationLocalization>().FromInstance(_accountVerificationLocalization).AsSingle();
            Container.BindInterfacesAndSelfTo<VerificationMethodPageArgsFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<VerificationCodePageArgsFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<AddVerificationMethodPresenter>().AsSingle();
            Container.BindInterfacesAndSelfTo<ChangeVerificationMethodPresenter>().AsSingle();
            Container.BindInterfacesAndSelfTo<VerificationMethodUpdateEventHandler>().AsSingle();
        }

        public override async void Start()
        {
            await Container.Resolve<LoginMethodsProvider>().UpdateLoginMethodsAsync();
        }
    }
}