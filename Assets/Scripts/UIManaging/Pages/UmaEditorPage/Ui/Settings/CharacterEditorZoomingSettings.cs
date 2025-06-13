using System.Collections.Generic;
using UnityEngine;
using Modules.PhotoBooth.Character;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UIManaging.Pages.UmaEditorPage
{
    [CreateAssetMenu(fileName = "CharacterEditorZoomingSettings.asset", menuName = "Friend Factory/Configs/Character Editor Zooming Settings", order = 4)]
    public class CharacterEditorZoomingSettings : ScriptableObject
    {
        [Serializable]
        public class ZoomingSetting
        {
            public long Id;
            public BodyDisplayMode BodyDisplayMode;
            public bool ForWholeCategory;
        }

        [SerializeField]
        public List<ZoomingSetting> ZoomingSettings = new List<ZoomingSetting>();
    }
}
