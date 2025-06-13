using Modules.SocialActions;
using Zenject;

namespace Installers
{
    public static class SocialActionsBinder
    {
        public static void BindSocialActionsServices(this DiContainer container, SocialActionModelFactory factory)
        {
            container.Bind<SocialActionModelFactory>().FromInstance(factory);
            container.BindInterfacesAndSelfTo<SocialActionsManager>().AsSingle();
        }
        
    }
}