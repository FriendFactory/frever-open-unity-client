using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Configs
{
    [CreateAssetMenu(fileName = "DNASlidersGroupingSettings.asset", menuName = "Friend Factory/Configs/DNA Sliders grouping settings", order = 4)]
    public class DNASlidersGroupingSettings : ScriptableObject
    {
        [SerializeField]
        private List<DNASliderGroupingSetting> _groupingSettings = new List<DNASliderGroupingSetting>();

        public IEnumerable<DNASliderGroupingSetting> GroupingSettings => _groupingSettings;

        [Serializable]
        public class DNASliderGroupingSetting
        {
            public string Group;
            public string GroupKey;
            public string HighlightingItemName;
            public int SortOrder = 1;
            public List<DNASliderSetting> DNASliderSettings = new List<DNASliderSetting>();

            public int GetId()
            {
                return GroupKey.GetHashCode();
            }
        }

        [Serializable]
        public class DNASliderSetting
        {
            public string DNAKey;
            public string NewName;
            public int SortOrder;
        }
    }
}
