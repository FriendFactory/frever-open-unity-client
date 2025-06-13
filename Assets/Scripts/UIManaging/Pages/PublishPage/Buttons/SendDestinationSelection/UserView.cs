using Bridge.Models.ClientServer;
using Bridge.Services.UserProfile;
using UIManaging.Common.Args.Views.Profile;
using UnityEngine;

namespace UIManaging.Pages.PublishPage.Buttons.SendDestinationSelection
{
    internal sealed class UserView : MonoBehaviour
    {
        [SerializeField] private UserPortraitView _userPortrait;

        public void Display(Profile profile)
        {
            var model = new UserPortraitModel
            {
                UserGroupId = profile.MainGroupId,
                UserMainCharacterId = profile.MainCharacter.Id,
                MainCharacterThumbnail = profile.MainCharacter.Files
            };
            _userPortrait.Initialize(model);
        }
        
        public void Display(GroupShortInfo group)
        {
            var userPortraitModel = new UserPortraitModel
            {
                UserGroupId = group.Id,
                UserMainCharacterId = group.MainCharacterId.Value,
                MainCharacterThumbnail = group.MainCharacterFiles
            };
            _userPortrait.Initialize(userPortraitModel);
        }
    }
}
