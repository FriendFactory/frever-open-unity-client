using System;
using Bridge.Models.ClientServer.Crews;

namespace UIManaging.PopupSystem.Configurations
{
    public sealed class CrewSidebarPopupConfiguration : PopupConfiguration
    {
        public readonly Action OnSlideOutStarted;
        public readonly CrewModel Model;
        public readonly bool ShowRequests;

        public CrewSidebarPopupConfiguration(CrewModel model, Action onSlideOutStarted, bool showRequests = false)
            : base(PopupType.CrewSidebar, null)
        {
            OnSlideOutStarted = onSlideOutStarted;
            Model = model;
            ShowRequests = showRequests;
        }
    }
}