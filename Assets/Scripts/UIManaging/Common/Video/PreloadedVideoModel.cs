using System;
using System.Collections;
using System.IO;
using Bridge.Models.VideoServer;
using Common;
using UIManaging.Common.Interfaces;
using UnityEngine;
using UnityEngine.Networking;

namespace UIManaging.Common
{
    public class PreloadedVideoModel: ILocalVideoModel
    {
        private const string SAVE_PATH_DIRECTORY = "PreloadedVideos";
        
        public event Action VideoStarted;
        public event Action VideoLoaded;
        public event Action<string> VideoError;
        
        public string LocalFilePath { get; private set; }
        public Video Video { get; }
        public string Url { get; }
        public bool IsStarted { get; private set; }
        public bool IsLoaded { get; private set; }

        private IEnumerator _downloadCoroutine;
        private UnityWebRequest _currentRequest;

        public PreloadedVideoModel(Video video, string url)
        {
            Video = video;
            Url = url;
        }
        
        public void LoadVideo()
        {
            if (IsLoaded || _downloadCoroutine != null)
            {
                return;
            }
            
            CoroutineSource.Instance.StartCoroutine(_downloadCoroutine = DownloadVideoCoroutine());
        }

        public void StartVideo()
        {
            if (!IsLoaded && IsStarted)
            {
                return;
            }

            IsStarted = true;
            VideoStarted?.Invoke();
        }

        public void CleanUp()
        {
            if (_downloadCoroutine != null)
            {
                CoroutineSource.Instance.StopCoroutine(_downloadCoroutine);
            }
            
            _currentRequest?.Abort();

            IsLoaded = false;
            IsStarted = false;

            if (LocalFilePath != null)
            {
                File.Delete(LocalFilePath);
                LocalFilePath = null;
            }

        }
        
        public void Error(string message)
        {
            VideoError?.Invoke(message);
        }
        
        private IEnumerator DownloadVideoCoroutine()
        {
            _currentRequest = UnityWebRequest.Get(Url);
            
            yield return _currentRequest.SendWebRequest();

            var isError = _currentRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError;
            
            if (isError)
            {
                Error(_currentRequest.error);
            }
            else
            {
                var saveDirectory = $"{Application.persistentDataPath}/{SAVE_PATH_DIRECTORY}";
                
                if (!File.Exists(saveDirectory))
                {
                    Directory.CreateDirectory(saveDirectory);
                }

                LocalFilePath = $"{saveDirectory}/video_{Video.Key}.mp4";
                
                File.WriteAllBytes(LocalFilePath, _currentRequest.downloadHandler.data);

                IsLoaded = true;
                VideoLoaded?.Invoke();
            }

            _downloadCoroutine = null;
            _currentRequest = null;
        }
    }
}