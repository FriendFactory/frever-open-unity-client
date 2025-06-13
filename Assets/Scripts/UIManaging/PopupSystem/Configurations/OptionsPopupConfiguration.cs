using System;
using System.Collections.Generic;

namespace UIManaging.PopupSystem.Configurations
{
    public sealed class OptionsPopupConfiguration : PopupConfiguration
    {
        public List<Option> Options = new List<Option>();

        public OptionsPopupConfiguration(): base(PopupType.OptionsPopup, null, null)
        {
        }

        public void AddOption(Option option)
        {
            Options.Add(option);
        }
    }
    
    public struct Option
    {
        public string Name;
        public Action OnSelected;
        public OptionColor Color;
    }
    
    public enum OptionColor
    {
        Default,
        Red
    }
}