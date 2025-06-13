using Modules.AudioOutputManaging;
using Modules.AudioOutputManaging.iOS;
using Zenject;

namespace Installers
{
    internal static class DeviceAudioOutputControlBinder
    {
        public static void BindDeviceAudioOutputControl(this DiContainer container)
        {
            container.Bind<IDeviceAudioOutputControl>().To<DeviceAudioOutputControl>().AsSingle();
        }
    }
}