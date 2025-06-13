using System;

namespace UIManaging.PopupSystem.Configurations
{
    public sealed class LoadingIndicatorPopupConfiguration : PopupConfiguration
    {
        public event Action OnHideRequested;
        public LoadingIndicatorPopupConfiguration() : base(PopupType.LoadingIndicator, null)
        {
            
        }

        public void Hide()
        {
            OnHideRequested?.Invoke();
        }

        public void ClearEvents()
        {
            OnHideRequested = null;
        }
    }
}