using UIManaging.Pages.PublishPage.VideoDetails.Attributes;
using Zenject;

namespace UIManaging.Pages.PublishPage
{
    internal sealed class PublishPageInstaller: MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<VideoPostAttributesModel>().AsSingle();
        }
    }
}