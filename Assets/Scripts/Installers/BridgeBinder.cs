using AppStart;
using Bridge;
using Modules.Encryption;
using Utils;
using Zenject;

namespace Installers
{
    internal static class BridgeBinder
    {
        public static void BindAndSetupBridge(this DiContainer container)
        {
            var bridge = AppEntryContext.Bridge;
            container.BindInterfacesAndSelfTo<ServerBridge>().FromInstance(bridge);
            EncryptionServiceProvider.Initialize(bridge);
            var platform = PlatformUtils.GetRuntimePlatform();
            bridge.SetPlatform(platform);
        }
    }
}