using System.Collections.Generic;
using UIManaging.PopupSystem.Popups;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UIManaging.PopupSystem.Configurations
{
    [CreateAssetMenu(fileName = "PopupPresetsConfig.asset", menuName = "Friend Factory/Popup System/Popup Presets Config", order = 4)]
    public class PopupPresetsConfig : ScriptableObject
    {
        [SerializeField]
        private List<BasePopup> _popupPresets = new List<BasePopup>();

        public BasePopup GetPresetByType(PopupType type)
        {
            return _popupPresets.Find((popup) => popup.Type == type);
        }
    }
}