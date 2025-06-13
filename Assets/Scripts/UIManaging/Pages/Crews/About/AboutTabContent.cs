using System.Threading;
using Bridge.Models.ClientServer.Crews;
using Extensions;
using UIManaging.Common;
using UnityEngine;

namespace UIManaging.Pages.Crews
{
    internal sealed class AboutTabContent : CrewTabContent
    {
        [SerializeField] private CrewInfoView _crewInfo;
        [SerializeField] private CrewMembersView _crewMembers;
        [SerializeField] private CrewBackgroundView _crewBackground;
        [SerializeField] private FollowerMembersView _followerMembersView;

        private CancellationTokenSource _cancellationTokenSource;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnDisable()
        {
            _cancellationTokenSource?.CancelAndDispose();
            _cancellationTokenSource = null;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void InitFollowerMembers(CrewShortInfo crewShortInfo)
        {
            _followerMembersView.Initialize(new FollowerMembersModel
            {
                Members = crewShortInfo.Members,
                FollowersCount = crewShortInfo.FollowersCount,
                FollowingCount = crewShortInfo.FollowingCount,
                FriendsCount = crewShortInfo.FriendsCount
            });
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _crewInfo.Initialize(new CrewInfoModel(ContextData));
            _crewMembers.Initialize(new CrewMembersModel(ContextData));
            _crewBackground.Initialize(ContextData);
        }
        
        protected override void BeforeCleanup()
        {
            _followerMembersView.SetActive(false);
            _crewMembers.CleanUp();
        }
    }
}