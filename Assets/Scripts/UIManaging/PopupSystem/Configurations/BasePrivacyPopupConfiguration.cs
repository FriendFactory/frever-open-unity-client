using System;

namespace UIManaging.PopupSystem.Configurations
{
    public abstract class BasePrivacyPopupConfiguration<T> : PopupConfiguration where T: Enum
    {
        public T CurrentAccess { get; }

        protected BasePrivacyPopupConfiguration(T access, PopupType type, Action<object> onClose = null) : base(
            type, onClose, "") 
        {
            CurrentAccess = access;
        }
    }
}