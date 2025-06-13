using System.Linq;
using Abstract;
using UIManaging.Common;
using UIManaging.Pages.Tasks.TaskVideosGridPage;
using UnityEngine;

namespace UIManaging.Pages.Tasks
{
    internal sealed class TaskVideoThumbnail: BaseContextDataView<TaskModel>
    {
        [SerializeField] private VideoListItem _videoThumbnail;
        
        protected override void OnInitialized()
        {
            if (ContextData.LevelPreviewArgs.Count > 0)
            {
                TryToLoadVideoThumbnail();
            }
            else
            {
                ContextData.NewPageAppended += TryToLoadVideoThumbnail;
                ContextData.DownloadFirstPage();
            }
        }

        protected override void BeforeCleanup()
        {
            ContextData.NewPageAppended -= TryToLoadVideoThumbnail;
            base.BeforeCleanup();
        }

        private void TryToLoadVideoThumbnail()
        {
            var videoModel = ContextData.LevelPreviewArgs?.FirstOrDefault();
            if (videoModel == null) return;
            _videoThumbnail.Initialize(videoModel);
        }
    }
}