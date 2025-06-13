using System;
using Bridge.Models.ClientServer.Assets;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups.AiGeneration
{
    public class AiGridPopupConfiguration : PopupConfiguration
    {
        public Sprite Logo { get; }
        public SetLocationBackgroundSettings Entity { get; }
        public BackgroundOptionsSet[] OptionSets { get; }
        public Action<string[]> ConfirmAction { get; }

        public AiGridPopupConfiguration(Sprite logo, BackgroundOptionsSet[] optionSets, Action<string[]> confirmAction) : base(PopupType.AiGridPopup, null)
        {
            Logo = logo;
            Entity = null;
            OptionSets = optionSets;
            ConfirmAction = confirmAction;
        }

        public AiGridPopupConfiguration(SetLocationBackgroundSettings entity, Action<string[]> confirmAction) : base(PopupType.AiGridPopup, null)
        {
            Logo = null;
            Entity = entity;
            OptionSets = entity.Settings.Sets;
            ConfirmAction = confirmAction;
        }
    }
}