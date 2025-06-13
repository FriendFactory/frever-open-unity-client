using System.Linq;
using Extensions;
using JetBrains.Annotations;
using Modules.CameraSystem.CameraSystemCore;
using Modules.LevelManaging.Assets;

namespace Modules.LevelManaging.Editing
{
    [UsedImplicitly]
    internal sealed class CameraAnchorManager
    {
        private readonly ICameraSystem _cameraSystem;

        public CameraAnchorManager(ICameraSystem cameraSystem)
        {
            _cameraSystem = cameraSystem;
        }

        public void SetAnchor(ISetLocationAsset setLocationAsset, long characterSpawnPointId)
        {
            var setLocationModel = setLocationAsset.RepresentedModel;
            var spawnPositions = setLocationModel.GetSpawnPositions();
            var spawnPositionGuild = spawnPositions.First(x => x.Id == characterSpawnPointId).UnityGuid;
            var characterSpawnPoint = setLocationAsset.CharacterSpawnPositions.First(x => x.Guid == spawnPositionGuild).transform;
            _cameraSystem.SetCameraAnchor(characterSpawnPoint);
        }
    }
}