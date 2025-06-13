using System;
using System.Collections.Generic;
using System.Linq;
using Bridge;
using Bridge.Models.ClientServer.Battles;
using UIManaging.Common.Interfaces;
using UIManaging.Pages.VotingFeed.Interfaces;
using UnityEngine;

namespace UIManaging.Pages.VotingFeed
{
    public class VotingFeedModel: IVotingFeedModel
    {
        public event Action VotingCompleted;

        public int MaxIterations => AllBattleData?.Count ?? 0;
        public int CurrentIteration { get; private set; }

        public BattleData CurrentBattleData => CurrentIteration <= 0  || CurrentIteration > AllBattleData.Count 
            ? null : AllBattleData[CurrentIteration - 1];
        public List<BattleData> AllBattleData { get; }

        private readonly IBridge _bridge;
        private readonly List<BattleVoteModel> _votedVideos = new List<BattleVoteModel>();
        private readonly long _taskId;

        public VotingFeedModel(long taskId, List<BattleData> allBattleData, IBridge bridge)
        {
            _taskId = taskId;
            AllBattleData = allBattleData;
            _bridge = bridge;
        }
        
        public void StartNextVote()
        {
            if (CurrentIteration >= MaxIterations)
            {
                Debug.LogError("Trying to start next vote when all votes have passed");
                return;
            }

            CurrentIteration++;

            var allLoaded = CurrentBattleData.BattleVideos.All(battleData => battleData.VideoModel.IsLoaded);
            
            foreach (var battleData in CurrentBattleData.BattleVideos)
            {
                if (allLoaded)
                {
                    battleData.VideoModel.StartVideo();
                }
                else
                {
                    battleData.VideoModel.VideoLoaded += OnVideoLoaded;
                }
            }
        }

        public void VoteForVideo(long videoId)
        {
            _votedVideos.Add(new BattleVoteModel { BattleId = CurrentBattleData.BattleId, VotedVideoId = videoId });

            if (_votedVideos.Count == MaxIterations)
            {
                _bridge.Vote(_taskId, _votedVideos);
            }
        }

        public void FinishVoting()
        {
            if (_votedVideos.Count != MaxIterations)
            {
                Debug.LogError("Voting finished prematurely");
                return;
            }
            
            VotingCompleted?.Invoke();
        }

        private void OnVideoLoaded()
        {
            if (CurrentBattleData.BattleVideos.All(battleData => battleData.VideoModel.IsLoaded))
            {
                foreach (var battleData in CurrentBattleData.BattleVideos)
                {
                    battleData.VideoModel.VideoLoaded -= OnVideoLoaded;
                    battleData.VideoModel.StartVideo();
                }
            }
        }
    }
}