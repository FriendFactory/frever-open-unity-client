using System.Collections.Generic;
using Bridge.Models.ClientServer;
using Bridge.Models.VideoServer;
using Navigation.Args;
using Navigation.Core;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups.Views;
using Zenject;

namespace UIManaging.PopupSystem.Popups
{
    internal sealed class PrivacyPopup : BasePrivacyPopup<VideoAccess, PrivacyToggle, PrivacyPopupConfiguration>
    {
        [Inject] private PageManager _pageManager;
        [Inject] private PopupManager _popupManager;
        
        public int SelectedProfilesCount => Configs.SelectedUsers?.Count ?? 0;

        private void SelectResult()
        {
            Configs.SelectedCallback?.Invoke(new PrivacyPopupResult
            {
                Access = SelectedOption, 
                SelectedProfiles = Configs.SelectedUsers
            });
        }
        
        protected override void OnValueSelected(VideoAccess access)
        {
            base.OnValueSelected(access);
            
            if (access == VideoAccess.ForTaggedGroups)
            {
                var configs = Configs;
                
                var args = new UserSelectionPageArgs
                {
                    Filter = UserSelectionPageArgs.UsersFilter.Friends,
                    SelectedProfiles = Configs.SelectedUsers,
                    LockedProfiles = Configs.TaggedUsers,
                    OnBackButton = () =>
                    {
                        _pageManager.MoveBack();
                        _popupManager.SetupPopup(configs);
                        _popupManager.ShowPopup(PopupType.PrivacyPopup);
                    },
                    OnSaveButton = selectedProfiles =>
                    {
                        Configs.SelectedUsers.Clear();
                        Configs.SelectedUsers.AddRange(selectedProfiles);
                        SelectResult();
                        
                        _pageManager.MoveBack();
                        
                        if (!configs.ReopenOnSave) return;
                        
                        _popupManager.SetupPopup(new PrivacyPopupConfiguration(VideoAccess.ForTaggedGroups, Configs.SelectedUsers, configs.TaggedUsers, configs.ReopenOnSave, configs.SelectedCallback));
                        _popupManager.ShowPopup(PopupType.PrivacyPopup);
                    }
                };
                _pageManager.MoveNext(args);
            }
            else
            {
                SelectResult();
            }
        }
    }

    public class PrivacyPopupResult
    {
        public VideoAccess Access;
        public List<GroupShortInfo> SelectedProfiles;
    }
}