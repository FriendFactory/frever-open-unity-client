using Zenject;

namespace UIManaging.Pages.UmaEditorPage.Ui
{
    internal sealed class UmaEditorPageInstaller: MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<NotOwnedWardrobesProvider>().AsSingle();
        }
    }
}