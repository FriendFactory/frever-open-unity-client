using System;
using System.Collections.Generic;

namespace UIManaging.PopupSystem.Configurations
{
    public class ConfirmAddMembersPopupConfiguration : PopupConfiguration
    {
        public Action OnCreate { get; }
        public Action OnAdd { get; }

        public ConfirmAddMembersPopupConfiguration(Action onCreate, Action onAdd) : base(PopupType.ConfirmAddMembersPopup, null, "")
        {
            OnCreate = onCreate;
            OnAdd = onAdd;
        }
    }
}
