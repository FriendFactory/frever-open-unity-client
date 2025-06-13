using System;
using System.Collections.Generic;
using Bridge.Models.Common.Files;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Common;
using UIManaging.SnackBarSystem;
using FileInfo = Bridge.Models.Common.Files.FileInfo;

namespace UIManaging.Pages.Common.SongOption.AudioRipper
{
    public sealed class AudioRipperUploader
    {
        private readonly IBridge _bridge;
        private readonly SnackBarHelper _snackBarHelper;

        public AudioRipperUploader(IBridge bridge, SnackBarHelper snackBarHelper)
        {
            _bridge = bridge;
            _snackBarHelper = snackBarHelper;
        }

        public void UploadSongToDB(float clipLength, string localUrl, string copyrightsCheckInfo, Action<UserSoundFullInfo> onCompleted)
        {
            var info = new System.IO.FileInfo(localUrl);
            UploadSongToStorage(clipLength, localUrl, info.Length, copyrightsCheckInfo, onCompleted);
        }

        private async void UploadSongToStorage(float clipLength, string localUrl, long size, string copyrightsCheckInfo, Action<UserSoundFullInfo> onCompleted)
        {
            var requestModel = new CreateUserSoundModel
            {
                Size = size,
                Duration = (int) (clipLength * 1000f),
                Files = new List<FileInfo> {new FileInfo(localUrl, FileType.MainFile)},
                CopyrightCheckResults = copyrightsCheckInfo
            };
            
            var result = await _bridge.CreateUserSoundAsync(requestModel);
            if (result.IsSuccess)
            {
                onCompleted?.Invoke(result.Model);
            }
            else
            {
                _snackBarHelper.ShowInformationSnackBar(Constants.ErrorMessage.WRONG_FILE_FORMAT);
            }
        }
    }
}
