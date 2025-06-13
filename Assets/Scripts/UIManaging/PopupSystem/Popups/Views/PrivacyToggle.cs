using Bridge.Models.VideoServer;
using UnityEngine;
using Utils;

namespace UIManaging.PopupSystem.Popups.Views
{
    internal class PrivacyToggle: PrivacyToggleBase<VideoAccess>
    {
        public override void SetValue(VideoAccess access)
        {
            targetToggle.isOn = targetAccess == access;
        }

        protected override string ToText() => targetAccess.ToText();
    }
}