using System.Collections.Generic;
using Bridge.Models.ClientServer;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using Bridge.Models.Common.Files;

namespace UIManaging.Pages.Common.SongOption.Common
{
    public class PlayableTrendingUserSound: IPlayableMusic
    {
        public UserSoundFullInfo UserSound => TrendingUserSound.UserSound; 
        public GroupShortInfo Owner => TrendingUserSound.Owner; 
        public int UsageCount => TrendingUserSound.UsageCount; 
        
        public long Id
        {
            get => TrendingUserSound.UserSound.Id;
            set => TrendingUserSound.UserSound.Id = value;
        }

        public List<FileInfo> Files
        {
            get => TrendingUserSound.UserSound.Files;
            set => TrendingUserSound.UserSound.Files = value;
        }

        public int Duration
        {
            get => TrendingUserSound.UserSound.Duration;
            set => TrendingUserSound.UserSound.Duration = value;
        }
        
        private TrendingUserSound TrendingUserSound { get; }

        public PlayableTrendingUserSound(TrendingUserSound trendingUserSound)
        {
            TrendingUserSound = trendingUserSound;
        }
    }
}