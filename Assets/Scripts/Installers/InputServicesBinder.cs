using DigitalRubyShared;
using Modules.InputHandling;
using Modules.LevelManaging.Editing.CameraManaging.CameraInputHandling;
using Zenject;

namespace Installers
{
    internal static class InputServicesBinder
    {
        public static void BindInputServices(this DiContainer container)
        {
            container.Bind<FingersScript>().FromInstance(FingersScript.Instance);
            container.BindInterfacesAndSelfTo<InputManager>().AsSingle();
            container.BindInterfacesAndSelfTo<CameraInputController>().AsSingle();
            container.BindInterfacesAndSelfTo<UserSystemInputEventHandler>().AsSingle();
        }
    }
}
