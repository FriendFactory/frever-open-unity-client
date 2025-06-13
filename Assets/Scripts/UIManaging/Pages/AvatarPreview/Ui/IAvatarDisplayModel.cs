using System;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.Assets;
using UMA.CharacterSystem;

namespace UIManaging.Pages.AvatarPreview.Ui {

    public interface IAvatarDisplayModel
    {
        event Action CloseRoomRequested;

        DynamicCharacterAvatar Avatar { get; }
        long GenderId { get; }

        Task<DynamicCharacterAvatar> GetAvatar();
        Task<BodyAnimationInfo> GetIdleBodyAnimation();
        Task<BodyAnimationInfo> GetWaveBodyAnimation();
    }
}