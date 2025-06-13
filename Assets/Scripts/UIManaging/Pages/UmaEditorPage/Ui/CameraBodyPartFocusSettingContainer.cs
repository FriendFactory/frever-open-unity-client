using System;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.Wardrobe
{
	[Serializable]
	internal sealed class CameraBodyPartFocusSettingContainer
	{
		internal int RaceId => _raceId;
		internal CameraBodyPartFocusSetting[] Settings => _settings;

		[SerializeField] private int _raceId;
		[SerializeField] private CameraBodyPartFocusSetting[] _settings;
	}
}