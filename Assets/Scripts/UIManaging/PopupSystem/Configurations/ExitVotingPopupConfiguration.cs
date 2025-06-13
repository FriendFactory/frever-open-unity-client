using System;

namespace UIManaging.PopupSystem.Configurations
{
    public sealed class ExitVotingPopupConfiguration: PopupConfiguration
    {
        public Action ExitConfirmed;

        public ExitVotingPopupConfiguration(): base(PopupType.ExitVotingFeedPopup, null, null)
        {
        }
    }
}