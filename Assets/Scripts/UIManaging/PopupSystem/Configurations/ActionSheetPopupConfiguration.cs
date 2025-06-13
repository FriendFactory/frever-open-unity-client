using System;
using System.Collections.Generic;

namespace UIManaging.PopupSystem.Configurations
{
    public class ActionSheetPopupConfiguration : InformationPopupConfiguration
    {
        public int[] MainVariantIndexes = {0};
        public List<KeyValuePair<string, Action>> Variants = new List<KeyValuePair<string, Action>>();
        public string CancelButtonText;
        public Action OnCancel;
    }
}