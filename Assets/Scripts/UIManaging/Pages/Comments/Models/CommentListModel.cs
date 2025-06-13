using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.VideoServer;
using Extensions;
using UIManaging.Common.Args.Views.LastLevelsPanelArgs;
using UIManaging.Common.Loaders;
using UIManaging.PopupSystem.Configurations;

namespace UIManaging.Pages.Comments
{
    internal class CommentListModel : GenericPaginationLoader<CommentInfo>
    {
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action<int> ReplyInserted;   
        public event Action RepliesLoaded;   
        public event Action<int> RepliesRemoved;
        
        //---------------------------------------------------------------------
        // Fields
        //---------------------------------------------------------------------

        private readonly long _videoId;
        private readonly List<object> _listItemModels = new List<object>();
        private readonly IVideoBridge _bridge;
        
        private readonly Dictionary<string, CommentItemModel> _rootCommentModels = new Dictionary<string, CommentItemModel>();
        private readonly Dictionary<string, CommentRepliesModel> _replyModels = new Dictionary<string, CommentRepliesModel>();
        private readonly Action<CommentItemModel> _onReply;
        private readonly Action<CommentItemModel> _onContextMenu;
        private readonly Action<CommentItemModel> _onLike;
        private readonly Action _onMovingToProfileStarted;
        private readonly Action _onMovingToProfileFinished;
        private readonly HashSet<string> _insertedCommentKeys = new HashSet<string>();

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public IReadOnlyList<object> ListItemModels => _listItemModels;
        protected override int DefaultPageSize => 10;
        
        protected override object LastLoadedItemId => Models?.Count > 0 ? Models[Models.Count-1].Key : null;
        protected override object FirstLoadedItemId => Models?.Count > 0 ? Models[0].Key : null;
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public CommentListModel(IVideoBridge bridge, long videoId, Action<CommentItemModel> onReply, Action<CommentItemModel> onContextMenu, Action<CommentItemModel> onLike, Action onMovingToProfileStarted, Action onMovingToProfileFinished)
        {
            _bridge = bridge;
            _videoId = videoId;
            _onReply = onReply;
            _onContextMenu = onContextMenu;
            _onLike = onLike;
            _onMovingToProfileStarted = onMovingToProfileStarted;
            _onMovingToProfileFinished = onMovingToProfileFinished;
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void RemoveRange(object loaderModel, int loadedCount)
        {
            var startIndex = _listItemModels.IndexOf(loaderModel) - loadedCount;
            _listItemModels.RemoveRange(startIndex, loadedCount);
            RepliesRemoved?.Invoke(startIndex);
        }
        
        public void InsertComment(CommentInfo commentInfo, bool isNewReply = false)
        {
            var insertionIndex = 0;
            
            if (commentInfo.IsRoot)
            {
                var commentModel = CreateCommentItemModel(commentInfo);
                _listItemModels.Insert(0, commentModel);
                var replyModel = CreateCommentRepliesModel(commentInfo);
                _listItemModels.Insert(1, replyModel);
            }
            else
            {
                var replyModel = _replyModels[commentInfo.Key.Split('.')[0]];
                replyModel.InsertReply(commentInfo, isNewReply);
                insertionIndex = _listItemModels.IndexOf(replyModel);
                _listItemModels.Insert(insertionIndex, CreateCommentItemModel(commentInfo));
            }
            
            ReplyInserted?.Invoke(insertionIndex);
        }

        public void InsertReplies(object loaderModel, IEnumerable<CommentInfo> models)
        {
            _listItemModels.InsertRange(_listItemModels.IndexOf(loaderModel), models.Select(CreateCommentItemModel));
            RepliesLoaded?.Invoke();
        }
        
        public async void LoadOnTop(string rootCommentKey, string recipientReplyKey, string targetReplyKey)
        {
            //check if root comment was loaded
            if (_rootCommentModels.TryGetValue(rootCommentKey, out var rootComment))
            {
                Models.Remove(rootComment.CommentInfo);
                Models.Insert(0, rootComment.CommentInfo);
                
                _listItemModels.Remove(rootComment);
                _listItemModels.Insert(0, rootComment);

                _listItemModels.Remove(_replyModels[rootCommentKey]);
                _listItemModels.Insert(1, _replyModels[rootCommentKey]);
            }
            else
            {
                InsertComment((await DownloadModelsInternal(rootCommentKey, 0))[0]);
            }
            
            _insertedCommentKeys.Add(rootCommentKey);
            var replyModel = _replyModels[rootCommentKey];
            
            if (recipientReplyKey != null)
            {
                var recipientReplyModel = await replyModel.DownloadReply(recipientReplyKey);
                InsertComment(recipientReplyModel);
            }
            
            if (targetReplyKey != null)
            {
                var targetReplyModel = await replyModel.DownloadReply(targetReplyKey);
                InsertComment(targetReplyModel);
            }
        }
        
        public int FindIndex(long replyToCommentId)
        {
            return _listItemModels.FindIndex(info => (info as CommentItemModel)?.Id == replyToCommentId);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        public override async void DownloadNextPage()
        {
            var loadedModels = new List<CommentInfo>();
            var pageSize = DefaultPageSize;
            var lastLoadedKey = LastLoadedItemId;
            if (lastLoadedKey != null) ++pageSize;

            do
            {
                var models = await DownloadModelsInternal(lastLoadedKey, pageSize);

                if (lastLoadedKey != null)
                {
                    models = models.Skip(1).ToArray();
                }
                if (models.Length == 0) break;
                
                var filteredModels = models.Where(model => !_insertedCommentKeys.Contains(model.Key)).ToArray();
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
        
        protected override void OnNextPageLoaded(CommentInfo[] page)
        {
            foreach (var commentInfo in page)
            {
                var commentModel = CreateCommentItemModel(commentInfo);
                _listItemModels.Add(commentModel);
                var replyModel = CreateCommentRepliesModel(commentInfo);
                _listItemModels.Add(replyModel);
            }
        }
        
        protected override void OnFirstPageLoaded(CommentInfo[] page)
        {
            
        }

        protected override async Task<CommentInfo[]> DownloadModelsInternal(object targetId, int takeNext, int takePrevious = 0, CancellationToken token = default)
        {
            var result = await _bridge.GetVideoRootComments(_videoId, (string)targetId, takeNext, takePrevious, token);
            return result.Models;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private CommentItemModel CreateCommentItemModel(CommentInfo commentInfo)
        {
            var commentModel = new CommentItemModel(commentInfo);
            commentModel.MovingToProfileStart += _onMovingToProfileStarted;
            commentModel.MovingToProfileFinished += _onMovingToProfileFinished;
            commentModel.OnReply += _onReply;
            commentModel.OnLongPress += _onContextMenu;
            commentModel.OnLike += _onLike;
            _rootCommentModels[commentInfo.Key] = commentModel;
            return commentModel;
        }

        private CommentRepliesModel CreateCommentRepliesModel(CommentInfo commentInfo)
        {
            var replyModel = new CommentRepliesModel(_bridge, this, commentInfo);
            replyModel.NewPageAppended += RepliesLoaded;
            _replyModels[commentInfo.Key] = replyModel;
            return replyModel;
        }
    }
}