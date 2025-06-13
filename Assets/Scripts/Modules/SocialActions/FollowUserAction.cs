using Bridge;
using Navigation.Args;
using Navigation.Core;
using UIManaging.SnackBarSystem;
using UnityEngine;

namespace Modules.SocialActions
{
    internal sealed class FollowUserAction : ISocialAction
    {
        private readonly IBridge _bridge;
        private readonly SnackBarHelper _snackBarHelper;
        private readonly PageManager _pageManager;
        
        private readonly long _groupId;
        private readonly string _nickname;
        
        public FollowUserAction(long groupId, string nickname, IBridge bridge, SnackBarHelper snackBarHelper, PageManager pageManager)
        {
            _bridge = bridge;
            _snackBarHelper = snackBarHelper;
            _pageManager = pageManager;
            
            _groupId = groupId;
            _nickname = nickname;
        }
        
        public async void Execute()
        {
            await _bridge.StartFollow(_groupId);
            _snackBarHelper.ShowInviterFollowedSnackBar(_nickname, OnClick);

            void OnClick()
            {
                var args = new UserProfileArgs(_groupId, _nickname);
                _pageManager.MoveNext(args);
            }
        }
    }
}