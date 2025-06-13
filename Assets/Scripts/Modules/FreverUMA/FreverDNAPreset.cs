using System;
using UnityEngine;
using UMA;

namespace Modules.FreverUMA 
{
    [CreateAssetMenu(menuName = "Frever UMA/DNA Preset", fileName = "New Frever DNA Preset.asset")]
    public class FreverDNAPreset : ScriptableObject
    {
        public Sprite thumbnail;
        public DynamicUMADnaAsset _presetType;
        public bool b_raceDependent = false;
        public Preset[] _presets;

        public Preset[] GetPresets(string race = null) 
        {
            var racePresets = new Preset[_presets.Length / 2];
            var startIndex = 0;
            if (b_raceDependent)
            {
                startIndex = race == "Male Base" ? 0 : _presets.Length / 2 - 1;
            }
            for(var i = 0; i < _presets.Length / 2; i++) 
            {
                racePresets[i] = _presets[startIndex + i];
            }
            
            return racePresets;
        }
    }

    [Serializable]
    public class Preset 
    {
        public string name;
        [SerializeField]
        public float value;
    }
}

