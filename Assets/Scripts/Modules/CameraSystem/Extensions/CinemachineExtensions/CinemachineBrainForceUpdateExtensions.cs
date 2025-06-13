using System.Reflection;
using Cinemachine;

namespace Modules.CameraSystem.Extensions.CinemachineExtensions
{
    /// <summary>
    ///     We need to be able immediately update and apply camera state to camera game object.
    ///     For instance, it's necessary when we try to set camera on last frame via cinemachine.
    ///     Otherwise, changes will be applied at the end of LateUpdate cycle
    /// </summary>
    internal static class CinemachineBrainForceUpdateExtensions
    {
        private static readonly MethodInfo LateUpdateMethod = typeof(CinemachineBrain).GetMethod(
            "LateUpdate",
            BindingFlags.Instance | BindingFlags.NonPublic);

        public static void ForceUpdate(this CinemachineBrain brain)
        {
            LateUpdateMethod.Invoke(brain, null);
        }
    }
}