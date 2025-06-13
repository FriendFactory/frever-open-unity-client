using UIManaging.Common.Args.Views.Profile;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups.Share
{
    internal sealed class ShareSelectionFriendsSelectedItem: ShareSelectionSelectedItem<ShareSelectionFriendsItemModel>
    {
        [SerializeField] private UserPortraitView _userPortraitView;

        protected override string Title => ContextData.Title;

        protected override void RefreshPortraitImage()
        {
            _userPortraitView.InitializeFromProfile(ContextData.Profile);
        }

        protected override void BeforeCleanup()
        {
            _userPortraitView.CleanUp();
            
            base.BeforeCleanup();
        }
    }
}