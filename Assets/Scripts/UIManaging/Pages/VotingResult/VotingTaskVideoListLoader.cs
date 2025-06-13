using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.VideoServer;
using Navigation.Args;
using Navigation.Args.Feed;
using Navigation.Core;
using UIManaging.Common.Args.Views.LastLevelsPanelArgs;
using UIManaging.Pages.Common.VideoManagement;

namespace UIManaging.Pages.VotingResult
{
    using Extensions; // DWC-2022.2 upgrade: getting ambiguous warnings with this using statement so moving it here for scope
    internal sealed class VotingTaskVideoListLoader: BaseVideoListLoader
    {
        private readonly IReadOnlyCollection<Video> _votingTaskVideoList;
        private readonly long _taskId;
        private readonly IVotingBattleResultManager _votingBattleResultManager;

        public VotingTaskVideoListLoader(long taskId, IVotingBattleResultManager votingBattleResultManager, VideoManager videoManager, PageManager pageManager, IBridge bridge, long userGroupId, IReadOnlyCollection<Video> votingTaskVideoList) : base(videoManager, pageManager, bridge, userGroupId)
        {
            _taskId = taskId;
            _votingTaskVideoList = votingTaskVideoList;
            _votingBattleResultManager = votingBattleResultManager;
        }

        protected override Task<Video[]> DownloadModelsInternal(object targetId, int takeNext, int takePrevious = 0, CancellationToken token = default)
        {
            if (targetId is null)
            {
                return Task.FromResult(_votingTaskVideoList.Take(takeNext).ToArray());
            }

            var id = (long)targetId;
            var targetVideo = _votingTaskVideoList.First(x => x.Id == id);
            var videoBefore = _votingTaskVideoList.TakeWhile(x => x.Id != id).TakeLast(takePrevious);
            var videAfter = _votingTaskVideoList.SkipWhile(x => x.Id != id).Skip(1).Take(takeNext);
            var videos = videoBefore.Append(targetVideo).Concat(videAfter).ToArray();
            return Task.FromResult(videos);
        }

        protected override void OnVideoPreviewClicked(BaseLevelItemArgs args)
        {
            PageManager.MoveNext(PageId.Feed, new VotingResultFeedArgs(_votingBattleResultManager, VideoManager, args.Video.Id, _taskId));
        }
    }
}