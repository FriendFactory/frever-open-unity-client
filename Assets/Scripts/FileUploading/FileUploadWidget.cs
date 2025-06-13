using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bridge.Models.Common.Files;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Common;
using Common.Collections;
using Modules.LocalStorage;
using Sirenix.OdinInspector;
using UIManaging.Pages.Common.NativeGalleryManagement;
using UIManaging.PopupSystem;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Zenject;
using FileInfo = Bridge.Models.Common.Files.FileInfo;
using MediaType = NativeGallery.MediaType;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.Uploading
{
    public sealed class FileUploadWidget : MonoBehaviour
    {
        public event Action MediaConversionStarted;
        public event Action<PhotoFullInfo> OnPhotoConversionSuccess;
        public event Action<VideoClipFullInfo> OnVideoConversionSuccess;
        public event Action<string> OnMediaConversionError;
        
        [SerializeField] private Button _uploadButton;
        [SerializeField] private bool _usePreview;
        [ShowIf("_usePreview")]
        [SerializeField] private RawImage _preview;
        [SerializeField] private bool _generateThumbnails;
        [ShowIf("_generateThumbnails")]
        [SerializeField] private ThumbnailResolutionMapping _thumbnailResolutionMapping 
            = new ThumbnailResolutionMapping
        {
            [Resolution._256x256] = new Vector2Int(256, 256),
            [Resolution._512x512] = new Vector2Int(512, 512),
        };

        [Inject] private IBridge _bridge;
        [Inject] private INativeGallery _nativeGallery;
        [Inject] private PopupManagerHelper _popupManagerHelper;

        private Vector2 VideoSourceSizeRangeInMb => Constants.SourceFileSizes.CONVERT_VIDEO_SOURCE_FILE_SIZE_RANGE_MB;
        private Vector2 ImageSourceSizeRangeInMb => Constants.SourceFileSizes.CONVERT_IMAGE_SOURCE_FILE_SIZE_RANGE_MB;
        public MediaType MediaType { get; set; }

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            _uploadButton.onClick.AddListener(OnButtonClicked);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void ShowGallery()
        {
            _nativeGallery.GetMixedMediaFromGallery(OnMediaSelected, MediaType);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnButtonClicked()
        {
            ShowGallery();
        }
        
        private void OnMediaSelected(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            
            var fileSize = GetFileSizeInMb(path);
            var mediaType = NativeGallery.GetMediaTypeOfFile(path);
            var sizeRange = mediaType == MediaType.Video ? VideoSourceSizeRangeInMb : ImageSourceSizeRangeInMb;
            
            if (fileSize > sizeRange.y)
            {
                _popupManagerHelper.OpenFileReachMaxSizeExceptionPopup(sizeRange.y, fileSize, null);
                return;
            }
            
            if (fileSize < sizeRange.x)
            {
                _popupManagerHelper.OpenFileReachMinSizeExceptionPopup(sizeRange.x, fileSize, null);
                return;
            }

            MediaConversionStarted?.Invoke();

            if (mediaType == MediaType.Video)
            {
                ConvertAndApplyVideo(path);
            }
            else
            {
                ConvertAndApplyPhoto(path);
            }
        }

        private float GetFileSizeInMb(string filePath)
        {
            return FileUtil.GetFileSizeInMb(filePath);
        }

        private async void ConvertAndApplyPhoto(string path)
        {
            var texture = await NativeGallery.LoadImageAtPathAsync(path, generateMipmaps: false, markTextureNonReadable: false);
            var stream = new MemoryStream(texture.EncodeToJPG());
            var result = await _bridge.ConvertAsync(stream, path, "jpg");
            
            if (!result.IsSuccess)
            {
                OnMediaConversionError?.Invoke(result.ErrorMessage);
                return;
            }

            if (!gameObject.activeInHierarchy) return;
            
            var files = new List<FileInfo>
            {
                new FileInfo(result.ConvertedFilePath, FileType.MainFile, result.Extension)
                {
                    Source = new FileSource { UploadId = result.UploadId }
                }
            };

            if (_generateThumbnails)
            {
                var thumbnails = GenerateThumbnails(texture);
                if (thumbnails != null)
                {
                    var thumbnailUploads = thumbnails.Select(UploadThumbnail);
                    await Task.WhenAll(thumbnailUploads);
                    files.AddRange(thumbnails);
                }
            }

            var userPhoto = new PhotoFullInfo
            {
                Id = LocalStorageManager.GetNextLocalId(nameof(PhotoFullInfo)),
                Files = files
            };
                
            if (_usePreview && _preview)
            {
                _preview.gameObject.SetActive(true);
                _preview.texture = LoadImage(result.ConvertedFilePath);
            }

            OnPhotoConversionSuccess?.Invoke(userPhoto);
        }

        private List<FileInfo> GenerateThumbnails(Texture2D texture)
        {
            return _generateThumbnails
                ? _thumbnailResolutionMapping?
                 .Select(resolution => new FileThumbnailCreator(resolution.Value, resolution.Key))
                 .Select(thumbnailCreator => thumbnailCreator.CreateThumbnail("tmpGenereatedThumbnail", texture))
                 .ToList()
                : null;
        }

        private async Task UploadThumbnail(FileInfo thumbnail)
        {
            var result = await _bridge.ConvertAsync(File.OpenRead(thumbnail.FilePath), thumbnail.FilePath, "png");

            if (result.IsError)
            {
                Debug.LogError($"Failed to upload thumbnail {thumbnail.Resolution}");
                return;
            }

            thumbnail.Source = new FileSource() { UploadId = result.UploadId };
            thumbnail.Extension = FileExtension.Png;
        }

        private async void ConvertAndApplyVideo(string path)
        {
            var fileExtension = Path.GetExtension(path).Replace(".", string.Empty);
            var result = await _bridge.ConvertAsync(File.OpenRead(path), path, fileExtension);

            if (!result.IsSuccess)
            {
                OnMediaConversionError?.Invoke(result.ErrorMessage);
                return;
            }
            
            if (!gameObject.activeInHierarchy) return;

            var properties = GetVideoProperties(result.ConvertedFilePath);
            var fileInfo = new FileInfo(result.ConvertedFilePath, FileType.MainFile)
            {
                Source = new FileSource()
                {
                    UploadId = result.UploadId
                }
            };

            var videoClip = new VideoClipFullInfo
            {
                Id = LocalStorageManager.GetNextLocalId(nameof(VideoClipFullInfo)),
                Files = new List<FileInfo> {fileInfo},
                Duration = (int)properties.duration
            };

            OnVideoConversionSuccess?.Invoke(videoClip);
        }

        private static NativeGallery.VideoProperties GetVideoProperties(string convertedPath)
        {
            #if !UNITY_EDITOR
            return NativeGallery.GetVideoProperties(convertedPath);
            #else
            //mock editor video properties
            return new NativeGallery.VideoProperties(886, 1920, 12000, 90);
            #endif
        }
        
        private static Texture2D LoadImage(string path)
        {
            if (File.Exists(path) == false)
            {
                Debug.LogError($"File doesn't exist: {path}");
                return null;
            }

            var fileData = File.ReadAllBytes(path);

            var tex = new Texture2D(2, 2);
            tex.LoadImage(fileData); // this will auto-resize the texture dimensions.

            return tex;
        }
        
        [Serializable] private class ThumbnailResolutionMapping : SerializedDictionary<Resolution, Vector2Int> { }
    }
}