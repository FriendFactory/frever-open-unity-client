using System;
using System.Collections.Generic;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsManaging;
using Modules.AssetsManaging.LoadArgs;
using Event = Models.Event;

namespace Modules.LevelManaging.Editing.LevelPreview
{
    [UsedImplicitly]
    internal sealed class EventUserSoundLoader : DefaultLoader<UserSoundFullInfo, UserSoundLoadArgs>
    {
        public EventUserSoundLoader(IAssetManager assetManager) : base(assetManager)
        {
        }

        public override DbModelType Type => DbModelType.UserSound;
        
        protected override UserSoundLoadArgs[] Args => new[] {new UserSoundLoadArgs {DeactivateOnLoad = true}};

        protected override ICollection<UserSoundFullInfo> ExtractAssetModels(Event @event)
        {
            var userSound = @event.GetUserSound();
            return userSound == null ? Array.Empty<UserSoundFullInfo>() : new []{userSound}; 
        }
    }
}
