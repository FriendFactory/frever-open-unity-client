using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(fileName = "BoneColliderSettings.asset", menuName = "Friend Factory/Configs/Bone collider settings", order = 4)]
    public class BoneColliderSettings : ScriptableObject
    {
        [SerializeField] 
        public long RaceId = 1;
        [SerializeField]
        private BoneColliderSetting[] _boneColliderSettings;

        public IEnumerable<BoneColliderSetting> BoneColliders => _boneColliderSettings;

        [Serializable]
        public class BoneColliderSetting
        {
            public string BoneName;
            public Vector3 PossitionOffset;
            public float Radius;
            public float Height;
        }
    }
}