using System.Collections.Generic;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.VideoServer;
using I2.Loc;

namespace Utils
{
    public static class AccessUtils
    {
        public static IReadOnlyDictionary<VideoAccess, string> VideoAccessLocalizationKeys =
            new Dictionary<VideoAccess, string>
            {
                [VideoAccess.Public] = "VIDEO_ACCESS_PUBLIC",
                [VideoAccess.ForFriends] = "VIDEO_ACCESS_FRIENDS",
                [VideoAccess.ForFollowers] = "VIDEO_ACCESS_FOLLOWERS",
                [VideoAccess.Private] = "VIDEO_ACCESS_PRIVATE",
                [VideoAccess.ForTaggedGroups] = "VIDEO_ACCESS_TAGGED"
            };
        
        public static IReadOnlyDictionary<CharacterAccess, string> CharacterAccessLocalizationKeys =
            new Dictionary<CharacterAccess, string>
            {
                [CharacterAccess.ForFriends] = "CHARACTER_ACCESS_FRIENDS",
                [CharacterAccess.Private] = "CHARACTER_ACCESS_PRIVATE",
            };

        public static string ToText(this VideoAccess access) => LocalizationManager.GetTranslation(VideoAccessLocalizationKeys[access]);

        public static string ToText(this CharacterAccess access) => LocalizationManager.GetTranslation(CharacterAccessLocalizationKeys[access]);
    }
}