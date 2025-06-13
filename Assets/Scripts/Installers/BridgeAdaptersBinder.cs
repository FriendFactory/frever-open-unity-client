using Common.BridgeAdapter;
using Modules.Chat;
using Zenject;

namespace Installers
{
    internal static class BridgeAdaptersBinder
    {
        public static void BindBridgeAdapters(this DiContainer container)
        {
            container.BindInterfacesAndSelfTo<LevelService>().AsSingle();
            container.BindInterfacesAndSelfTo<ChatService>().AsSingle();
        }
    }
}