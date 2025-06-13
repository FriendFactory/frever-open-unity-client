using System;
using System.Collections.Generic;

namespace UIManaging.PopupSystem.Configurations
{
    public sealed class DarkActionPopupConfiguration : PopupConfiguration
    {
        public List<KeyValuePair<string, Action>> Variants = new List<KeyValuePair<string, Action>>();
    }
}
