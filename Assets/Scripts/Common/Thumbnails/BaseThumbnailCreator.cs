using System.IO;
using Bridge.Models.Common.Files;
using Extensions;
using Unity.Collections;
using UnityEngine;
using FileInfo = Bridge.Models.Common.Files.FileInfo;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace Common.Thumbnails
{
    public abstract class BaseThumbnailCreator
    {
        private readonly Resolution _thumbnailResolution;
        protected string _saveDirectoryPath;
        protected readonly Vector2Int ThumbnailsSize;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public Texture2D CapturedThumbnail { get; private set; }

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        protected BaseThumbnailCreator(Vector2Int thumbnailsSize, Resolution thumbnailResolution)
        {
            ThumbnailsSize = thumbnailsSize;
            _thumbnailResolution = thumbnailResolution;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public FileInfo CreateThumbnail(string fileName, Vector2 screenPosition, NativeArray<uint> screenshotPixels, Vector2Int screenshotResolution)
        {
            var textureSize = GetTextureSize(screenshotResolution);
            var textureOffset = GetTextureOffset(screenPosition, screenshotResolution) - new Vector2Int(textureSize.x / 2, textureSize.y);
            textureOffset = new Vector2Int(Mathf.Clamp(textureOffset.x, 0, textureSize.x), Mathf.Clamp(textureOffset.y, 0, textureSize.y));
            var amountOfPixels = textureSize.x * textureSize.y;
            var firstPixelIndex = textureOffset.y * textureSize.x + textureOffset.x;
            var lastPixelIndex = firstPixelIndex + amountOfPixels;

            if (lastPixelIndex >= screenshotPixels.Length)
            {
                firstPixelIndex = screenshotPixels.Length - amountOfPixels - 1;
            }

            CapturedThumbnail = new Texture2D(textureSize.x, textureSize.y, TextureFormat.ARGB32, false){hideFlags = HideFlags.HideAndDontSave};
            var texturePixels = screenshotPixels.GetSubArray(firstPixelIndex,  amountOfPixels);

            CapturedThumbnail.LoadRawTextureData(texturePixels);
            CapturedThumbnail.Apply();
            CapturedThumbnail = AfterTextureCreated(CapturedThumbnail);

            var thumbnailSavePath = SaveImageFile(fileName, CapturedThumbnail, _thumbnailResolution);
            return new FileInfo(thumbnailSavePath, FileType.Thumbnail, _thumbnailResolution);
        }
        
        public FileInfo CreateThumbnail(string fileName, Texture2D texture)
        {
            CapturedThumbnail = texture.ScaleAndCrop(ThumbnailsSize);

            var thumbnailSavePath = SaveImageFile(fileName, CapturedThumbnail, _thumbnailResolution);
            return new FileInfo(thumbnailSavePath, FileType.Thumbnail, _thumbnailResolution);
        }

        public void DestroyCapturedTexture()
        {
            if (CapturedThumbnail == null) return;
            Object.Destroy(CapturedThumbnail);
            CapturedThumbnail = null;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected abstract Vector2Int GetTextureSize(Vector2Int screenshotResolution);

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private Vector2Int GetTextureOffset(Vector2 screenPosition, Vector2Int screenshotResolution)
        {
            var xOffset = screenPosition.x * screenshotResolution.x;
            var yOffset = (1f - screenPosition.y + 0.1f) * screenshotResolution.y;
            return new Vector2Int((int)xOffset, (int)yOffset);
        }
        
        private Texture2D AfterTextureCreated(Texture2D inputTexture)
        {
            inputTexture.Scale(ThumbnailsSize.x, ThumbnailsSize.y, FilterMode.Bilinear);
            return inputTexture;
        }

        private string SaveImageFile(string fileName, Texture2D texture2D, Resolution resolution)
        {
            TryCreateSaveDirectory();
            var thumbnailSavePath = GetSavePathForThumbnail(fileName, resolution);
            var thumbnailBytes = texture2D.EncodeToPNG();

            using (var fs = File.Open(thumbnailSavePath, FileMode.OpenOrCreate))
            {
                fs.Write(thumbnailBytes, 0, thumbnailBytes.Length);
            }

            return thumbnailSavePath;
        }

        private string GetSavePathForThumbnail(string fileName, Resolution resolution)
        {
            var thumbnailFileName = $"{fileName}_{resolution}.png";
            return Path.Combine(_saveDirectoryPath, thumbnailFileName);
        }
        
        private void TryCreateSaveDirectory()
        {
            if(!Directory.Exists(_saveDirectoryPath))
            {
                Directory.CreateDirectory(_saveDirectoryPath);
            }
        }
    }
}