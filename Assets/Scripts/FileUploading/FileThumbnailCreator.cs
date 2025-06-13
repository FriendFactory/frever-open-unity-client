using System.IO;
using Common.Thumbnails;
using UnityEngine;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.Uploading
{
    public class FileThumbnailCreator : BaseThumbnailCreator
    {
        public FileThumbnailCreator(Vector2Int thumbnailsSize, Resolution thumbnailResolution) : base(thumbnailsSize, thumbnailResolution)
        {
            _saveDirectoryPath = Path.Combine(Application.persistentDataPath, "TmpFileThumbnails");
        }

        protected override Vector2Int GetTextureSize(Vector2Int screenshotResolution)
        {
            return ThumbnailsSize;
        }
    }
}