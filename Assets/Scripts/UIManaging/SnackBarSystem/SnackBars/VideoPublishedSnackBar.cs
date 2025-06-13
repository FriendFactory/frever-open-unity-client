using System;
using System.Collections;
using System.IO;
using DG.Tweening;
using UIManaging.SnackBarSystem.Configurations;
using UnityEngine;
using UnityEngine.Networking;
using Zenject;

namespace UIManaging.SnackBarSystem.SnackBars
{
    internal sealed class VideoPublishedSnackBar : SnackBar<VideoPublishedSnackBarConfiguration>
    {
        private const string SAVE_PATH_DIRECTORY = "/SharedVideos";
        private const float LOADING_FADE_DURATION = 0.3f;
        
        [SerializeField] private GameObject _share;
        [SerializeField] private GameObject _loadingCircle;
        [SerializeField] private CanvasGroup _loadingCircleCanvasGroup;
        
        private readonly NativeShare _nativeShare = new NativeShare();
        private Action _onClick;
        private Action _onShareClick;
        
        private string _sharingUrl;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public override SnackBarType Type => SnackBarType.VideoPublished;
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnConfigure(VideoPublishedSnackBarConfiguration configuration)
        {
            _sharingUrl = configuration.SharingUrl;
            _onClick = configuration.OnClick;
            _share.SetActive(SharingIsPossible());
            _onShareClick = configuration.OnShareClick;
        }

        protected override void OnTap()
        {
            if (_onClick != null)
            {
                _onClick.Invoke();
                RequestHide();
                return;
            }
            
            if (SharingIsPossible())
            {
                _onShareClick?.Invoke();
                PrepareSharingVideo();
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //------------------------------------------s---------------------------

        private void PrepareSharingVideo()
        {
            ShowLoadingCircle();
            
            StartCoroutine(DownloadVideoRoutine());
        }
        
        private IEnumerator DownloadVideoRoutine()
        {
            var www = UnityWebRequest.Get(_sharingUrl);
            yield return www.SendWebRequest();

            var isError = (www.result == UnityWebRequest.Result.ConnectionError) || (www.result == UnityWebRequest.Result.ProtocolError);
            if (isError)
            {
                OnFailedVideoDownloading(www.error);
            }
            else
            {
                var saveDirectory = $"{Application.persistentDataPath}{SAVE_PATH_DIRECTORY}";
                if (!File.Exists(saveDirectory))
                {
                    Directory.CreateDirectory(saveDirectory);
                }

                var savePath = $"{saveDirectory}/video.mp4";
                File.WriteAllBytes(savePath, www.downloadHandler.data);
                ShareVideo(savePath);
            }

            HideLoadingCircle();
            RequestHide();
        }

        private void OnFailedVideoDownloading(string error)
        {
            Debug.LogError(error);
            RequestHide();
        }

        private void ShareVideo(string filePath)
        {
            _nativeShare.Clear();
            _nativeShare.SetText(string.Empty);
            _nativeShare.AddFile(filePath);
            _nativeShare.Share();
        }
        
        private void ShowLoadingCircle()
        {
            _loadingCircleCanvasGroup.alpha = 0f;
            _loadingCircle.SetActive(true);
            _loadingCircleCanvasGroup.DOKill();
            _loadingCircleCanvasGroup.DOFade(1f, LOADING_FADE_DURATION);
            _loadingCircleCanvasGroup.blocksRaycasts = true;
        }

        private void HideLoadingCircle()
        {
            _loadingCircleCanvasGroup.DOKill();
            _loadingCircleCanvasGroup
               .DOFade(0f, LOADING_FADE_DURATION)
               .OnComplete(() => _loadingCircle.SetActive(false));
            
            _loadingCircleCanvasGroup.blocksRaycasts = false;
        }

        private bool SharingIsPossible()
        {
            return !string.IsNullOrEmpty(_sharingUrl);
        }
    }
}
