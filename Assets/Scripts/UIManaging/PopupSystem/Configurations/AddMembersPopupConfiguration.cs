using System;
using System.Collections.Generic;

namespace UIManaging.PopupSystem.Configurations
{
    public class AddMembersPopupConfiguration : PopupConfiguration
    {
        public long ChatId { get; }
        public IReadOnlyList<long> MemberIds { get; }
        public Action<long> OnSuccess { get; }

        public AddMembersPopupConfiguration(long chatId, IReadOnlyList<long> memberIds, Action<long> onSuccess) : base(PopupType.AddMembersPopup, null, "")
        {
            ChatId = chatId;
            MemberIds = memberIds;
            OnSuccess = onSuccess;
        }
    }
}
