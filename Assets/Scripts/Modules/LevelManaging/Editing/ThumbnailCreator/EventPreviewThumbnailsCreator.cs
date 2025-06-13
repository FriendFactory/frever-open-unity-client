using System.IO;
using Common.Thumbnails;
using UnityEngine;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace Modules.LevelManaging.Editing.ThumbnailCreator
{
    internal sealed class EventPreviewThumbnailsCreator : BaseThumbnailCreator
    {
        public EventPreviewThumbnailsCreator(Vector2Int thumbnailsSize, Resolution thumbnailResolution) : base(thumbnailsSize, thumbnailResolution)
        {
            _saveDirectoryPath = Path.Combine(Application.persistentDataPath, "EventThumbnails");
        }

        protected override Vector2Int GetTextureSize(Vector2Int screenshotResolution)
        {
            var aspectRatio = (float)ThumbnailsSize.y / ThumbnailsSize.x;
            var width = screenshotResolution.x;
            var height = width * aspectRatio;
            return new Vector2Int(width, (int)height);
        }
    }
}