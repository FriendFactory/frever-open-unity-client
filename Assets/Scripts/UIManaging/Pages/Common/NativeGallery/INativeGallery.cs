using System;
using UnityEngine;

namespace UIManaging.Pages.Common.NativeGalleryManagement
{
    public interface INativeGallery
    {
        void GetMixedMediaFromGallery(NativeGallery.MediaPickCallback callback, NativeGallery.MediaType mediaTypes, string title = "");
        void GetVideoFromGallery(NativeGallery.MediaPickCallback callback, string title = "");
        void SaveVideoWithCurrentDateToGallery(byte[] mediaBytes, string album, string filename, Action<bool, string> callback = null);
        void SaveVideoToGallery(byte[] mediaBytes, string album, string filename, Action<bool, string> callback = null);
        void SaveVideoToGallery(string existingMediaPath, string album, string filename, Action<bool, string> callback = null);
        Texture2D LoadImageAtPath(string imagePath, int maxSize = -1, bool markTextureNonReadable = true, bool generateMipmaps = true, bool linearColorSpace = false);
        Texture2D GetVideoThumbnail(string videoPath);
        NativeGallery.VideoProperties GetVideoInfo(string videoPath);
    }
}