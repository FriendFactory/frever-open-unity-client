using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modules.PhotoBooth.Character
{
	internal abstract class PhotoBoothPresetProvider<TMode, TPreset, TPresetModel> : MonoBehaviour,
		IPhotoBoothPresetProvider<TMode, TPreset>
		where TMode : Enum
		where TPresetModel : PresetModel<TMode, TPreset>
	{
		[SerializeField] private List<RaceSpecificPresets<TPresetModel>> _racePresets;

		private Dictionary<(long RaceId, TMode Mode), TPreset> _presetsMap;

		protected virtual void Awake()
		{
			GeneratePresetsMap();
		}

		public virtual bool TryGetPreset(long raceId, TMode key, out TPreset preset)
		{
			preset = default;
			var lookupKey = (raceId, key);
			if (!_presetsMap.ContainsKey(lookupKey)) return false;

			preset = _presetsMap[lookupKey];
			return true;
		}

		public virtual bool TryGetPreset(TMode key, out TPreset preset)
		{
			// fallback with the default race ID=1
			return TryGetPreset(1, key, out preset);
		}

		private void GeneratePresetsMap()
		{
			_presetsMap = new Dictionary<(long RaceId, TMode Mode), TPreset>();
            
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