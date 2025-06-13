using System;
using UnityEngine;

namespace UIManaging.PopupSystem.Configurations
{
    public class DialogDarkPopupConfiguration : InformationPopupConfiguration
    {
        public Action OnYes;
        public Action OnNo;
        public string YesButtonText;
        public string NoButtonText;
        public bool YesButtonSetTextColorRed;
        public Color32? CustomBackgroundColor;
    }

    public sealed class StashEditorChangesPopupConfigs : DialogDarkPopupConfiguration
    {
        private static readonly Color32 BACKGROUND_COLOR =  new Color32(98, 42, 205, 255);
        
        public StashEditorChangesPopupConfigs()
        {
            PopupType = PopupType.StashChangesBeforeExitEditorPopup;
            CustomBackgroundColor = BACKGROUND_COLOR;
        }
    }
}