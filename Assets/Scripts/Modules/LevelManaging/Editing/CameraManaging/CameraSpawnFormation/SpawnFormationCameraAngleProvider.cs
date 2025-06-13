using System.Collections.Generic;
using System.Linq;
using Modules.LevelManaging.Editing.AssetChangers.SpawnFormations;
using UnityEngine;

namespace Modules.LevelManaging.Editing.CameraManaging.CameraSpawnFormation
{
    internal interface ISpawnFormationCameraAngleProvider
    {
        float GetAngle(long spawnFormationId, int characterSequenceNumber);
    }

    internal sealed class SpawnFormationCameraAngleProvider : ISpawnFormationCameraAngleProvider
    {
        private readonly float _defaultAngle;
        
        private readonly IReadOnlyCollection<SpawnFormationCameraData> _setups;

        public SpawnFormationCameraAngleProvider(IReadOnlyCollection<SpawnFormationCameraData> setups, float defaultAngle)
        {
            _setups = setups;
            _defaultAngle = defaultAngle;
        }

        public float GetAngle(long spawnFormationId, int characterSequenceNumber)
        {
            var setup = _setups.FirstOrDefault(x =>
                x.FormationType.GetId() == spawnFormationId && x.CameraSettings.Any(_=>_.CharacterSequenceNumber == characterSequenceNumber));

            if (setup == null) return _defaultAngle;
            
            var cameraSettings = setup.CameraSettings.First(x => x.CharacterSequenceNumber == characterSequenceNumber);
            return cameraSettings.ViewAngle * Mathf.Sign((int) cameraSettings.ViewDirection);
        }
    }
}