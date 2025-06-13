using System;
using System.Collections.Generic;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(fileName ="TierSettings.asset", menuName ="Friend Factory/Configs/Tier settings", order =4)]
    public class TierSettings : ScriptableObject
    {
        [SerializeField]
        private List<TierSetting> _tierSettings = new List<TierSetting>();

        public IEnumerable<TierSetting> Settings => _tierSettings;

        [Serializable]
        public class TierSetting
        {
            public long TierId;
            public Sprite TierSprite;
        }
    }
}
