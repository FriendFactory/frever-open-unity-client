using System;
using System.IO;
using JetBrains.Annotations;
using UnityEngine;
using UnityEditor;

namespace UIManaging.Pages.Common.NativeGalleryManagement
{
    [UsedImplicitly]
    internal sealed class EditorNativeGallery: INativeGallery, INativeGalleryPermissionsHelper
    {
        private const string SAVE_PATH_DIRECTORY = "/SavedVideos";
        private static readonly string[] IMAGE_FILES = {"fake_photo", "fake_photo2"};
        private static int _lastUsedImage = -1;
 
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void GetMixedMediaFromGallery(NativeGallery.MediaPickCallback callback, NativeGallery.MediaType mediaTypes, string title = "")
        {
            switch (mediaTypes)
            {
                case NativeGallery.MediaType.Image:
                    if (ShowPhotoSelectionDialog()) callback?.Invoke(GetMockPhotoPath());
                    return;
                
                case NativeGallery.MediaType.Video:
                    if (ShowVideoSelectionDialog()) callback?.Invoke(GetMockVideoPath());
                    return;
                
                case NativeGallery.MediaType.Image | NativeGallery.MediaType.Video:
                    switch (ShowPhotoVideoSelectionDialog())
                    {
                        case 0: // Photo
                            callback?.Invoke(GetMockPhotoPath());
                            return;
                        case 1: // Video
                            callback?.Invoke(GetMockVideoPath());
                            return;
                    }
                    return;
                case NativeGallery.MediaType.Audio:
                default:
                    throw new ArgumentOutOfRangeException(nameof(mediaTypes), mediaTypes, null);
            }
        }

        public void GetVideoFromGallery(NativeGallery.MediaPickCallback callback, string title = "")
        {
            if (ShowVideoSelectionDialog()) callback?.Invoke(GetMockVideoPath());
        }

        public void SaveVideoToGallery(string existingMediaPath, string album, string filename, Action onSuccess = null)
        {
            // throw new NotImplementedException();
        }

        public void SaveVideoWithCurrentDateToGallery(byte[] mediaBytes, string album, string filename, Action<bool, string> callback = null)
        {
            SaveVideoToGallery(mediaBytes, album, filename, callback);
        }

        public void SaveVideoToGallery(byte[] mediaBytes, string album, string filename, Action<bool, string> callback = null)
        {
            var saveDirectory = $"{Application.persistentDataPath}{SAVE_PATH_DIRECTORY}";
            if (!File.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }

            var savePath = $"{saveDirectory}/{filename}";
            File.WriteAllBytes(savePath, mediaBytes);

            callback?.Invoke(true, savePath);
        }

        public void SaveVideoToGallery(string existingMediaPath, string album, string filename,
                                       Action<bool, string> callback = null)
        {
            Debug.Log($"[{GetType().Name}] Emulating success saving of video # {existingMediaPath}");
            callback?.Invoke(true, existingMediaPath);
        }

        public Texture2D LoadImageAtPath(string imagePath, int maxSize = -1, bool markTextureNonReadable = true, bool generateMipmaps = true, bool linearColorSpace = false)
        {
            return NativeGallery.LoadImageAtPath(imagePath, maxSize, markTextureNonReadable, generateMipmaps, linearColorSpace);
        }

        public Texture2D GetVideoThumbnail(string videoPath)
        {
            return NativeGallery.GetVideoThumbnail(videoPath);
        }

        public NativeGallery.VideoProperties GetVideoInfo(string videoPath)
        {
            return NativeGallery.GetVideoProperties(videoPath);
        }

        public bool IsPermissionGranted(NativeGallery.PermissionType permissionType, NativeGallery.MediaType mediaType)
        {
            return true;
        }

        public void RequestPermission(NativeGallery.PermissionType permissionType, Action onSuccess = null, Action onFail = null) { }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private static string GetMockVideoPath()
        {
            return Application.dataPath + "/Editor/FakeAssets/fake_video.mp4";
        }

        private static string GetMockPhotoPath()
        {
            var fileName = GetNextImageName();
            return Application.dataPath + $"/Editor/FakeAssets/{fileName}.jpg";
        }

        private static string GetNextImageName()
        {
            var nextImage = _lastUsedImage + 1;
            if (nextImage >= IMAGE_FILES.Length) nextImage = 0;
            _lastUsedImage = nextImage;
            return IMAGE_FILES[nextImage];
        }

        private static bool ShowPhotoSelectionDialog()
        {
            return EditorUtility.DisplayDialog
            (
                "Fake Photo Gallery",
                "Select fake photo",
                "Photo", "Cancel"
            );
        }

        private static bool ShowVideoSelectionDialog()
        {
            return EditorUtility.DisplayDialog
            (
                "Fake Video Gallery",
                "Select fake video",
                "Video", "Cancel"
            );
        }

        private static int ShowPhotoVideoSelectionDialog()
        {
            return EditorUtility.DisplayDialogComplex
            (
                "Fake Photo/Video Gallery",
                "Select fake photo or video",
                "Photo", "Video", "Cancel"
            );
        }
    }
}