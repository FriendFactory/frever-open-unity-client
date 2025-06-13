using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.StartPack.Metadata;
using JetBrains.Annotations;
using Modules.CameraSystem.CameraSettingsHandling;
using Modules.CameraSystem.CameraSystemCore;

namespace Modules.LevelManaging.Editing.CameraManaging.CameraSettingsManaging
{
    [UsedImplicitly]
    internal sealed class CameraSettingProvider
    {
        private List<CameraSetting> _cameraSettings;
        private bool IsInitialized => _cameraSettings != null;
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Initialize(ICollection<SpawnPositionSpaceSize> spawnPositionSpaceSizes)
        {
            if (IsInitialized) return;
            CreateCameraSettings(spawnPositionSpaceSizes);
        }

        public CameraSetting GetSettingWithSpawnPositionId(long spawnPositionSpaceSizeId)
        {
            return _cameraSettings.First(setting => setting.Id == spawnPositionSpaceSizeId);
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void CreateCameraSettings(ICollection<SpawnPositionSpaceSize> spawnPositionSizes)
        {
            _cameraSettings = new List<CameraSetting>();

            foreach (var size in spawnPositionSizes)
            {
                var cameraSetting = new CameraSetting()
                {
                    Id = size.Id,
                    OrbitRadiusMax = size.MaxDistance / 1000f,
                    OrbitRadiusMaxDefault = size.MaxDistance / 1000f,
                    OrbitRadiusMin = size.MinDistance / 1000f,
                    OrbiRadiusStart = size.StartDistance / 1000f,
                    FoVStart = size.StartFoV / 1000f,
                    FoVMax = size.MaxFoV / 1000f,
                    FoVMin = size.MinFoV / 1000f,
                    DepthOfFieldMax = CameraSystemConstants.FreeLookCamera.DOF_MAX,
                    DepthOfFieldMin = CameraSystemConstants.FreeLookCamera.DOF_MIN,
                    DepthOfFieldStart = CameraSystemConstants.FreeLookCamera.DOF_DEFAULT
                };
                _cameraSettings.Add(cameraSetting);
            }
        }
    }
}
