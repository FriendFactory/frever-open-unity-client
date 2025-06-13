using Modules.CameraSystem.CameraAnimations;
using Modules.CameraSystem.PlayerCamera;
using Modules.LevelManaging.Editing.CameraManaging.CameraInputHandling;
using Modules.LevelManaging.Editing.EventRecording;
using Zenject;

namespace Installers
{
    internal static class ExecutionOrderSetup
    {
        public static void SetupExecutionOrder(this DiContainer container)
        {
            container.BindExecutionOrder<StopWatch>(-1);//should calculate current frame time before any other entity tries get it
            container.BindExecutionOrder<CameraAnimator>(1);
            container.BindExecutionOrder<CameraInputController>(2);//since it overrides  camera animator, it should be after camera animator, but before Cinemachine
            container.BindExecutionOrder<CinemachineBasedController>(3);//should apply changes to Camera game object after CameraAnimator set next frame values
        }
    }
}