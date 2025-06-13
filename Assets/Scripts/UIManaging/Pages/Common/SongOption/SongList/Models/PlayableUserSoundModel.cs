using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using UnityEngine;

namespace UIManaging.Pages.Common.SongOption.SongList
{
    public class PlayableUserSoundModel: PlayableItemModel
    {
        public UserSoundFullInfo UserSoundFullInfo { get; }

        public override long Id => UserSoundFullInfo.Id;
        public override string Title { get; }
        public override string ArtistName => "User Sound";
        public Texture Thumbnail { get; }


        public override IPlayableMusic Music => UserSoundFullInfo;

        public PlayableUserSoundModel(UserSoundFullInfo userSoundFullInfo, Texture thumbnail)
        {
            UserSoundFullInfo = userSoundFullInfo;
            Title = UserSoundFullInfo.Name;
            Thumbnail = thumbnail;
        }
    }
}