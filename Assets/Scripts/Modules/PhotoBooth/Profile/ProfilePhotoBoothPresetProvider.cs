using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modules.PhotoBooth.Profile
{
    [Serializable]
    internal sealed class CharacterPhotoBoothPresetModel : PresetModel<ProfilePhotoType, ProfilePhotoBoothPreset> { }

    [Serializable]
    internal class RaceSpecificProfilePresets
    {
        public long RaceId;
        public List<CharacterPhotoBoothPresetModel> Presets;
    }
    
    [CreateAssetMenu(order = 10, fileName = "Profile Preset Provider", menuName = "ScriptableObjects/PhotoBooth/Profile Preset Provider")]
    public sealed class ProfilePhotoBoothPresetProvider : ScriptableObject,
                                                          IProfilePhotoBoothPresetProvider
    {
        [SerializeField] private List<RaceSpecificProfilePresets> _racePresets;
        
        private Dictionary<(long RaceId, ProfilePhotoType Mode), ProfilePhotoBoothPreset> _presetsMap;

        public bool TryGetPreset(long raceId, ProfilePhotoType key, out ProfilePhotoBoothPreset preset)
        {
            InitializePresetsMapIfNeeded();
            
            preset = default;
            var lookupKey = (raceId, key);
            if (!_presetsMap.ContainsKey(lookupKey)) return false;

            preset = _presetsMap[lookupKey];
            return true;
        }

        public bool TryGetPreset(ProfilePhotoType key, out ProfilePhotoBoothPreset preset)
        {
            return TryGetPreset(1, key, out preset);
        }

        private void InitializePresetsMapIfNeeded()
        {
            if (_presetsMap != null) return;
            
            _presetsMap = new Dictionary<(long RaceId, ProfilePhotoType Mode), ProfilePhotoBoothPreset>();
            
            foreach (var racePreset in _racePresets)
            {
                foreach (var presetModel in racePreset.Presets)
                {
                    _presetsMap.Add((racePreset.RaceId, presetModel.mode), presetModel.preset);
                }
            }
        }
    }
}