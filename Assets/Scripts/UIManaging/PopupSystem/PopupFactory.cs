using System.Collections.Generic;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups;
using UnityEngine;

namespace UIManaging.PopupSystem
{
    internal sealed class PopupFactory : MonoBehaviour
    {
        [SerializeField]
        private PopupPresetsConfig _popupPresetsConfig;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public BasePopup CreatePopup(PopupConfiguration configuration, Transform parent = null)
        {
            var preset = GetPresetByType(configuration.PopupType);
            if (preset == null)
            {
                Debug.LogError($"Popup with {configuration.PopupType} doesn't have a preset");
                return null;
            }

            var popup = Instantiate(preset, parent ? parent : transform, false);

            return popup;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private BasePopup GetPresetByType(PopupType type)
        {
            return _popupPresetsConfig.GetPresetByType(type);
        }
    }
}