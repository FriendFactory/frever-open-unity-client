using Modules.DeepLinking;
using Zenject;

namespace Installers
{
    public static class DeeplinkServicesBinder
    {
        public static void BindDeeplinkServices(this DiContainer container)
        {
            container.BindInterfacesAndSelfTo<InvitationLinkHandler>().AsSingle();
            container.BindInterfacesAndSelfTo<InvitationCodeModel>().AsSingle();
        }
    }
}