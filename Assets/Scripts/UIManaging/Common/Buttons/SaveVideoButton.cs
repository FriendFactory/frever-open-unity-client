using System;
using System.Collections;
using System.Collections.Generic;
using Abstract;
using Bridge;
using DG.Tweening;
using Modules.Amplitude;
using UIManaging.Localization;
using UIManaging.Pages.Common.NativeGalleryManagement;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.Networking;
using Zenject;

namespace UIManaging.Common.Buttons
{
    public class SaveVideoButton : BaseContextDataButton<SaveVideoButtonArgs>
    {
        private const float LOADING_FADE_DURATION = 0.3f;
    
        [SerializeField] private GameObject _saveLoadingCircle;
        [SerializeField] private CanvasGroup _saveLoadingCanvasGroup;
    
        [Inject] private IBridge _bridge;
        [Inject] private AmplitudeManager _amplitudeManager;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private INativeGallery _nativeGallery;
        [Inject] private FeedLocalization _localization;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        protected bool IsSaving { get; set; }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
    
        protected override void OnInitialized()
        {
        }
    
        protected override async void OnUIInteracted()
        {
            if (IsSaving) return;

            IsSaving = true;
            
            var videoUrlResp = await _bridge.GetVideoFileUlr(ContextData.VideoId);
            if (videoUrlResp.IsError)
            {
                _snackBarHelper.ShowInformationSnackBar("Video was deleted or set to private. Can't share");
                IsSaving = false;
                return;
            }

            var saveMetaData = new Dictionary<string, object>(1)
            {
                [AmplitudeEventConstants.EventProperties.VIDEO_ID] = ContextData.VideoId
            };
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.SAVE_VIDEO, saveMetaData);

            StartCoroutine(DownloadVideoForSaving(videoUrlResp.Url));
        }
    
        private IEnumerator DownloadVideoForSaving(string videoFileUrl)
        {
            ShowLoadingCircle();

            yield return DownloadVideo(videoFileUrl, OnVideoLoaded, OnFail);

            void OnFail(string error)
            {
                VideoSaveCallback(false, string.Empty);
                Debug.Log(error);
            }
        }

        protected virtual void OnVideoLoaded(byte[] data)
        {
            _nativeGallery.SaveVideoWithCurrentDateToGallery(data, "Frever", "FreverVideo.mp4", VideoSaveCallback);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private IEnumerator DownloadVideo(string videoFileUrl ,Action<byte[]> onSuccess, Action<string> onFail)
        {
            var www = UnityWebRequest.Get(videoFileUrl);
            yield return www.SendWebRequest();

            if (www.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
            {
                onFail?.Invoke(www.error);
            }
            else
            {
                onSuccess?.Invoke(www.downloadHandler.data);
            }
        }

        private void VideoSaveCallback(bool success, string path)
        {
            IsSaving = false;
            
            var description = success 
                ? _localization.VideoSaveSuccessSnackbarMessage 
                : _localization.VideoSaveFailedSnackbarMessage ;
            _snackBarHelper.ShowInformationSnackBar(description);
            HideLoadingCircle();
        }
    
        protected void ShowLoadingCircle()
        {
            _saveLoadingCanvasGroup.alpha = 0f;
            _saveLoadingCircle.SetActive(true);
            _saveLoadingCanvasGroup.DOKill();
            _saveLoadingCanvasGroup.DOFade(1f, LOADING_FADE_DURATION);
            _saveLoadingCanvasGroup.blocksRaycasts = true;
        }

        protected void HideLoadingCircle()
        {
            _saveLoadingCanvasGroup.DOKill();
            _saveLoadingCanvasGroup.DOFade(0f, LOADING_FADE_DURATION)
                                   .OnComplete(() => _saveLoadingCircle.SetActive(false));
            _saveLoadingCanvasGroup.blocksRaycasts = false;
        }
    }
}