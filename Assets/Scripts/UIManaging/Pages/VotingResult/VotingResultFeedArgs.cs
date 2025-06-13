using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bridge.Models.VideoServer;
using Bridge.NotificationServer;
using Navigation.Args.Feed;
using UIManaging.Pages.Common.VideoManagement;
using UnityEngine;

namespace UIManaging.Pages.VotingResult
{
    using Extensions; // DWC-2022.2 upgrade: getting ambiguous warnings with this using statement so moving it here for scope
    public class VotingResultFeedArgs : BaseFeedArgs
    {
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public override string Name => "Tasks";
        public override VideoListType VideoListType => VideoListType.Task;
        public long TaskId { get; }
        private readonly IVotingBattleResultManager _votingBattleResultManager;
        private readonly List<Video> _cachedVideos = new List<Video>();
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public VotingResultFeedArgs(IVotingBattleResultManager votingBattleResultManager, VideoManager videoManager, long taskId) : base(videoManager)
        {
            _votingBattleResultManager = votingBattleResultManager;
            TaskId = taskId;
        }

        public VotingResultFeedArgs(IVotingBattleResultManager votingBattleResultManager, VideoManager videoManager, long idOfFirstVideoToShow, long taskId, CommentInfo commentInfo = null) : base(videoManager, idOfFirstVideoToShow, commentInfo)
        {
            _votingBattleResultManager = votingBattleResultManager;
            TaskId = taskId;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override bool ShouldShowNavigationBar()
        {
            return false;
        }

        public override bool ShouldShowTabs()
        {
            return false;
        }

        public override bool ShouldShowUseBasedOnTemplateButton()
        {
            return true;
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override async void DownloadVideosInternal(Action<Video[]> onSuccess, Action<string> onFail, long? videoId,
                                                       int takeNextCount,
                                                       int takePreviousCount = 0,
                                                       CancellationToken cancellationToken = default)
        {
            if (_cachedVideos.Count == 0)
            {
                var result = await _votingBattleResultManager.GetVotingBattleResult(TaskId, cancellationToken);

                if (result == null || result.Length == 0)
                {
                    Debug.LogError($"No videos found for task id {TaskId}");
                    onFail?.Invoke($"No videos found for task id {TaskId}");
                    return;
                }
                
                _cachedVideos.AddRange(result.OrderByDescending(x => x.Score).Where(x=> x.Video != null).Select(battle => battle.Video));
            }

            if (videoId is null)
            {
                onSuccess?.Invoke(_cachedVideos.Take(takeNextCount).ToArray());
                return;
            }

            var id = (long)videoId;
            var targetVideo = _cachedVideos.FirstOrDefault(x => x.Id == id);
            
            if (targetVideo == null)
            {
                Debug.LogError($"No video found with id {videoId}");
                onFail?.Invoke($"No video found with id {videoId}");
                return;
            }
            
            var videoBefore = _cachedVideos.TakeWhile(x => x.Id != id).TakeLast(takePreviousCount);
            var videAfter = _cachedVideos.SkipWhile(x => x.Id != id).Skip(1).Take(takeNextCount);
            var videos = videoBefore.Append(targetVideo).Concat(videAfter).ToArray();
            
            onSuccess?.Invoke(videos);
        }

        protected override Video[] OnBeforeVideosCallback(Video[] inputVideos)
        {
            return inputVideos;
        }
    }
}