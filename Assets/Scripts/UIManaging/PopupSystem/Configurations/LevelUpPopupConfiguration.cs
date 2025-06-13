using System;
using System.Collections.Generic;
using Bridge.Models.ClientServer.Gamification;

namespace UIManaging.PopupSystem.Configurations
{
    public sealed class LevelUpPopupConfiguration : PopupConfiguration
    {
        public LevelUpPopupConfiguration(int previousLevel, int newLevel, Action<object> onClose)
            :base(PopupType.LevelUpPopup, onClose, string.Empty)
        {
            PreviousLevel = previousLevel;
            NewLevel = newLevel;
        }

        public int PreviousLevel { get; }
        public int NewLevel { get; }
    }
}