using System;
using System.Collections.Generic;
using Common.ApplicationCore;
using Modules.Amplitude;
using Modules.AudioOutputManaging;
using Modules.FrameRate;
using Modules.RenderingPipelineManagement;
using Modules.SentryManaging;
using Settings;
using UIManaging;
using UIManaging.PopupSystem;
using UnityEngine;
using Zenject;
#if UNITY_IOS && !UNITY_EDITOR
using System.Linq;
using UnityEngine.XR.ARKit;
using ARKit;
#endif

namespace AppInitialization
{
    internal sealed class AppInitializer: MonoBehaviour
    {
        [SerializeField] private UiInitializer _uiInitializer;

        [Inject] private IDeviceAudioOutputControl _audioOutputControl;
        [Inject] private IRenderingPipelineManager _renderingPipelineManager;
        [Inject] private SentryManager _sentryManager;
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] private IAppEventsSource _appEventsSource;
        [Inject] private IFrameRateControl _frameRateControl;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Start()
        {
            _audioOutputControl.Run();
            _uiInitializer.Run();
            _renderingPipelineManager.Init();

            var pipelineType = (AppSettings.UseOptimizedRenderingScale)
                ? PipelineType.Default
                : PipelineType.HighQuality;

            _renderingPipelineManager.SetPipeline(pipelineType);

            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            QualitySettings.vSyncCount = 0;
            //This line fixes blend-shapes for characters
            QualitySettings.skinWeights = SkinWeights.FourBones;

        #if UNITY_IOS && !UNITY_EDITOR
            var instances = new List<ARKitSessionSubsystem>();
            
            SubsystemManager.GetInstances( instances );

            if (instances.Count > 0)
            {
                var instance = instances.First();

                instance.sessionDelegate = new FreverARKitSessionDelegate(() =>
                {
                    _popupManagerHelper.ShowAlertPopup("ARKit encountered an error. Please restart the device to continue.", "Restart the device", "Quit", Application.Quit);
                });
            }
        #endif

            InitializeSentry();

            _frameRateControl.Initialize();
        }

        private void InitializeSentry()
        {
            try
            {
                _sentryManager.Reinitialize();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
    }
}