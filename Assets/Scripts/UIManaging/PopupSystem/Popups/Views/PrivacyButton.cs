using System;
using System.Collections.Generic;
using Bridge.Models.ClientServer;
using Bridge.Models.VideoServer;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using Utils;

namespace UIManaging.PopupSystem.Popups.Views
{
    public sealed class PrivacyButton: BasePrivacyButton<VideoAccess>
    {
        [SerializeField] private bool _reopenOnSave = true;
        
        protected override string ToText(VideoAccess access)
        {
            if (access == VideoAccess.ForTaggedGroups)
            {
                return SelectedUsers.Count > 0
                    ? $"Selected friends: {SelectedUsers.Count} >"
                    : "Selected friends";
            }
            
            return access.ToText();
        }

        public List<GroupShortInfo> SelectedUsers { get; set; } = new List<GroupShortInfo>();
        public List<GroupShortInfo> TaggedUsers { get; set; } = new List<GroupShortInfo>();
        public Action<PrivacyPopupResult> SelectedCallback { get; set; }

        protected override BasePrivacyPopupConfiguration<VideoAccess> GetPopupConfiguration()
        {
            var config = new PrivacyPopupConfiguration(Access, SelectedUsers, TaggedUsers, _reopenOnSave, result =>
            {
                Access = result.Access;
                SelectedUsers = result.SelectedProfiles;
                SelectedCallback?.Invoke(result);
            });

            return config;
        }
    }
}