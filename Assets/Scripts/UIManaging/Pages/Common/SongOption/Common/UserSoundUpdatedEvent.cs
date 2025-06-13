using Bridge.Models.ClientServer.Assets;
using StansAssets.Foundation.Patterns;

namespace UIManaging.Pages.Common.SongOption.Common
{
    public class UserSoundUpdatedEvent: IEvent
    {
        public UserSoundFullInfo UserSoundFullInfo { get; }

        public UserSoundUpdatedEvent(UserSoundFullInfo userSoundFullInfo)
        {
            UserSoundFullInfo = userSoundFullInfo;
        }
    }
}