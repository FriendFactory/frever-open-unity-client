using JetBrains.Annotations;
using UnityEngine;
#if UNITY_IOS
using UnityEngine.iOS;
#endif

namespace Modules.RenderingPipelineManagement.iOS
{
    [UsedImplicitly]
    internal sealed class iOSRenderingPipelineManager : BaseRenderingPipelineManager
    {
        private const string PROFILE_FILE_PATH = "ScriptableObjects/iOS Rendering Pipeline Profile";

        //---------------------------------------------------------------------
        // IRenderingPipelineManager
        //---------------------------------------------------------------------

        public override void Init()
        {
            var profile = Resources.Load<RenderingPipelineProfile>(PROFILE_FILE_PATH);

            #if UNITY_IOS
                var currentDevice = GetDeviceGeneration();
	            profile.CurrentDeviceGeneration = currentDevice;
            #endif

            PipeType = PipelineType.Default;
	        PipelinesData = profile.GetPipelines();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        #if UNITY_IOS
        private DeviceGeneration GetDeviceGeneration()
        {
            #if UNITY_EDITOR
                return DeviceGeneration.iPhoneXS;
            #else
                return Device.generation;
            #endif
        }
        #endif
    }
}