using System;

namespace UIManaging.PopupSystem.Configurations
{
    public abstract class PopupConfiguration
    {
        public PopupType PopupType { get; set; }

        public string Title { get; set; }

        public Action<object> OnClose;

        protected PopupConfiguration() { }

        protected PopupConfiguration(PopupType type, Action<object> onClose, string title = "")
        {
            PopupType = type;
            Title = title;
            OnClose = onClose;
        }
    }
}