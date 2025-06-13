using System;

namespace UIManaging.PopupSystem.Configurations
{
    public class QuestPopupConfiguration : PopupConfiguration
    {
        public Action OnHidingBegin { get; }

        public QuestPopupConfiguration(Action onHidingBegin = null) : base(PopupType.Quest, null, "")
        {
            OnHidingBegin = onHidingBegin;
        }
    }
}