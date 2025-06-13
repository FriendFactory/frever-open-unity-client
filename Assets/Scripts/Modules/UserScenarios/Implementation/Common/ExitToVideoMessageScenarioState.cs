using System;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.LevelCreation;

namespace Modules.UserScenarios.Implementation.Common
{
    [UsedImplicitly]
    internal sealed class ExitToVideoMessageScenarioState: StateBase<ILevelCreationScenarioContext>, IResolvable
    {
        public override ScenarioState Type => ScenarioState.ExitToVideoMessageCreation;
        public override ITransition[] Transitions => Array.Empty<ITransition>();
        public override bool IsExitState => true;

        public override void Run()
        {
            var args = new VideoMessageOpenArgs();
            var messagePublishInfo = Context.PublishContext.VideoPublishSettings.MessagePublishInfo;
            if (messagePublishInfo != null)
            {
                args.ShareDestination = messagePublishInfo.ShareDestination;
            }
            args.ChatInfo = Context.OpenedFromChat;
            
            Context.ExecuteVideoMessageCreationScenario?.Invoke(args);
        }
    }
}