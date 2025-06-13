using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_IOS
using UnityEngine.iOS;
using static System.String;
#endif
using UnityEngine.Rendering.Universal;

namespace Modules.RenderingPipelineManagement.iOS
{
    [CreateAssetMenu(menuName = "Friend Factory/Rendering Pipeline Profile/iOS", fileName = "iOS Rendering Pipeline Profile")]
    internal sealed class RenderingPipelineProfile: ScriptableObject
    {
        [SerializeField] private PipelinesData _defaultPipelines;
        [SerializeField] private PipelinesData _newUnknownDevicePipeline;
        [SerializeField] private List<DevicesPipelineConfig> _configs;
        
        private PipelinesData _editablePipeline;

        #if UNITY_IOS
        public DeviceGeneration CurrentDeviceGeneration {get;set;}

        private readonly DeviceGeneration[] _unknownDevices =
            {
                DeviceGeneration.Unknown,
                DeviceGeneration.iPadUnknown,
                DeviceGeneration.iPhoneUnknown
            };
        #endif


        public PipelinesData GetPipelines()
        {
            #if UNITY_IOS
                PipelinesData pipelinesData;

                if (_unknownDevices.Contains(CurrentDeviceGeneration))
                {
                    pipelinesData =  _newUnknownDevicePipeline;
                }
                else
                {
                    var specialConfig = _configs.FirstOrDefault(x => x.DeviceGenerations.Contains(CurrentDeviceGeneration));
                    pipelinesData = specialConfig?.PipelinesData ?? _defaultPipelines;
                }

                _editablePipeline.OptimizedPreviewPipeline = Instantiate(pipelinesData.OptimizedPreviewPipeline);
                _editablePipeline.HighQualityVideoRenderPipeline = Instantiate(pipelinesData.HighQualityVideoRenderPipeline);
                return _editablePipeline;
            #else
	            // ANDROID POC: Currently the default values are used for Android
	            PipelinesData pipelinesData = _defaultPipelines;
	            _editablePipeline.OptimizedPreviewPipeline = Instantiate(pipelinesData.OptimizedPreviewPipeline);
	            _editablePipeline.HighQualityVideoRenderPipeline = Instantiate(pipelinesData.HighQualityVideoRenderPipeline);
	            return _editablePipeline;
            #endif
        }

        
        #if UNITY_EDITOR
        private void OnValidate()
        {
            if(_configs==null) return;
            #if UNITY_IOS
                //prevent duplication of device generation in the same config
                foreach (var config in _configs)
                {

                    if(config.DeviceGenerations==null) continue;
                    config.DeviceGenerations = config.DeviceGenerations.Distinct().ToArray();
                }

                //check if the same device generation is not included to 2 different configs
                var repeatedGenerations = _configs.Where(x => x.DeviceGenerations != null)
                    .SelectMany(x => x.DeviceGenerations).GroupBy(x=>x).Where(x=>x.Count()>1).ToArray();

                if(!repeatedGenerations.Any()) return;

                foreach (var repeatedGeneration in repeatedGenerations)
                {
                    var errorText = $"Generation {repeatedGeneration.Key} is included in multiple configs, what is not allowed";
                    Debug.LogError(errorText);
                    UnityEditor.EditorUtility.DisplayDialogComplex("Error", errorText, "Got It", Empty, Empty);
                }
            #endif
        }
        #endif
    }

    [Serializable]
    internal sealed class DevicesPipelineConfig
    {
        public PipelinesData PipelinesData; 

        #if UNITY_IOS
        public DeviceGeneration[] DeviceGenerations;
        #endif
    }

    [Serializable]
    internal struct PipelinesData
    {
        public UniversalRenderPipelineAsset OptimizedPreviewPipeline;
        public UniversalRenderPipelineAsset HighQualityVideoRenderPipeline;
    }
}