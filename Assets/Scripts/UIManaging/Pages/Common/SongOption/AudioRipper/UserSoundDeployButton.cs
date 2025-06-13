using System;
using System.Collections.Generic;
using System.IO;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Common;
using Extensions;
using Modules.Amplitude;
using JetBrains.Annotations;
using TMPro;
using UIManaging.PopupSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using UIManaging.Pages.Common.NativeGalleryManagement;
using UIManaging.Pages.Common.SongOption.Common;
using UIManaging.SnackBarSystem;
using Utils;

namespace UIManaging.Pages.Common.SongOption.AudioRipper
{
    internal sealed class UserSoundDeployButton : MonoBehaviour
    {
        private const string DEFAULT_TEXT = "Upload new sound";
        private const string UPLOADING_TEXT = "Uploading...";
        
        [SerializeField] private GameObject _loadingWheel;
        [SerializeField] private GameObject _uploadImage;
        [SerializeField] private TMP_Text _uploadText;

        private bool _isTranscoding;
        private IBridge _bridge;
        private Button _button;
        private AudioRipperUploader _audioUploader;
        private PopupManagerHelper _popupManagerHelper;
        private SnackBarHelper _snackBarHelper;
        private INativeGallery _nativeGallery;
        private AmplitudeManager _amplitudeManager;
        private MusicPlayerController _musicPlayerController;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private static float MaxFileSizeMb => Constants.SourceFileSizes.CONVERT_AUDIO_SOURCE_FILE_SIZE_RANGE_MB.y;
        private static float MinFileSizeMb => Constants.SourceFileSizes.CONVERT_AUDIO_SOURCE_FILE_SIZE_RANGE_MB.x;
        
        //---------------------------------------------------------------------
        // Events 
        //---------------------------------------------------------------------

        public event Action<UserSoundFullInfo> SoundUploaded;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject, UsedImplicitly]
        private void Construct(IBridge bridge, PopupManagerHelper popupManagerHelper, INativeGallery nativeGallery,
            SnackBarHelper snackBarHelper, AmplitudeManager amplitudeManager, MusicPlayerController musicPlayerController)
        {
            _bridge = bridge;
            _popupManagerHelper = popupManagerHelper;
            _nativeGallery = nativeGallery;
            _snackBarHelper = snackBarHelper;
            _amplitudeManager = amplitudeManager;
            _musicPlayerController = musicPlayerController;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(ChooseVideoFromLibrary);
            _audioUploader = new AudioRipperUploader(_bridge, _snackBarHelper);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void Display()
        {
            if (_isTranscoding) return;
            
            _button.interactable = true;
            _loadingWheel.SetActive(false);
            _uploadImage.SetActive(true);
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void ChooseVideoFromLibrary()
        {
            _musicPlayerController.Stop();
            DeleteFilesInDirectory();
            _nativeGallery.GetVideoFromGallery(OnMediaSelected);
        }

        private void OnMediaSelected(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            
            var fileSizeMb = FileUtil.GetFileSizeInMb(path);
            
            if (fileSizeMb > MaxFileSizeMb)
            {
                void OnPopupClosed(object x) => ResetUploadButton();
                _popupManagerHelper.OpenFileReachMaxSizeExceptionPopup(MaxFileSizeMb, fileSizeMb, OnPopupClosed);
                return;
            }
            
            if (fileSizeMb < MinFileSizeMb)
            {
                void OnPopupClosed(object x) => ResetUploadButton();
                _popupManagerHelper.OpenFileReachMinSizeExceptionPopup(MinFileSizeMb, fileSizeMb, OnPopupClosed);
                return;
            }
            
            TranscodeAudio(path);
        }

        private async void TranscodeAudio(string path)
        {
            using (var fs = File.OpenRead(path))
            {
                var bytes = ReadFully(fs);
                _isTranscoding = true;
                _button.interactable = false;
                _loadingWheel.SetActive(true);
                _uploadImage.SetActive(false);
                
                UpdateText(true);

                var videoDurationSec = (int)_nativeGallery.GetVideoInfo(path).duration.ToSeconds();
                var transcodeResult = await _bridge.ExtractAudioAsync(bytes, videoDurationSec);
                
                if (transcodeResult.IsSuccess)
                {
                    var isCopyRight = transcodeResult.CopyrightsCheckInfo != null;
                    SendCopyRightNoticeEvent(isCopyRight);
                    
                    if (isCopyRight)
                    {
                        ShowCopyrightMessage();
                    }
                    
                    AudioClipCallback(transcodeResult.AudioClip, transcodeResult.FilePath, transcodeResult.CopyrightsCheckInfo);
                }
                else
                {
                    OnTranscodingFailed(transcodeResult.ErrorMessage);
                    _amplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.SOUND_UPLOAD_FAILED);
                }
            }
        }

        private void OnTranscodingFailed(string errorMessage)
        {
            ResetUploadButton();
                 
            var isCopyRight = errorMessage.Contains(Constants.ErrorMessage.COPYRIGHT_ERROR_IDENTIFIER);
            SendCopyRightNoticeEvent(isCopyRight);
            
            if (isCopyRight)
            {
                ShowCopyrightMessage();
                return;
            }

            _snackBarHelper.ShowFailSnackBar("Your file could not be uploaded. Please try with another", 5);
        }

        private void ShowCopyrightMessage()
        {
            _popupManagerHelper.ShowCopyrightFailedPopup();
        }

        private byte[] ReadFully(Stream input)
        {
            var bufferSize = SizeUtil.ConvertMbToBytes(MaxFileSizeMb);
            var buffer = new byte[bufferSize];
            using (var memoryStream = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    memoryStream.Write(buffer, 0, read);
                }
                return memoryStream.ToArray();
            }
        }

        private void AudioClipCallback(AudioClip clip, string path, string copyrightsCheckInfo)
        {
            ResetUploadButton();
            
            _audioUploader.UploadSongToDB(clip.length, path, copyrightsCheckInfo, userSound =>
            {
                _snackBarHelper.ShowSuccessDarkSnackBar("Your sound is uploaded!", 2);
                SoundUploaded?.Invoke(userSound);
            });
        }

        private void ResetUploadButton()
        {
            _isTranscoding = false;
            _button.interactable = true;
            _loadingWheel.SetActive(false);
            _uploadImage.SetActive(true);
            
            UpdateText(false);
        }
        
        private void DeleteFilesInDirectory()
        {
            var pathToTranscodingResultDirectory = Application.persistentDataPath + "/Transcoding/Result/";
            
            if (Directory.Exists(pathToTranscodingResultDirectory))
            {
                foreach (var file in Directory.GetFiles(pathToTranscodingResultDirectory))
                {
                    File.Delete(file);
                }
            }
        }

        private void SendCopyRightNoticeEvent(bool isCopyRight)
        {
            var metaData = new Dictionary<string, object>
            {
                [AmplitudeEventConstants.EventProperties.COPYRIGHT_MATCH] = isCopyRight,
                [AmplitudeEventConstants.EventProperties.MEDIA_TYPE] = "Sound"
            };

            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.COPYRIGHT_NOTICE, metaData);
        }

        private void UpdateText(bool isUploading) => _uploadText.text = isUploading ? UPLOADING_TEXT : DEFAULT_TEXT;
    }
}
