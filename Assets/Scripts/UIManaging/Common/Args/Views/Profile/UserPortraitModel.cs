using System.Collections.Generic;
using Bridge.Models.Common.Files;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Common.Args.Views.Profile
{
    public sealed class UserPortraitModel
    {
        public State UserState;
        public long UserGroupId;
        public long UserMainCharacterId;
        public List<FileInfo> MainCharacterThumbnail;
        public Resolution? Resolution;

        public override string ToString()
        {
            return $"{nameof(UserGroupId)}: {UserGroupId}";
        }

        public enum State
        {
            Available,
            Blocked,
            Missing
        }
    }
}