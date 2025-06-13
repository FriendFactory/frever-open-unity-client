using System;
using Bridge.Models.ClientServer;

namespace UIManaging.PopupSystem.Configurations
{
    public sealed class ChatUserInfoPopupConfiguration : PopupConfiguration
    {
        public GroupShortInfo UserInfo { get; }

        public Action<bool> HideAction { get; }

        public ChatUserInfoPopupConfiguration(GroupShortInfo userInfo, Action<bool> hideAction) 
            : base(PopupType.ChatUserInfo, null)
        {
            UserInfo = userInfo;
            HideAction = hideAction;
        }
    }
}