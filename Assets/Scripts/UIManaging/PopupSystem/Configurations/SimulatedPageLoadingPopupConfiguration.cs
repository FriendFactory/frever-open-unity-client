using System;

namespace UIManaging.PopupSystem.Configurations
{
    public sealed class SimulatedPageLoadingPopupConfiguration : BasePageLoadingPopupConfiguration
    {
        public SimulatedPageLoadingPopupConfiguration(string header, string progressBarText, Action<object> onClose) 
            : base(header, progressBarText, onClose, PopupType.SimulatedPageLoading)
        {
        }

    }
}