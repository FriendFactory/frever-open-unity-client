using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.VideoServer;
using UIManaging.Common.Args.Views.LastLevelsPanelArgs;
using UIManaging.Common.Loaders;

namespace UIManaging.Pages.Comments
{
    public class CommentRepliesModel : GenericPaginationLoader<CommentInfo>
    {
        //---------------------------------------------------------------------
        // Fields
        //---------------------------------------------------------------------

        private readonly long _videoId;
        private readonly IVideoBridge _bridge;
        private readonly CommentListModel _listModel;
        private readonly string _rootKey;
        private readonly HashSet<long> _userAddedReplyIds = new HashSet<long>();
        
        private int _appendedCount;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public int FullCount { get; private set; }
        public int LoadedCount { get; private set; }
        public bool IsFullyLoaded => LoadedCount == FullCount;
        protected override int DefaultPageSize => 3;
        protected override object LastLoadedItemId => Models?.Count > _userAddedReplyIds.Count ? Models[Models.Count - _appendedCount - 1].Key : null;
        protected override object FirstLoadedItemId => Models?.Count > 0 ? Models[0].Key : null;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        internal CommentRepliesModel(IVideoBridge bridge, CommentListModel listModel, CommentInfo rootComment)
        {
            _videoId = rootComment.VideoId;
            _rootKey = rootComment.Key;
            FullCount = rootComment.ReplyCount;
            _bridge = bridge;
            _listModel = listModel;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override async void DownloadNextPage()
        {
            var loadedModels = new List<CommentInfo>();
            var pageSize = DefaultPageSize;
            var lastLoadedKey = LastLoadedItemId;
            if (lastLoadedKey != null) ++pageSize;
            _appendedCount = 0;

            do
            {
                var models = await DownloadModelsInternal(lastLoadedKey, pageSize);

                if (lastLoadedKey != null)
                {
                    models = models.Skip(1).ToArray();
                }
                if (models.Length == 0) break;
                
                var filteredModels = models.Where(model => !_userAddedReplyIds.Contains(model.Id)).ToArray();
                loadedModels.AddRange(filteredModels);
                pageSize -= filteredModels.Length;
                lastLoadedKey = models[models.Length - 1].Key;
            } 
            while (pageSize > 1);

            var page = loadedModels.ToArray();
            if (page.Length == 0)
            {
                InvokeLastPageLoaded();
                return;
            }
            Models.AddRange(page);
            OnNextPageLoaded(page);
            InvokeNewPageAppended();
        }

        public void LoadNext()
        {
            if (Models.Count > LoadedCount)
            {
                OnNextPageLoaded(Models.Skip(LoadedCount).Take(DefaultPageSize).ToArray());
                return;
            }
            
            DownloadNextPage();
        }

        public void Hide()
        {
            var loadedCount = LoadedCount;
            LoadedCount = 0;
            _listModel.RemoveRange(this, loadedCount);
        }

        public void InsertReply(CommentInfo commentInfo, bool isNew) 
        {
            if (Models.Count > _userAddedReplyIds.Count && FullCount > LoadedCount) ++_appendedCount;
            Models.Insert(LoadedCount, commentInfo);
            _userAddedReplyIds.Add(commentInfo.Id);
            ++LoadedCount;
            if (isNew) ++FullCount;
        }

        public async Task<CommentInfo> DownloadReply(string key)
        {
            var commentInfo = (await DownloadModelsInternal(key, 1))[0];
            return commentInfo;
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnNextPageLoaded(CommentInfo[] page)
        {
            LoadedCount += page.Length;
            _listModel.InsertReplies(this, page);
        }

        protected override void OnFirstPageLoaded(CommentInfo[] page)
        {
            
        }

        protected override async Task<CommentInfo[]> DownloadModelsInternal(object targetId, int takeNext, int takePrevious = 0, CancellationToken token = default)
        {
            return (await _bridge.GetVideoThreadComments(_videoId, _rootKey,(string)targetId, takePrevious, takeNext, token)).Models;
        }
    }
}