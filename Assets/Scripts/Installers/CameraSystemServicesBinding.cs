using Cinemachine;
using Modules.CameraSystem.CameraAnimations;
using Modules.CameraSystem.CameraSettingsHandling;
using Modules.CameraSystem.CameraSystemCore;
using Modules.CameraSystem.Extensions.CinemachineExtensions;
using Modules.CameraSystem.PlayerCamera;
using Modules.CameraSystem.PlayerCamera.Handlers;
using Modules.CameraSystem.PlayerCamera.Raycasting;
using Modules.CameraSystem.Preview;
using Modules.LevelManaging.Editing.Players.AssetPlayers;
using UIManaging.Pages.LevelEditor.Ui;
using UIManaging.Pages.LevelEditor.Ui.SushiBarComponents;
using Zenject;

namespace Installers
{
    internal static class CameraSystemServicesBinding
    {
        public static void BindCameraSystemServices(this DiContainer container)
        {
            container.BindInterfacesAndSelfTo<CameraSystem>().AsSingle();

            InjectCameraSystemInternalServices(container);
        }

        private static void InjectCameraSystemInternalServices(DiContainer container)
        {
            container.BindInterfacesAndSelfTo<CameraSwitchHistory>().AsSingle();
            container.BindInterfacesAndSelfTo<CameraDofManager>().AsSingle();
            container.BindInterfacesAndSelfTo<CameraAnimationSaver>().AsSingle();
            container.BindInterfacesAndSelfTo<CameraAnimationHolder>().AsSingle();
            container.BindInterfacesAndSelfTo<CameraAnimationCreator>().AsSingle();
            container.BindInterfacesAndSelfTo<CameraAnimationRecorder>().AsSingle();
            container.BindInterfacesAndSelfTo<CameraAnimator>().AsSingle();
            container.BindInterfacesAndSelfTo<AnimationPlayingManager>().AsSingle();
            container.BindInterfacesAndSelfTo<TransformBasedController>().AsSingle();
            container.BindInterfacesAndSelfTo<CameraSettingControl>().AsSingle();
            container.BindInterfacesAndSelfTo<CameraRaycastHandler>().AsSingle();
            container.BindInterfacesAndSelfTo<CameraDofUpdater>().AsSingle();
            container.BindInterfacesAndSelfTo<CameraNoiseProfileHandler>().AsSingle();
            container.BindInterfacesAndSelfTo<RotationCurve360FlipCorrector>().AsSingle();
            container.BindInterfacesAndSelfTo<CinemachineRelativeRotationKeeper>().AsSingle();
            container.BindInterfacesAndSelfTo<CameraAnimationConverter>().AsSingle();
            container.BindInterfacesAndSelfTo<CameraTemplatesManager>().AsSingle();
            container.Bind<TemplateCameraAnimationSpeedUpdater>().AsSingle();
            container.Bind<PlayingTypeProvider>().AsSingle();
            container.Bind<CameraAnimationPlayer>().AsTransient();
            container.BindInterfacesAndSelfTo<CameraAnimationPropsReplacer>().AsSingle();
            
            container.BindInterfacesAndSelfTo<CinemachineFreeLook>().FromNewComponentOnNewGameObject().WithGameObjectName("FreeLookCamera").AsSingle();
            container.BindInterfacesAndSelfTo<CinemachineBasedController>().FromNewComponentOn(context => context.Container.Resolve<CinemachineFreeLook>().gameObject).AsSingle();
        }
    }
}