using System;
using System.Collections.Generic;

namespace Modules.PhotoBooth.Character
{
	[Serializable]
	internal class RaceSpecificPresets<TPresetModel>
	{
		public int RaceId;
		public List<TPresetModel> Presets;
	}
}