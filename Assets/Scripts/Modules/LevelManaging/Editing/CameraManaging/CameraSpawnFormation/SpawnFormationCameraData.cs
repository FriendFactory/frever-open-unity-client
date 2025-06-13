using System;
using System.Collections.Generic;
using Modules.LevelManaging.Editing.AssetChangers.SpawnFormations;

namespace Modules.LevelManaging.Editing.CameraManaging.CameraSpawnFormation
{
    [Serializable]
    internal sealed class SpawnFormationCameraData
    {
        public FormationType FormationType;
        public List<CameraSettings> CameraSettings;
    }

    [Serializable]
    internal struct CameraSettings
    {
        public long CharacterSequenceNumber;
        public float ViewAngle;
        public CameraViewDirection ViewDirection;
    }
    
    internal enum CameraViewDirection
    {
        FromRight = -1,
        FromLeft = 1
    }
}