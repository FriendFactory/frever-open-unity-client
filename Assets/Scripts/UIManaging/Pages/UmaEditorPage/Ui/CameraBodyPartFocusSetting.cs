using System;
using Modules.PhotoBooth.Character;

namespace UIManaging.Pages.LevelEditor.Ui.Wardrobe
{
	[Serializable]
	internal struct CameraBodyPartFocusSetting
	{
		public BodyDisplayMode DisplayMode;
		public string BoneName;
		public float Zoom;
		public float AxisX;
		public float AxisY;
		public float OrbitRadius;
		public bool ForWholeCategory;
	}
}