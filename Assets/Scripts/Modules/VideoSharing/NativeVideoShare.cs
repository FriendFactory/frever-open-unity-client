using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Modules.VideoSharing
{
    internal sealed class NativeVideoShare
    {
        private const string SAVE_PATH_DIRECTORY = "/SharedVideos";
        private const string VIDEO_FILE_NAME = "video.mp4";
        
        private readonly NativeShare _nativeShare = new();

        public async Task<NativeVideoShareResult> ShareVideoAsync(string sharingUrl)
        {
            NativeVideoShareResult result = null;
            var savePath = await DownloadVideoAsync(sharingUrl);
            if (string.IsNullOrEmpty(savePath))
            {
                return new NativeVideoShareResult($"Failed to download a video # url: {sharingUrl}");
            }
            
            ShareVideo(savePath);

            while (result == null)
            {
                await Task.Delay(42);
            }

            return result;

            void ShareVideo(string filePath)
            {
                _nativeShare.Clear();
                _nativeShare.SetText(string.Empty);
                _nativeShare.AddFile(filePath);
                _nativeShare.SetCallback((sharedResult, target) =>
                {
                    Debug.Log($"[{GetType().Name}] Native Share callback # shared result: {sharedResult}, target: {target}");
                    result = new NativeVideoShareResult(sharedResult);
                });
                _nativeShare.Share();
            }
        }

        public async Task<NativeVideoShareResult> ShareVideoLinkAsync(string personalizedVideoUrl)
        {
            var tcs = new TaskCompletionSource<NativeVideoShareResult>();

            _nativeShare.Clear();
            _nativeShare.SetUrl(personalizedVideoUrl);
            _nativeShare.SetCallback((sharedResult, target) =>
            {
                Debug.Log(
                    $"[{GetType().Name}] Native Share callback # shared result: {sharedResult}, target: {target}");
                tcs.SetResult(new NativeVideoShareResult(sharedResult));
            });
            _nativeShare.Share();

            return await tcs.Task;
        }
        
        private async Task<string> DownloadVideoAsync(string url)
        {
            using (var webRequest = UnityWebRequest.Get(url))
            {
                var operation = webRequest.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (webRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError($"Error: {webRequest.error}");
                    return null;
                }

                var saveDirectory = $"{Application.persistentDataPath}{SAVE_PATH_DIRECTORY}";
                if (!File.Exists(saveDirectory))
                {
                    Directory.CreateDirectory(saveDirectory);
                }

                var savePath = $"{saveDirectory}/{VIDEO_FILE_NAME}";
                await File.WriteAllBytesAsync(savePath, webRequest.downloadHandler.data);

                return savePath;
            }
        }
    }
}