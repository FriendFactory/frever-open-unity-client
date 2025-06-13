using Bridge.Models.ClientServer;
using Bridge.Models.Common.Files;

namespace UIManaging.Common.Args.Views.Profile
{
    public static class UserPortraitViewExtensions
    {
        public static void InitializeFromGroupInfo(this UserPortraitView view, GroupShortInfo groupInfo)
        {
            if (groupInfo?.MainCharacterId == null) return;
            
            var userPortraitModel = new UserPortraitModel
            {
                Resolution = Resolution._128x128,
                UserGroupId = groupInfo.Id,
                UserMainCharacterId = groupInfo.MainCharacterId ?? 0,
                MainCharacterThumbnail = groupInfo.MainCharacterFiles
            };
            
            view.Initialize(userPortraitModel);
        }
        
        public static void InitializeFromProfile(this UserPortraitView view, Bridge.Services.UserProfile.Profile profile)
        {
            if (profile?.MainCharacter == null) return;

            var userPortraitModel = new UserPortraitModel
            {
                Resolution = Resolution._128x128,
                UserGroupId = profile.MainGroupId,
                UserMainCharacterId = profile.MainCharacter.Id,
                MainCharacterThumbnail = profile.MainCharacter.Files,
            };
            
            view.Initialize(userPortraitModel);
        }
    }
}