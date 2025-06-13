using Bridge;
using Bridge.Models.VideoServer;
using Navigation.Core;
using UIManaging.Common;
using UIManaging.Common.Args.Views.LastLevelsPanelArgs;
using UIManaging.Pages.Common.VideoManagement;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.VotingResult
{
    internal sealed class VotingVideoListControl: MonoBehaviour
    {
        [SerializeField] private VideoList _videoList;

        [Inject] private IBridge _bridge;
        [Inject] private VideoManager _videoManager;
        [Inject] private PageManager _pageManager;
        [Inject] private IVotingBattleResultManager _votingBattleResultManager;
        
        public void Initialize(long taskId, Video[] videos)
        {
            var videoProvider = new VotingTaskVideoListLoader(taskId, _votingBattleResultManager, _videoManager, 
                                                              _pageManager, _bridge, _bridge.Profile.GroupId, videos);
            _videoList.Initialize(videoProvider);
        }
    }
}