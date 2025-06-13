using System;
using System.Collections.Generic;
using Bridge.Services.UserProfile;
using UIManaging.PopupSystem.Configurations;

namespace UIManaging.PopupSystem.Popups.SwipeToFollow
{
    public sealed class SwipeToFollowPopupConfiguration : PopupConfiguration
    {
        public List<Profile> Profiles { get; }

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public SwipeToFollowPopupConfiguration(List<Profile> profiles, Action<object> onClose = null)
            : base(PopupType.SwipeToFollowPopup, onClose)
        {
            Profiles = profiles;
        }
    }
}