using Modules.Amplitude;
using Modules.Amplitude.Events.App;
using Modules.Amplitude.Events.PageChange;
using Modules.Amplitude.Signals;
using Zenject;

namespace Installers
{
    internal static class AmplitudeServicesBinder 
    {
        public static void BindAmplitudeServices(this DiContainer container)
        {
            container.BindInterfacesAndSelfTo<AmplitudeManager>().AsSingle();
            container.Bind<AmplitudeAssetEventLogger>().AsSingle();
            
            container.DeclareSignal<AmplitudeEventSignal>();

            container.BindInterfacesAndSelfTo<AmplitudeEventSignalListener>().AsSingle();
            container.BindInterfacesAndSelfTo<PageChangeAmplitudeEventSignalEmitter>().AsSingle();
            container.BindInterfacesAndSelfTo<AppEventsAmplitudeEventSignalEmitter>().AsSingle();
        }
    }
}
