using System;
using Bridge.Models.ClientServer.StartPack.Metadata;

namespace UIManaging.PopupSystem.Configurations
{
    public class IPSelectionPopupConfiguration : PopupConfiguration
    {
        public Action<Universe> OnUseClicked { get; }
        public Action OnCancel { get; }
        public Universe[] Universes { get; }

        public IPSelectionPopupConfiguration(Universe[] universes, Action<Universe> onUse, Action onCancel = null)
        {
            PopupType = PopupType.IPSelection;
            Universes = universes;
            OnUseClicked = onUse;
            OnCancel = onCancel;
        }
    }
}
