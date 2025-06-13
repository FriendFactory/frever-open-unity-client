using System;
using Bridge.Models.ClientServer.Assets;

namespace UIManaging.Pages.Common.SongOption.Uploads
{
    public class SoundSettingsViewModel: MusicViewModel
    {
        public UserSoundFullInfo UserSoundFullInfo { get; }
        public Action OnNameChanged { get; } 

        public SoundSettingsViewModel(UserSoundFullInfo userSoundFullInfo, Action onNameChanged)
        {
            UserSoundFullInfo = userSoundFullInfo;
            OnNameChanged = onNameChanged;
        }
    }
}