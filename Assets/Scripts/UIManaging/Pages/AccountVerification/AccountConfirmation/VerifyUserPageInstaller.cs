using Zenject;

namespace UIManaging.Pages.AccountVerification.AccountConfirmation
{
    internal sealed class VerifyUserPageInstaller: MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<VerificationFlowProvider>().AsSingle();
            Container.BindInterfacesAndSelfTo<VerifyUserPresenter>().AsSingle();
        }
    }
}