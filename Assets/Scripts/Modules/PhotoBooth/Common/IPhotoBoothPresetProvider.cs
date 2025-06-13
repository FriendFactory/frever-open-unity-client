using System;

namespace Modules.PhotoBooth
{
	internal interface IPhotoBoothPresetProvider<in TMode, TPreset> where TMode: Enum
	{
		bool TryGetPreset(TMode key, out TPreset preset);
		bool TryGetPreset(long raceId, TMode key, out TPreset preset);
	}
}