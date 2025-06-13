using Abstract;
using Bridge.Models.ClientServer.Assets;
using StansAssets.Foundation.Patterns;
using UIManaging.Pages.Common.SongOption.AudioRipper;
using UIManaging.Pages.Common.SongOption.Common;
using UnityEngine;

namespace UIManaging.Pages.Common.SongOption.Uploads
{
    internal sealed class UploadsOwnSoundsView: BaseContextDataView<UserSoundsListModel>
    {
        [SerializeField] private UserSoundDeployButton _deployButton;
        [SerializeField] private UserSoundsPanel _userSoundsPanel;
        
        protected override void OnInitialized()
        {
            _userSoundsPanel.Initialize(ContextData);
            
            _deployButton.SoundUploaded += AddUserSound;
            
            StaticBus<UserSoundUpdatedEvent>.Subscribe(OnUserSoundUpdated);
        }

        protected override void BeforeCleanup()
        {
            _deployButton.SoundUploaded -= AddUserSound;
            
            StaticBus<UserSoundUpdatedEvent>.Unsubscribe(OnUserSoundUpdated);
        }

        private void AddUserSound(UserSoundFullInfo userSound) => _userSoundsPanel.AddUserSound(userSound);
        
        private void OnUserSoundUpdated(UserSoundUpdatedEvent @event)
        {
            _userSoundsPanel.ReplaceOrAddUserSound(@event.UserSoundFullInfo);
        }
    }
}