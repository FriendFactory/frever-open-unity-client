using UnityEngine;
using Zenject;

namespace UIManaging.Pages.AppSettingsPage.LoginMethods
{
    public class LoginMethodsPreviewInstaller: MonoInstaller
    {
        [SerializeField] private LoginMethodsLocalization _localization;
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<LoginMethodsPreviewPanelPresenter>().AsSingle();
            Container.Bind<LoginMethodsLocalization>().FromInstance(_localization).AsSingle();
        }
    }
}