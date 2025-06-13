using System.Collections.Generic;
using UnityEngine;

namespace Modules.LevelManaging.Editing.CameraManaging.CameraSpawnFormation
{
    [CreateAssetMenu(menuName = "ScriptableObject/SpawnFormationAngleSetup", fileName = "Spawn Formation Angles Setup")]
    internal sealed class SpawnFormationCameraSetup: ScriptableObject
    {
        [Tooltip("Will be applied for SpawnFormation-CharacterSequenceNumber cases, not defined in custom setup list below")]
        public float DefaultAngle;
      
        public List<SpawnFormationCameraData> FormationSetups;
    }
}