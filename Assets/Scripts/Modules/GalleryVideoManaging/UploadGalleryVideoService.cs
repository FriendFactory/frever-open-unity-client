using System;
using System.IO;
using System.Linq;
using Extensions;
using JetBrains.Annotations;
using Navigation.Args;
using UIManaging.Pages.Common.NativeGalleryManagement;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;

namespace Modules.GalleryVideoManaging
{
    public interface IUploadGalleryVideoService
    {
        bool IsVideoToFeedAllowed { get; }
        void TryToOpenVideoGallery(Action<NonLeveVideoData> videoSelected, Action onClosed = null, Action onFailed = null);
    }
    
    [UsedImplicitly]
    internal sealed class UploadGalleryVideoService: IUploadGalleryVideoService
    {
        private const int MAX_VIDEO_DURATION = 60000;
        private const long MAX_FILE_SIZE_MB = 50;
        private const long MAX_FILE_SIZE_BYTES = MAX_FILE_SIZE_MB * 1024L * 1024L;

        private readonly string[] VIDEO_FORMATS = { ".mp4", ".mov" };
        
        private readonly INativeGallery _nativeGallery;
        private readonly PopupManager _popupManager;
        private readonly LocalUserDataHolder _localUser;

        private Action<NonLeveVideoData> _onVideoPicked;
        private Action _onVideoSelectionFailed;

        public bool IsVideoToFeedAllowed => _localUser.LevelingProgress.AllowVideoToFeed;

        public UploadGalleryVideoService(INativeGallery nativeGallery, PopupManager popupManager, LocalUserDataHolder localUser)
        {
            _nativeGallery = nativeGallery;
            _popupManager = popupManager;
            _localUser = localUser;
        }

        public void TryToOpenVideoGallery(Action<NonLeveVideoData> videoSelected, Action onClosed = null, Action onFail = null)
        {
            _onVideoPicked = videoSelected;
            _onVideoSelectionFailed = onFail;
            if (!IsVideoToFeedAllowed)
            {
                _popupManager.PushPopupToQueue(new AlertPopupConfiguration
                {
                    PopupType = PopupType.VideoToFeedLocked
                });
                OnVideoSelectionFailed();
                return;
            }

            _popupManager.PushPopupToQueue(new DialogPopupConfiguration
            {
                PopupType = PopupType.UploadVideoToFeedPopup,
                OnYes = OpenVideoGallery,
                OnClose = x =>
                {
                    _onVideoSelectionFailed?.Invoke();
                    onClosed?.Invoke();
                }
            });
        }

        private void OpenVideoGallery()
        {
            _nativeGallery.GetVideoFromGallery(OnVideoSelected);
        }

        private void OnVideoSelected(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            if (!Application.isEditor)
            {
                var fileInfo = new FileInfo(path);
                var videoInfo = _nativeGallery.GetVideoInfo(path);
                if (fileInfo.Length > MAX_FILE_SIZE_BYTES)
                {
                    DisplayFormatErrorPopup("Your file is too big", "Please make sure file is maximum 50 MB");
                    OnVideoSelectionFailed();
                    return;
                }

                if (!VIDEO_FORMATS.Contains(fileInfo.Extension, System.StringComparer.OrdinalIgnoreCase))
                {
                    DisplayFormatErrorPopup("Your file has wrong format",
                                            "Please make sure file format is .mp4 or .mov");
                    OnVideoSelectionFailed();
                    return;
                }

                if (videoInfo.duration > MAX_VIDEO_DURATION)
                {
                    DisplayFormatErrorPopup("Your file is too long", "Please make sure file is maximum 1 minute long");
                    OnVideoSelectionFailed();
                    return;
                }
            }

            var videoData = new NonLeveVideoData
            {
                Path = path,
                DurationSec = (int)_nativeGallery.GetVideoInfo(path).duration.ToSeconds()
            };

            _onVideoPicked?.Invoke(videoData);
        }

        private void OnVideoSelectionFailed()
        {
            _onVideoSelectionFailed?.Invoke();
        }

        private void DisplayFormatErrorPopup(string title, string message)
        {
            var config = new AlertPopupConfiguration
            {
                Title = title,
                Description = message,
                PopupType = PopupType.AlertPopup,
                ConfirmButtonText = "OK"
            };
            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(PopupType.AlertPopup);
        }
    }
}